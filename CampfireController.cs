using UnityEngine;

public class CampfireController : MonoBehaviour
{
    [Header("스케일 대상과 기준점")]
    public Transform flameScaleTarget;   // 크기만 바꿀 오브젝트(불 비주얼 루트)
    public Transform baseAnchor;         // 바닥 기준점(FlameScaleTarget의 자식으로, 불 바닥 위치에 둬)

    [Header("라이트/사운드(선택)")]
    public Light fireLight;
    public AudioSource fireAudio;

    [Header("연료 세팅")]
    public float maxFuel = 100f;
    public float burnRatePerSec = 3f;
    public float fuel = 60f;

    [Header("세기 → 스케일/라이트/사운드 매핑")]
    public float minFlameScale = 0.2f;
    public float maxFlameScale = 1.0f;
    public float minLight = 0f;
    public float maxLight = 2.5f;
    public float minVolume = 0f;
    public float maxVolume = 0.6f;

    void Update()
    {
        if (fuel > 0f)
        {
            fuel = Mathf.Max(0f, fuel - burnRatePerSec * Time.deltaTime);
            ApplyIntensity();
        }
    }

    public void AddFuel(float amount)
    {
        fuel = Mathf.Clamp(fuel + amount, 0, maxFuel);
        ApplyIntensity();
    }

    void ApplyIntensity()
    {
        float t = (maxFuel <= 0f) ? 0f : Mathf.Clamp01(fuel / maxFuel);

        // ★ 바닥 기준점(world pos) 보존하면서 스케일
        if (flameScaleTarget)
        {
            Vector3 before = baseAnchor ? baseAnchor.position : flameScaleTarget.position;

            float s = Mathf.Lerp(minFlameScale, maxFlameScale, t);
            flameScaleTarget.localScale = Vector3.one * s;

            if (baseAnchor)
            {
                Vector3 delta = before - baseAnchor.position;
                flameScaleTarget.position += delta;  // 기준점 되돌리기
            }
        }

        if (fireLight) fireLight.intensity = Mathf.Lerp(minLight, maxLight, t);
        if (fireAudio)
        {
            fireAudio.volume = Mathf.Lerp(minVolume, maxVolume, t);
            fireAudio.pitch = 0.9f + 0.3f * t;
        }
    }
}
