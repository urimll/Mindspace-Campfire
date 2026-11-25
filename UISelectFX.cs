using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UISelectFX : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerDownHandler
{
    public float selectedScale = 1.06f;   // 선택 시 살짝 커짐
    public float pressedScale = 0.97f;   // 눌렀을 때 살짝 눌림
    public Color selectedOutline = new Color(1, 1, 1, 0.8f); // 선택 시 외곽선

    Outline outline;
    Vector3 baseScale;
    Coroutine scaleCo;

    void Awake()
    {
        baseScale = transform.localScale;

        outline = GetComponent<Outline>();
        if (!outline) outline = gameObject.AddComponent<Outline>();
        outline.effectDistance = new Vector2(6f, -6f);
        var c = selectedOutline; c.a = 0f; outline.effectColor = c; // 기본은 보이지 않게
    }

    public void OnSelect(BaseEventData e)
    {
        SetOutlineAlpha(selectedOutline.a);
        LerpScale(baseScale * selectedScale, 0.1f);
    }

    public void OnDeselect(BaseEventData e)
    {
        SetOutlineAlpha(0f);
        LerpScale(baseScale, 0.1f);
    }

    public void OnSubmit(BaseEventData e)      // 키보드/컨트롤러 Submit
    {
        if (scaleCo != null) StopCoroutine(scaleCo);
        StartCoroutine(Pulse());
    }

    public void OnPointerDown(PointerEventData e)  // 마우스로 눌렀을 때도
    {
        if (scaleCo != null) StopCoroutine(scaleCo);
        StartCoroutine(Pulse());
    }

    IEnumerator Pulse()
    {
        yield return ScaleTo(baseScale * pressedScale, 0.05f);
        yield return ScaleTo(baseScale * selectedScale, 0.08f);
    }

    void SetOutlineAlpha(float a)
    {
        var c = outline.effectColor; c.a = a; outline.effectColor = c;
    }

    void LerpScale(Vector3 target, float dur)
    {
        if (scaleCo != null) StopCoroutine(scaleCo);
        scaleCo = StartCoroutine(ScaleTo(target, dur));
    }

    IEnumerator ScaleTo(Vector3 target, float dur)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, dur);
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.localScale = target;
    }
}
