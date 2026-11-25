using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LeftHandButtonRunner : MonoBehaviour
{
    [Header("눌릴 UI 버튼 (심호흡하기)")]
    public Button targetButton;

    [Header("메뉴가 열려 있을 땐 입력 무시")]
    public GameObject menuRoot;  // ← BackgroundMenu 드래그

    [Header("에디터 테스트용(스페이스 키 허용)")]
    public bool alsoBindKeyboardSpace = true;

    private InputAction leftPrimary;   // 왼손 X (primaryButton)
    private InputAction spaceKey;

    void Awake()
    {
        leftPrimary = new InputAction(
            type: InputActionType.Button,
            binding: "<XRController>{LeftHand}/primaryButton");

        if (alsoBindKeyboardSpace)
            spaceKey = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
    }

    void OnEnable()
    {
        leftPrimary.performed += OnPressed;
        leftPrimary.Enable();

        if (alsoBindKeyboardSpace)
        {
            spaceKey.performed += OnPressed;
            spaceKey.Enable();
        }
    }

    void OnDisable()
    {
        leftPrimary.performed -= OnPressed;
        leftPrimary.Disable();

        if (alsoBindKeyboardSpace)
        {
            spaceKey.performed -= OnPressed;
            spaceKey.Disable();
        }
    }

    private void OnPressed(InputAction.CallbackContext _)
    {
        // 메뉴가 켜져 있으면 심호흡 실행을 막음
        if (menuRoot != null && menuRoot.activeSelf) return;

        if (targetButton != null)
            targetButton.onClick.Invoke();
    }
}
