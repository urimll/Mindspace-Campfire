using UnityEngine;
using TMPro;

public class BackgroundButtonLabelGuard : MonoBehaviour
{
    public BackgroundMenuController controller; // BackgroundUI를 드래그
    public TMP_Text label;                      // BackgroundButton/Text (TMP)
    public string labelWhenClosed = "배경설정";
    public string labelWhenOpen = "닫기";

    void LateUpdate()
    {
        if (!controller || !label) return;
        bool open = controller.menuRoot && controller.menuRoot.activeSelf;
        string want = open ? labelWhenOpen : labelWhenClosed;
        if (label.text != want) label.text = want; // 다른 스크립트가 바꿔도 매 프레임 되돌림
    }
}
