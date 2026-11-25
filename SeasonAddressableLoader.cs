using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.XR.CoreUtils; // XROrigin API


 public enum Season { Spring = 0, Summer = 1, Autumn = 2, Winter = 3 }

public class SeasonAddressableLoader : MonoBehaviour
{
    [Header("Addressable 프리팹 (봄/가을/겨울)")]
    public AssetReferenceGameObject springRef;
    public AssetReferenceGameObject autumnRef;
    public AssetReferenceGameObject winterRef;

    [Header("씬에 이미 있는 '여름'(코어) 루트")]
    public GameObject sceneSummerRoot;

    [Header("Addressable 인스턴스 부모(SeasonRoot)")]
    public Transform parent; // (0,0,0) / rot=identity / scale=1 유지 권장

    [Header("Core 공유 객체 (SeasonRoot 밖!)")]
    public Transform coreCampfire;   // 불+장작 최상위
    public Transform corePlayerRig;  // XR Origin 루트

    [Header("시즌 변경 후 자동 닫을 메뉴(선택)")]
    public BackgroundMenuController menuToClose;

    [Header("여름 복귀 옵션")]
    public bool resetSummerToInitialCamera = true;
    public bool restoreCampfireOnSummer = true;

    // ─ 내부 상태 ─
    GameObject currentInstance;
    bool usingSceneSummer;
    bool hasHandle;
    AsyncOperationHandle<GameObject> lastHandle;

    // 여름 처음 자리 캐시
    bool summerPoseCaptured;
    Vector3 summerCamPos, summerCamFwd;
    Vector3 summerCampfirePos;
    Quaternion summerCampfireRot;

    void Start()
    {
        // SeasonRoot 확보 및 정렬
        if (!parent)
        {
            var go = GameObject.Find("SeasonRoot") ?? new GameObject("SeasonRoot");
            parent = go.transform;
        }
        parent.position = Vector3.zero;
        parent.rotation = Quaternion.identity;
        parent.localScale = Vector3.one;

        // 시작은 코어 시즌(여름)
        if (sceneSummerRoot)
        {
            sceneSummerRoot.SetActive(true);
            currentInstance = sceneSummerRoot;
            usingSceneSummer = true;

            CaptureInitialSummerPose();                 // 여름 시작 자리 저장
            StartCoroutine(SnapToAnchorsOn(currentInstance)); // 앵커 없으면 no-op
        }
    }

    // 버튼 연결용
    public void SetSeason(int i) => SetSeason((Season)i);
    public void SetSeason(Season season) => StartCoroutine(Swap(season));
    public void SetSpringButton() => SetSeason(Season.Spring);
    public void SetSummerButton() => SetSeason(Season.Summer);
    public void SetAutumnButton() => SetSeason(Season.Autumn);
    public void SetWinterButton() => SetSeason(Season.Winter);

    // ─────────────────────────────────────────────

    // 현재 시즌 완전 정리(봄 잔상 방지 하드 클린)
    void ClearCurrent()
    {
        // 코어(여름)는 끄기만
        if (usingSceneSummer && sceneSummerRoot)
            sceneSummerRoot.SetActive(false);

        // Addressable 인스턴스 해제 (핸들이 있든 없든 둘 다 시도)
        if (!usingSceneSummer && currentInstance)
        {
            if (hasHandle) Addressables.Release(lastHandle);
            Addressables.ReleaseInstance(currentInstance);
        }

        currentInstance = null;
        hasHandle = false;

        // ★ SeasonRoot 밑 잔여 자식 전부 제거 (봄이 남는 현상 차단)
        if (parent)
        {
            for (int i = parent.childCount - 1; i >= 0; --i)
                Destroy(parent.GetChild(i).gameObject);
        }
    }

