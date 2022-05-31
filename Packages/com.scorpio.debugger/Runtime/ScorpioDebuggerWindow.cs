﻿using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Debugger
{
    public class ScorpioDebuggerWindow : MonoBehaviour
    {
        public RectTransform tabs;
        public RectTransform items;
        public Text hideText;
        public GameObject minimize;
        public GameObject maximize;
        public bool visiable = true;
        public AutoEdge autoEdge;
        void Awake()
        {
            OnClickMinimize();
            autoEdge.onClick = OnClickMaximize;
        }
        public void OnClickShowHide()
        {
            SetTabVisiable(!visiable);
        }
        public void SetTabVisiable(bool visiable)
        {
            if (this.visiable == visiable) { return; }
            this.visiable = visiable;
            if (visiable)
            {
                tabs.anchoredPosition = new Vector2(0, 0);
                items.offsetMin = new Vector2(160, 0);
                hideText.text = "◁";
            }
            else
            {
                tabs.anchoredPosition = new Vector2(-160, 0);
                items.offsetMin = new Vector2(0, 0);
                hideText.text = "▷";
            }
        }
        public void OnClickMinimize()
        {
            minimize.SetActive(true);
            maximize.SetActive(false);
        }
        public void OnClickMaximize()
        {
            minimize.SetActive(false);
            maximize.SetActive(true);
        }
        public void OnClickClose()
        {
            ScorpioDebugger.Instance.Hide();
        }
    }
}