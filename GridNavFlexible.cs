using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridNavFlexible : MonoBehaviour
{
    [Header("Row-major 순서 (예: TL, TR, BL, BR)")]
    public List<Button> buttons = new List<Button>();

    [Header("열(Column) 수")]
    public int columns = 2;

    [Header("경계에서 넘어가면 보낼 곳 (선택)")]
    public Selectable onUpEdge;
    public Selectable onDownEdge;
    public Selectable onLeftEdge;
    public Selectable onRightEdge;

    [Header("랩 이동 (선택)")]
    public bool wrapHorizontal = false; // 행 안에서 좌↔우 랩
    public bool wrapVertical = false;   // 열 안에서 상↔하 랩

    void OnEnable() => Apply();

    public void Apply()
    {
        if (buttons == null || buttons.Count == 0 || columns <= 0) return;

        int rows = Mathf.CeilToInt((float)buttons.Count / columns);

        for (int i = 0; i < buttons.Count; i++)
        {
            var b = buttons[i];
            if (!b) continue;

            int r = i / columns;
            int c = i % columns;

            // 이 행의 마지막 열 인덱스(마지막 행이 꽉 안 찼을 때 대비)
            int lastColThisRow = LastColInRow(r, rows);

            Button up = (r > 0) ? GetButton(r - 1, Mathf.Min(c, LastColInRow(r - 1, rows))) : null;
            Button down = (r < rows - 1 && IndexValid(r + 1, c)) ? GetButton(r + 1, c) : null;
            Button left = (c > 0) ? GetButton(r, c - 1) : null;
            Button right = (c < lastColThisRow) ? GetButton(r, c + 1) : null;

            // 랩 처리
            if (!up && wrapVertical) up = GetButton(rows - 1, Mathf.Min(c, LastColInRow(rows - 1, rows)));
            if (!down && wrapVertical) down = GetButton(0, Mathf.Min(c, LastColInRow(0, rows)));
            if (!left && wrapHorizontal) left = GetButton(r, lastColThisRow);
            if (!right && wrapHorizontal) right = GetButton(r, 0);

            // 폴백 적용(없으면 null → 그 방향에서 멈춤)
            Selectable upSel = (Selectable)(up ? up : onUpEdge);
            Selectable downSel = (Selectable)(down ? down : onDownEdge);
            Selectable leftSel = (Selectable)(left ? left : onLeftEdge);
            Selectable rightSel = (Selectable)(right ? right : onRightEdge);

            var nav = b.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = upSel;
            nav.selectOnDown = downSel;
            nav.selectOnLeft = leftSel;
            nav.selectOnRight = rightSel;
            b.navigation = nav;
        }
    }

    bool IndexValid(int r, int c)
    {
        int idx = r * columns + c;
        return idx >= 0 && idx < buttons.Count && buttons[idx] != null;
    }

    Button GetButton(int r, int c)
    {
        int idx = r * columns + c;
        if (idx < 0 || idx >= buttons.Count) return null;
        return buttons[idx];
    }

    int LastColInRow(int r, int totalRows)
    {
        int remain = buttons.Count - r * columns;
        int countInRow = Mathf.Clamp(remain, 0, columns);
        return Mathf.Max(0, countInRow - 1);
    }
}
