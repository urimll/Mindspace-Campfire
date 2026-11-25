using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CloseButtonHook : MonoBehaviour, ISubmitHandler
{
    public BackgroundMenuController controller; // BackgroundUI에 있는 그 컴포넌트 드래그

    Button btn;
    void Awake() { btn = GetComponent<Button>(); }

    void OnEnable()
    {
        if (btn != null)
        {
            btn.onClick.RemoveListener(Toggle);
            btn.onClick.AddListener(Toggle);
        }
    }
    void OnDisable()
    {
        if (btn != null) btn.onClick.RemoveListener(Toggle);
    }

    public void OnSubmit(BaseEventData _) { Toggle(); } // 트리거/엔터 등 Submit도 처리

    void Toggle() { controller?.Toggle(); }
}
