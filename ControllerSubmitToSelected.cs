using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControllerSubmitToSelected : MonoBehaviour
{
    public bool useLeftHand = true;
    public enum Confirm { PrimaryButton, Trigger }
    public Confirm confirm = Confirm.PrimaryButton;   // 왼손 X 기본
    public bool alsoKeyboardEnter = true;

    InputAction submitAction, kbEnter;

    void OnEnable()
    {
        string hand = useLeftHand ? "{LeftHand}" : "{RightHand}";
        string path = (confirm == Confirm.PrimaryButton)
            ? $"<XRController>{hand}/primaryButton"
            : $"<XRController>{hand}/triggerPressed";

        submitAction = new InputAction(type: InputActionType.Button, binding: path);
        submitAction.performed += SendSubmitToSelected;
        submitAction.Enable();

        if (alsoKeyboardEnter)
        {
            kbEnter = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/enter");
            kbEnter.performed += SendSubmitToSelected;
            kbEnter.Enable();
        }
    }

    void OnDisable()
    {
        if (submitAction != null) { submitAction.performed -= SendSubmitToSelected; submitAction.Disable(); }
        if (kbEnter != null) { kbEnter.performed -= SendSubmitToSelected; kbEnter.Disable(); }
    }

    void SendSubmitToSelected(InputAction.CallbackContext _)
    {
        var go = EventSystem.current?.currentSelectedGameObject;
        if (!go) return;
        // 버튼의 OnSubmit(눌림 연출 + onClick)까지 트리거
        ExecuteEvents.Execute(go, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }
}
