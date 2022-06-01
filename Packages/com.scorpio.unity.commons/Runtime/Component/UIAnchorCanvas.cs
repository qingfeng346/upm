using UnityEngine;

[DisallowMultipleComponent]
public sealed class UIAnchorCanvas : MonoBehaviour
{
    private Canvas m_Canvas = null;
    private Canvas canvas => m_Canvas ?? (m_Canvas = GetComponentInParent<Canvas>());
    private RectTransform rectTransform => transform as RectTransform;
    private Rect m_SafeArea = Rect.zero;

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        if (Time.frameCount % 5 == 0 && m_SafeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }

    private void OnTransformParentChanged()
    {
        ApplySafeArea();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (canvas == null || rectTransform == null)
        {
            return;
        }
        var safeArea = Screen.safeArea;
        m_SafeArea = new Rect(safeArea.x, 0, safeArea.width, Screen.height);
        var anchorMin = m_SafeArea.position;
        var anchorMax = m_SafeArea.position + m_SafeArea.size;
        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
