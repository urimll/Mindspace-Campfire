// HapticsUtility.cs (fixed)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // Gamepad rumble (새 Input System)
#endif

public enum HapticCue { InhaleStart, Hold, ExhaleStart, SessionEnd }

public static class HapticsUtility
{
    public static bool debugLog = true;

    // 코루틴 러너
    class Runner : MonoBehaviour { }
    static Runner _runner;
    static Runner RunnerInstance
    {
        get
        {
            if (_runner == null)
            {
                var go = new GameObject("~HapticsRunner");
                Object.DontDestroyOnLoad(go);
                _runner = go.AddComponent<Runner>();
            }
            return _runner;
        }
    }

    // 외부 API
    public static void BreathCue(HapticCue cue)
    {
        switch (cue)
        {
            case HapticCue.InhaleStart: Pulse(0.6f, 0.08f); break;
            case HapticCue.Hold: Pulse(0.25f, 0.05f); break;
            case HapticCue.ExhaleStart: Pulse(0.4f, 0.06f); break;
            case HapticCue.SessionEnd: DoublePulse(0.9f, 0.18f, 0.08f); break;
        }
    }

    public static void Pulse(float amplitude, float duration)
    {
        TryXRPulse(amplitude, duration);

#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
            RunnerInstance.StartCoroutine(GamepadPulseRoutine(amplitude, duration));
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (duration >= 0.07f) Handheld.Vibrate();
#endif

        if (debugLog) Debug.Log($"[Haptics] Pulse amp={amplitude:0.00} dur={duration:0.00}s");
    }

    public static void DoublePulse(float amplitude, float eachDuration, float gap)
    {
        TryXRPulse(amplitude, eachDuration);
        RunnerInstance.StartCoroutine(DelayRoutine(gap, () => TryXRPulse(amplitude, eachDuration)));

#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
            RunnerInstance.StartCoroutine(GamepadDoublePulseRoutine(amplitude, eachDuration, gap));
#endif

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        RunnerInstance.StartCoroutine(DelayRoutine(gap, Handheld.Vibrate));
#endif

        if (debugLog) Debug.Log("[Haptics] DoublePulse");
    }

    // ---- XR 진동 (이 부분이 핵심: XR 타입을 완전히 수식해서 충돌 제거) ----
    static void TryXRPulse(float amplitude, float duration)
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(devices);

        for (int i = 0; i < devices.Count; i++)
        {
            var dev = devices[i];
            if (dev.TryGetHapticCapabilities(out UnityEngine.XR.HapticCapabilities caps) && caps.supportsImpulse)
            {
                dev.SendHapticImpulse(0u, Mathf.Clamp01(amplitude), duration);
            }
        }
    }
    // --------------------------------------------------------------------

#if ENABLE_INPUT_SYSTEM
    static IEnumerator GamepadPulseRoutine(float amplitude, float duration)
    {
        var pad = Gamepad.current;
        if (pad == null) yield break;
        pad.SetMotorSpeeds(amplitude, amplitude);
        yield return new WaitForSeconds(duration);
        pad.SetMotorSpeeds(0, 0);
    }

    static IEnumerator GamepadDoublePulseRoutine(float amplitude, float eachDuration, float gap)
    {
        var pad = Gamepad.current;
        if (pad == null) yield break;
        pad.SetMotorSpeeds(amplitude, amplitude);
        yield return new WaitForSeconds(eachDuration);
        pad.SetMotorSpeeds(0, 0);
        yield return new WaitForSeconds(gap);
        pad.SetMotorSpeeds(amplitude, amplitude);
        yield return new WaitForSeconds(eachDuration);
        pad.SetMotorSpeeds(0, 0);
    }
#endif

    static IEnumerator DelayRoutine(float sec, System.Action act)
    {
        yield return new WaitForSeconds(sec);
        act?.Invoke();
    }
}
