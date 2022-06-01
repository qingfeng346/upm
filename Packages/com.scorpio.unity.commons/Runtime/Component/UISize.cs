using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("UISize")]
public class UISize : MonoBehaviour
{
    public bool width, height;
    public Vector2 offset;
    RectTransform mRectTransform;
    RectTransform rectTransform => mRectTransform ?? (mRectTransform = transform as RectTransform);
    void Start()
    {
        StartCoroutine(StartUpdateSize());
    }
    void OnEnable()
    {
        StartCoroutine(StartUpdateSize());
    }
    IEnumerator StartUpdateSize()
    {
        for (var i = 0; i < 5; ++i)
        {
            UpdateSize();
            yield return new WaitForEndOfFrame();
        }
    }
    public void UpdateSize()
    {
        if (width || height)
        {
            var canvas = this.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var size = ((RectTransform)canvas.transform).sizeDelta;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                var sizeDelta = rectTransform.sizeDelta;
                if (width) { sizeDelta.x = size.x + offset.x; }
                if (height) { sizeDelta.y = size.y + offset.y; }

                rectTransform.sizeDelta = sizeDelta;
            }
        }
    }
    void OnTransformParentChanged()
    {
        UpdateSize();
    }
}
