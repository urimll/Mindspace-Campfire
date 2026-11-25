using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class BackgroundMenuController : MonoBehaviour
{
    [Header("열고 닫을 메뉴 루트")]
    public GameObject menuRoot;

    [Header("메뉴 열릴 때 포커스 줄 버튼")]
    public Button defaultButton;

    [Header("메뉴 열릴 때 숨길 UI (심호흡 버튼 등)")]
    public GameObject[] hideWhenOpen;

    [Header("토글 버튼(배경설정)과 라벨")]
    public Button toggleButton;            // Canvas/BackgroundButton
    public TMP_Text toggleButtonLabel;     // Canvas/BackgroundButton/Text (TMP)
    public string labelWhenClosed = "배경설정";
    public string labelWhenOpen = "닫기";

    [Header("여는 입력(왼손 Y) / 에디터 T")]
    public bool useLeftHand = true;
    public bool alsoKeyboardT = true;

    [Header("메뉴 열릴 때 잠시 꺼둘 스크립트들(선택)")]
    public Behaviour[] disableScriptsWhenOpen; // LeftHandButtonRunner 등

    private InputAction toggleAction, kbToggle;
    private bool _isOpen = false;

    void Awake()
    {
        // 라벨 자동 바인딩(누락 대비)
        if (!toggleButtonLabel && toggleButton)
            toggleButtonLabel = toggleButton.GetComponentInChildren<TMP_Text>(true);

        ApplyOpenState(false, setFocus: false); // 시작은 닫힘 기준으로 정리
    }

    void OnEnable()
    {
        if (toggleButton)
        {
            toggleButton.onClick.RemoveListener(Toggle);
            toggleButton.onClick.AddListener(Toggle);
        }

        string hand = useLeftHand ? "{LeftHand}" : "{RightHand}";
        toggleAction = new InputAction(type: InputActionType.Button,
                         binding: $"<XRController>{hand}/secondaryButton"); // Y
        toggleAction.performed += _ => Toggle();
        toggleAction.Enable();

        if (alsoKeyboardT)
        {
            kbToggle = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/t");
            kbToggle.performed += _ => Toggle();
            kbToggle.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleAction != null) { toggleAction.performed -= _ => Toggle(); toggleAction.Disable(); }
        if (kbToggle != null) { kbToggle.performed -= _ => Toggle(); kbToggle.Disable(); }
        if (toggleButton) toggleButton.onClick.RemoveListener(Toggle);
    }

    // 외부에서도 부를 수 있게 공개 API
    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);
    public void Toggle() => SetOpen(!_isOpen);

    public void SetOpen(bool open) => ApplyOpenState(open, setFocus: true);

    private void ApplyOpenState(bool open, bool setFocus)
    {
        _isOpen = open;

        if (menuRoot) menuRoot.SetActive(open);

        if (hideWhenOpen != null)
            foreach (var go in hideWhenOpen) if (go) go.SetActive(!open);

        if (disableScriptsWhenOpen != null)
            foreach (var b in disableScriptsWhenOpen) if (b) b.enabled = !open;

        if (toggleButtonLabel)
            toggleButtonLabel.text = open ? labelWhenOpen : labelWhenClosed;

        if (setFocus)
        {
            if (open && defaultButton)
                EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
            else if (!open && toggleButton)
                EventSystem.current.SetSelectedGameObject(toggleButton.gameObject);
        }
    }
}
