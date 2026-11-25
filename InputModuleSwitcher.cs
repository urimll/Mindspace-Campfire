using UnityEngine;
using UnityEngine.XR.Management; // XR 관리를 위해 필요합니다.

public class InputModuleSwitcher : MonoBehaviour
{
    // Inspector 창에서 두 컨트롤러 오브젝트를 연결해줄 변수
    public GameObject pcControllerObject; // BreatheController를 연결할 곳
    public GameObject vrControllerObject; // LEFT HAND CONTROLLER를 연결할 곳

    void Awake()
    {
        // VR 기기가 활성화되어 있는지 확인
        bool isVREnabled = XRGeneralSettings.Instance != null &&
                           XRGeneralSettings.Instance.Manager.isInitializationComplete;

        if (isVREnabled)
        {
            // VR 모드일 경우: VR 컨트롤러는 켜고, PC 컨트롤러는 끈다.
            Debug.Log("VR 모드가 활성화되었습니다. VR 입력 모듈을 사용합니다.");
            if (pcControllerObject != null) pcControllerObject.SetActive(false);
            if (vrControllerObject != null) vrControllerObject.SetActive(true);
        }
        else
        {
            // PC(일반) 모드일 경우: PC 컨트롤러는 켜고, VR 컨트롤러는 끈다.
            Debug.Log("VR 모드가 비활성화되었습니다. PC 입력 모듈을 사용합니다.");
            if (pcControllerObject != null) pcControllerObject.SetActive(true);
            if (vrControllerObject != null) vrControllerObject.SetActive(false);
        }
    }
}