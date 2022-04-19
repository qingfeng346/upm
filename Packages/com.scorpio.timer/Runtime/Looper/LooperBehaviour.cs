using System;
using UnityEngine;
namespace Scorpio.Timer
{
    public class LooperBehaviour : MonoBehaviour
    {
        private void LateUpdate()
        {
            LooperManager.Instance.OnUpdate();
        }
    }
}