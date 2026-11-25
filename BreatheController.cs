using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BreatheController : MonoBehaviour
{
    // Refs
    public GameObject overlayPanel;
    public RectTransform petalContainer;
    public Sprite petalSprite;
    public TextMeshProUGUI label;
    public TextMeshProUGUI timerText;

    // ▶ 버튼 & 라벨 (글자 토글)
    public Button startButton;                 // ← 가능하면 인스펙터에 직접 드래그!
    public TextMeshProUGUI startButtonText;    // 버튼의 Text(TMP)
    public string startLabel = "심호흡하기";
    public string stopLabel = "멈추기";

    // Session
    public int sessionSeconds = 60;
    public bool autoCloseOnFinish = true;

    // Calming pattern (4-2-6)
    public float inhaleSeconds = 4f;
    public float holdAfterInhale = 2f;
    public float exhaleSeconds = 6f;
    public float holdAfterExhale = 0f;

    // Petal look
    [Range(6, 16)] public int petalCount = 10;
    public float petalBaseSize = 240f;
    [Range(0.6f, 1.2f)] public float petalAspect = 0.88f;
    public float petalAlphaMin = 0.14f, petalAlphaMax = 0.36f;

    // Colors
    [Range(0f, 1f)] public float baseHue = 0.55f;
    [Range(0f, 0.25f)] public float hueSpread = 0.08f;
    [Range(0f, 1f)] public float sat = 0.65f, val = 1f;
    public bool useHueGradient = true;

    // Animation
    public float groupScaleMin = 0.86f, groupScaleMax = 1.26f;
    public float petalScaleMin = 0.52f, petalScaleMax = 1.10f;
    public float spinDegrees = 22f;
    public AnimationCurve inhaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve exhaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool waveOffset = true;
    public float waveAmount = 0.08f;
    public float waveSpeed = 1.0f;
    public float petalOffsetMin = 0f;
    public float petalOffsetMax = 58f;

    float sessionEnd;
    Coroutine routine;

    struct Petal { public RectTransform rt; public Image img; public float baseAngle; public float phase; }
    readonly List<Petal> petals = new();

    // ---------- Helpers ----------
    void EnsureStartButtonRefs()
    {
        // 1) 인스펙터에서 넣어준 게 최우선
        if (startButton != null && startButtonText == null)
            startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>(true);

        // 2) 이름으로 찾기
        if (startButton == null)
        {
            var sb = GameObject.Find("StartButton");
            if (sb) startButton = sb.GetComponent<Button>();
        }
        if (startButtonText == null && startButton != null)
            startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>(true);

        // 3) 마지막 안전장치: 씬의 모든 Button 훑어서 Text(TMP) 가진 첫 번째 것
        if (startButtonText == null)
        {
            foreach (var btn in GameObject.FindObjectsOfType<Button>(true))
            {
                var tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    startButton ??= btn;
                    startButtonText = tmp;
                    break;
                }
            }
        }

        if (startButtonText == null)
            Debug.LogWarning("[BreatheController] startButtonText를 찾지 못했습니다. 인스펙터에서 직접 할당하세요.");
    }

    void SetStartLabel(bool running)
    {
        EnsureStartButtonRefs();
        if (startButtonText != null)
            startButtonText.text = running ? stopLabel : startLabel;
    }
    // -----------------------------

    void Awake()
    {
        EnsureStartButtonRefs(); // 최대한 빨리 훅업
    }

    void Start()
    {
        if (overlayPanel) overlayPanel.SetActive(false);
        SetupPetals();
        SetStartLabel(false);
    }

    // ▶ 버튼 OnClick: 토글
    public void StartSessionFromButton()
    {
        if (routine != null)
        {
            CancelSession();
            SetStartLabel(false);
            return;
        }

        SetupPetals();
        routine = StartCoroutine(SessionRoutine());
        SetStartLabel(true);
    }

    public void CancelSession()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        if (overlayPanel) overlayPanel.SetActive(false);
        if (label) label.text = "";
        if (timerText) timerText.text = "";
        SetStartLabel(false);
    }

    void SetupPetals()
    {
        if (!petalContainer || petalSprite == null) return;

        for (int i = petalContainer.childCount - 1; i >= 0; i--)
            Destroy(petalContainer.GetChild(i).gameObject);
        petals.Clear();

        for (int i = 0; i < petalCount; i++)
        {
            var go = new GameObject($"Petal_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(petalContainer, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(petalBaseSize * petalAspect, petalBaseSize);
            float angle = (360f / petalCount) * i;
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            var img = go.GetComponent<Image>();
            img.sprite = petalSprite;
            img.raycastTarget = false;

            Color c = Color.white;
            if (useHueGradient)
            {
                float h = Mathf.Repeat(baseHue + Mathf.Lerp(-hueSpread, hueSpread, i / (petalCount - 1f)), 1f);
                c = Color.HSVToRGB(h, sat, val);
            }
            c.a = petalAlphaMin;
            img.color = c;

            petals.Add(new Petal { rt = rt, img = img, baseAngle = angle, phase = waveOffset ? (i / (float)petalCount) : 0f });
        }

        petalContainer.localScale = Vector3.one * groupScaleMin;
    }

    IEnumerator SessionRoutine()
    {
        if (!overlayPanel || petals.Count == 0) yield break;
        overlayPanel.SetActive(true);
        sessionEnd = Time.time + sessionSeconds;

        float baseRot = 0f;

        while (Time.time < sessionEnd)
        {
            UpdateTimer();

            // 들숨
            if (label) label.text = "들이마시기";
            HapticsUtility.BreathCue(HapticCue.InhaleStart);
            yield return AnimatePhase(inhaleSeconds, groupScaleMin, groupScaleMax, petalAlphaMin, petalAlphaMax, inhaleCurve,
                                      baseRot, baseRot + spinDegrees);
            baseRot += spinDegrees;

            // 멈춤
            if (holdAfterInhale > 0f)
            {
                if (label) label.text = "잠시 멈추기";
                HapticsUtility.BreathCue(HapticCue.Hold);
                yield return Hold(holdAfterInhale);
            }

            // 날숨
            if (label) label.text = "내쉬기";
            HapticsUtility.BreathCue(HapticCue.ExhaleStart);
            yield return AnimatePhase(exhaleSeconds, groupScaleMax, groupScaleMin, petalAlphaMax, petalAlphaMin, exhaleCurve,
                                      baseRot, baseRot - spinDegrees);
            baseRot -= spinDegrees;

            if (holdAfterExhale > 0f)
            {
                if (label) label.text = "잠시 멈추기";
                yield return Hold(holdAfterExhale);
            }
        }

        if (label) label.text = "잘했어요!";
        if (timerText) timerText.text = "00:00";
        yield return new WaitForSeconds(1f);

        if (autoCloseOnFinish) overlayPanel.SetActive(false);
        routine = null;
        HapticsUtility.BreathCue(HapticCue.SessionEnd);
        SetStartLabel(false);
    }

    IEnumerator AnimatePhase(
        float dur, float fromScale, float toScale, float fromA, float toA, AnimationCurve curve,
        float petalsRotateStart, float petalsRotateEnd)
    {
        if (dur <= 0f) yield break;

        bool shrinking = toScale < fromScale;
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float e = curve.Evaluate(k);

            float gs = Mathf.LerpUnclamped(fromScale, toScale, e);
            petalContainer.localScale = Vector3.one * gs;

            float rot = Mathf.Lerp(petalsRotateStart, petalsRotateEnd, e);
            float a = Mathf.LerpUnclamped(fromA, toA, e);

            float phase01 = shrinking ? (1f - e) : e;
            float radialOffset = Mathf.LerpUnclamped(petalOffsetMin, petalOffsetMax, phase01);

            UpdatePetals(phase01, rot, a, radialOffset);
            UpdateTimer();
            yield return null;
        }

        float finalPhase = (toScale < fromScale) ? 0f : 1f;
        float finalOffset = (toScale < fromScale) ? petalOffsetMin : petalOffsetMax;

        petalContainer.localScale = Vector3.one * toScale;
        UpdatePetals(finalPhase, petalsRotateEnd, toA, finalOffset);
    }

    void UpdatePetals(float phase01, float rotationDeg, float alpha, float radialOffset)
    {
        if (petals.Count == 0) return;

        float timeWave = waveOffset ? (Time.time * waveSpeed) : 0f;

        for (int i = 0; i < petals.Count; i++)
        {
            var p = petals[i];

            float ph = phase01;
            if (waveOffset)
                ph = Mathf.Clamp01(phase01 + Mathf.Sin((p.phase * Mathf.PI * 2f) + timeWave) * waveAmount);

            float ps = Mathf.LerpUnclamped(petalScaleMin, petalScaleMax, ph);
            p.rt.localScale = new Vector3(ps, ps, 1f);

            float angle = p.baseAngle + rotationDeg;
            p.rt.localRotation = Quaternion.Euler(0, 0, angle);

            float rad = angle * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * radialOffset;
            p.rt.anchoredPosition = pos;

            var c = p.img.color;
            c.a = alpha;
            p.img.color = c;
        }
    }

    IEnumerator Hold(float seconds)
    {
        float end = Time.time + seconds;
        while (Time.time < end)
        {
            UpdateTimer();
            yield return null;
        }
    }

    void UpdateTimer()
    {
        if (!timerText) return;
        float remain = Mathf.Max(0f, sessionEnd - Time.time);
        int sec = Mathf.CeilToInt(remain);
        int mm = sec / 60;
        int ss = sec % 60;
        timerText.text = $"{mm:00}:{ss:00}";
    }
}
