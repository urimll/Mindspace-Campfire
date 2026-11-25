using UnityEngine;

// 이 스크립트는 World Space 캔버스를 카메라 시야에 꽉 차게 만들어줍니다.
// 캔버스 오브젝트에 이 스크립트를 붙여넣고, 캔버스는 메인 카메라의 자식으로 두어야 합니다.
[RequireComponent(typeof(RectTransform))]
public class CanvasFitter : MonoBehaviour
{
    // 인스펙터 창에서 설정할 카메라와의 거리
    public float distanceFromCamera = 1.5f;

    private RectTransform rectTransform;
    private Camera mainCamera;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = transform.parent.GetComponent<Camera>(); // 부모 오브젝트에서 카메라를 찾습니다.

        if (mainCamera == null)
        {
            Debug.LogError("CanvasFitter: 캔버스의 부모 오브젝트에 Camera 컴포넌트가 없습니다!");
            return;
        }

        // 캔버스를 카메라 정면에 위치시킵니다.
        transform.localPosition = new Vector3(0, 0, distanceFromCamera);
        transform.localRotation = Quaternion.identity; // 회전값 초기화

        // 카메라의 시야각과 거리를 이용해 캔버스의 크기를 계산합니다.
        float height = 2.0f * distanceFromCamera * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * mainCamera.aspect;

        // 계산된 크기를 캔버스의 RectTransform에 적용합니다.
        rectTransform.sizeDelta = new Vector2(width, height);

        // World Space UI는 Scale이 매우 작아야 제대로 보입니다. (중요)
        rectTransform.localScale = Vector3.one;
    }
}