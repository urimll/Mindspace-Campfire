using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BreathToggleController : MonoBehaviour
{
    [Header("이 버튼과 라벨 (비워두면 자동 찾기)")]
    public Button selfButton;
    public TMP_Text label;

    [Header("라벨 문구")]
    public string labelWhenIdle = "심호흡하기";
    public string labelWhenRunning = "닫기";

    [Header("시작 상태 / 실행/정지 이벤트(선택)")]
    public bool startIdle = true;
    public UnityEvent onStart; // 심호흡 시작 로직 연결
    public UnityEvent onStop;  // 심호흡 종료 로직 연결

    [Header("실행 중 숨길 오브젝트(선택)")]
    public GameObject[] hideWhileRunning;

    bool running;

    void Awake()
    {
        if (!selfButton) selfButton = GetComponent<Button>();
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
        SetState(!startIdle, false); // 시작 라벨 세팅
    }

    void OnEnable()
    {
        if (selfButton) { selfButton.onClick.AddListener(Toggle); }
    }

    void OnDisable()
    {
        if (selfButton) { selfButton.onClick.RemoveListener(Toggle); }
    }

    public void Toggle() => SetState(!running, true);

    public void SetState(bool run, bool invokeEvents)
    {
        running = run;
        if (label) label.text = running ? labelWhenRunning : labelWhenIdle;

        if (hideWhileRunning != null)
            foreach (var go in hideWhileRunning) if (go) go.SetActive(!running);

        if (invokeEvents)
        {
            if (running) onStart?.Invoke();
            else onStop?.Invoke();
        }
    }

    // 외부에서 강제 종료하고 싶을 때 호출 가능
    public void Stop() => SetState(false, true);
}
