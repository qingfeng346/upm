using UnityEngine;
using UnityEngine.UI;

public class ScorpioDebuggerWindow : MonoBehaviour
{
    public RectTransform tabs;
    public RectTransform items;
    public Text hideText;
    public GameObject minimize;
    public GameObject maximize;
    public bool visiable = true;
    public void OnClickShowHide() {
        SetVisiable(!visiable);
    }
    void SetVisiable(bool visiable) {
        if (this.visiable == visiable) { return; }
        this.visiable = visiable;
        if (visiable) {
            tabs.anchoredPosition = new Vector2(0, 0);
            items.offsetMin = new Vector2(160, 0);
            hideText.text = "◁";
        } else {
            tabs.anchoredPosition = new Vector2(-160, 0);
            items.offsetMin = new Vector2(0, 0);
            hideText.text = "▷";
        }
    }
    public void OnClickMinimize() {
        minimize.SetActive(true);
        maximize.SetActive(false);
    }
    public void OnClickMaximize() {
        minimize.SetActive(false);
        maximize.SetActive(true);
    }
}
