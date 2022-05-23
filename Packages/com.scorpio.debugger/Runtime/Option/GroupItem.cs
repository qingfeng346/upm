using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scorpio.Debugger
{
    public class GroupItem : MonoBehaviour
    {
        public Text title;
        public void SetTitle(string title)
        {
            this.title.text = title;
        }
    }
}