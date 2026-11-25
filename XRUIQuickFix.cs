#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

public static class XRUIQuickFix
{
    [MenuItem("Tools/XR UI/Convert Selected Canvases to World Space")]
    public static void ConvertSelectedCanvases()
    {
        var cam = Camera.main;
        foreach (var obj in Selection.gameObjects)
        {
            var canvas = obj.GetComponentInChildren<Canvas>(true);
            if (!canvas) continue;

            Undo.RecordObject(canvas, "Canvas to World Space");
            canvas.renderMode = RenderMode.WorldSpace;

            // 너무 크면 VR 스케일로 축소
            if (canvas.transform.localScale.magnitude > 1f)
                canvas.transform.localScale = Vector3.one * 0.001f;

            // XR용 레이캐스터 보장
            if (!canvas.TryGetComponent<TrackedDeviceGraphicRaycaster>(out _))
                Undo.AddComponent<TrackedDeviceGraphicRaycaster>(canvas.gameObject);

            // 카메라 앞 1.5m로 배치(편의)
            if (cam)
            {
                canvas.transform.position = cam.transform.position + cam.transform.forward * 1.5f;
                canvas.transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
            }
        }

        EditorUtility.DisplayDialog("XR UI", "선택한 캔버스를 World Space로 전환했어요.", "OK");
    }

    [MenuItem("Tools/XR UI/Fix EventSystem for XR")]
    public static void FixEventSystem()
    {
        var systems = Object.FindObjectsOfType<EventSystem>(true);
        if (systems.Length == 0)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
            systems = new[] { go.GetComponent<EventSystem>() };
        }

        // 첫 번째만 남기고 나머지 삭제
        for (int i = 1; i < systems.Length; i++)
            Undo.DestroyObjectImmediate(systems[i].gameObject);

        var es = systems[0];

        // XR 전용 모듈만 남기기
        foreach (var m in es.GetComponents<BaseInputModule>())
            if (!(m is XRUIInputModule)) Undo.DestroyObjectImmediate(m);

        if (!es.TryGetComponent<XRUIInputModule>(out _))
            Undo.AddComponent<XRUIInputModule>(es.gameObject);

        EditorUtility.DisplayDialog("XR UI", "EventSystem을 XR UI용으로 정리했어요.", "OK");
    }
}
#endif