    IEnumerator Swap(Season season)
    {
        ClearCurrent();

        // ─ 여름(코어)로 복귀 ─
        if (season == Season.Summer && sceneSummerRoot)
        {
            sceneSummerRoot.SetActive(true);
            currentInstance = sceneSummerRoot;
            usingSceneSummer = true;

            if (resetSummerToInitialCamera) ResetPlayerToSummerPose();
            if (restoreCampfireOnSummer) ResetCampfireToSummerPose();

            DynamicGI.UpdateEnvironment();
            if (menuToClose) menuToClose.Close();
            yield break;
        }

        // ─ Addressable(봄/가을/겨울) ─
        usingSceneSummer = false;

        AssetReferenceGameObject aref =
            (season == Season.Spring) ? springRef :
            (season == Season.Autumn) ? autumnRef :
            (season == Season.Winter) ? winterRef : null;

        if (aref == null || !aref.RuntimeKeyIsValid())
        {
            Debug.LogError($"[SeasonLoader] 해당 시즌 프리팹이 비어있어요: {season}");
            yield break;
        }

        // 인스턴스 생성
        lastHandle = Addressables.InstantiateAsync(aref, parent);
        hasHandle = true;
        yield return lastHandle;

        if (lastHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[SeasonLoader] 로드 실패: {aref}");
            hasHandle = false;
            yield break;
        }

        currentInstance = lastHandle.Result;

        // parent 기준 정렬 (오프셋/스케일 꼬임 방지)
        var t = currentInstance.transform;
        t.SetParent(parent, worldPositionStays: false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        SanitizeSeasonInstance(currentInstance);                       // 중복 시스템 무력화
        yield return StartCoroutine(SnapToAnchorsOn(currentInstance)); // 앵커로 스냅
        DynamicGI.UpdateEnvironment();

        if (menuToClose) menuToClose.Close();
    }

    // ─ 유틸리티 ─
    IEnumerator SnapToAnchorsOn(GameObject root)
    {
        var anchors = root.GetComponentInChildren<SeasonAnchors>(true);
        if (!anchors) yield break;

        if (anchors.campfire && coreCampfire)
            yield return StartCoroutine(TeleportCampfireSafely(coreCampfire, anchors.campfire));

        if (anchors.playerStart && corePlayerRig)
            SnapPlayerRig(anchors.playerStart);
    }

    void SnapPlayerRig(Transform anchor)
    {
        if (!anchor || !corePlayerRig) return;

        var xr = corePlayerRig.GetComponent<XROrigin>();
        var cc = corePlayerRig.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        if (xr)
        {
            xr.MoveCameraToWorldLocation(anchor.position);
            xr.MatchOriginUpCameraForward(Vector3.up, anchor.forward);
        }
        else
        {
            var e = corePlayerRig.eulerAngles; e.y = anchor.eulerAngles.y;
            corePlayerRig.SetPositionAndRotation(anchor.position, Quaternion.Euler(e));
        }

        if (cc) cc.enabled = true;
    }

    IEnumerator TeleportCampfireSafely(Transform fireRoot, Transform anchor)
    {
        if (!fireRoot || !anchor) yield break;

        var rbs = fireRoot.GetComponentsInChildren<Rigidbody>(true);
        var prevKin = new bool[rbs.Length];
        for (int i = 0; i < rbs.Length; i++)
        {
            prevKin[i] = rbs[i].isKinematic;
            rbs[i].isKinematic = true;
            rbs[i].velocity = Vector3.zero;
            rbs[i].angularVelocity = Vector3.zero;
        }

        fireRoot.gameObject.SetActive(true);
        fireRoot.SetPositionAndRotation(anchor.position, anchor.rotation);
        Physics.SyncTransforms();
        yield return null; // 한 프레임 안정화

        for (int i = 0; i < rbs.Length; i++)
            rbs[i].isKinematic = prevKin[i];
    }

    void SanitizeSeasonInstance(GameObject root)
    {
        foreach (var c in root.GetComponentsInChildren<Camera>(true)) c.enabled = false;
        foreach (var c in root.GetComponentsInChildren<AudioListener>(true)) c.enabled = false;
        foreach (var c in root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true)) c.enabled = false;
        foreach (var c in root.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>(true)) c.enabled = false;
        foreach (var c in root.GetComponentsInChildren<XROrigin>(true)) c.gameObject.SetActive(false);
    }

    // ─ 여름 시작 포즈 저장/복구 ─
    void CaptureInitialSummerPose()
    {
        if (summerPoseCaptured) return;

        if (coreCampfire)
        {
            summerCampfirePos = coreCampfire.position;
            summerCampfireRot = coreCampfire.rotation;
        }

        var xr = corePlayerRig ? corePlayerRig.GetComponent<XROrigin>() : null;
        if (xr && xr.Camera)
        {
            summerCamPos = xr.Camera.transform.position;
            var fwd = xr.Camera.transform.forward;
            summerCamFwd = Vector3.ProjectOnPlane(fwd, Vector3.up).normalized;
        }
        else if (corePlayerRig)
        {
            summerCamPos = corePlayerRig.position;
            summerCamFwd = Vector3.ProjectOnPlane(corePlayerRig.forward, Vector3.up).normalized;
        }

        summerPoseCaptured = true;
    }

    void ResetPlayerToSummerPose()
    {
        if (!summerPoseCaptured || !corePlayerRig) return;

        var xr = corePlayerRig.GetComponent<XROrigin>();
        var cc = corePlayerRig.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        if (xr)
        {
            xr.MoveCameraToWorldLocation(summerCamPos);
            xr.MatchOriginUpCameraForward(Vector3.up, summerCamFwd);
        }
        else
        {
            float yaw = Quaternion.LookRotation(summerCamFwd, Vector3.up).eulerAngles.y;
            var e = corePlayerRig.eulerAngles; e.y = yaw;
            corePlayerRig.SetPositionAndRotation(summerCamPos, Quaternion.Euler(e));
        }

        if (cc) cc.enabled = true;
    }

    void ResetCampfireToSummerPose()
    {
        if (!coreCampfire) return;
        coreCampfire.SetPositionAndRotation(summerCampfirePos, summerCampfireRot);
        Physics.SyncTransforms();
    }

    void OnDestroy()
    {
        if (hasHandle) Addressables.Release(lastHandle);
    }
}
