using System;
using System.Collections.Generic;
using System.Globalization;
using Scorpio.Timer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Scorpio.Unity.Logger;

using Object = UnityEngine.Object;

public static partial class EngineUtil {
    public enum Direction {
        Width,
        Height
    }
    private static readonly List<UITweener> s_TweenerCache = new List<UITweener> ();
    public static CultureInfo NumberCultureInfo = CultureInfo.InvariantCulture;
    //固定高度或宽度，根据图片比例自动设置宽高
    public static void FixSize (UnityEngine.Object obj, Direction dir) {
        var gameObject = GetGameObject (obj);
        if (gameObject == null) { return; }
        var rectTransform = gameObject.transform as RectTransform;
        if (rectTransform == null) { return; }
        var texture = gameObject.GetComponent<Image> ()?.mainTexture;
        if (texture == null) {
            texture = gameObject.GetComponent<RawImage> ()?.mainTexture;
        }
        if (texture == null) { return; }
        var width = texture.width;
        var height = texture.height;
        var size = rectTransform.sizeDelta;
        if (dir == Direction.Width) {
            size.y = size.x * height / width;
        } else {
            size.x = size.y * width / height;
        }
        rectTransform.sizeDelta = size;
    }
    public static List<RaycastResult> EventSystemRaycastAll (Vector2 position) {
        var results = new List<RaycastResult> ();
        EventSystem.current.RaycastAll (new PointerEventData (EventSystem.current) { position = position }, results);
        return results;
    }
    public static void SetSide (UnityEngine.Object obj, UIAnchor.Side side) {
        SetSide (obj, side, Vector2.zero);
    }
    public static void SetSide (UnityEngine.Object obj, UIAnchor.Side side, Vector2 offset) {
        var rectTransform = GetTransform (obj) as RectTransform;
        if (rectTransform == null) {
            logger.error ("SetSide 不包含 RectTransform");
            return;
        }
        var canvas = GetComponentInParent<Canvas> (obj);
        if (canvas == null) {
            logger.error ("SetSide 父级找不到 Canvas");
            return;
        }
        var size = ((RectTransform) canvas.transform).sizeDelta;
        var position = Vector2.zero;
        switch (side) {
            case UIAnchor.Side.BottomLeft:
                position = new Vector2 (-size.x / 2, -size.y / 2);
                break;
            case UIAnchor.Side.Left:
                position = new Vector2 (-size.x / 2, 0);
                break;
            case UIAnchor.Side.TopLeft:
                position = new Vector2 (-size.x / 2, size.y / 2);
                break;
            case UIAnchor.Side.Top:
                position = new Vector2 (0, size.y / 2);
                break;
            case UIAnchor.Side.TopRight:
                position = new Vector2 (size.x / 2, size.y / 2);
                break;
            case UIAnchor.Side.Right:
                position = new Vector2 (size.x / 2, 0);
                break;
            case UIAnchor.Side.BottomRight:
                position = new Vector2 (size.x / 2, -size.y / 2);
                break;
            case UIAnchor.Side.Bottom:
                position = new Vector2 (0, -size.y / 2);
                break;
            case UIAnchor.Side.Center:
                position = Vector2.zero;
                break;
        }
        rectTransform.anchorMin = new Vector2 (0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2 (0.5f, 0.5f);
        rectTransform.anchoredPosition = position + offset;
        var parentOffset = canvas.transform.position - rectTransform.parent.position;
        rectTransform.position = rectTransform.position + parentOffset;
    }
    //public static void SetInteractable (UnityEngine.Object obj, bool value) {
    //    SetInteractable (obj, value, true);
    //}
    //public static void SetInteractable (UnityEngine.Object obj, bool value, bool grey) {
    //    var group = GetComponent<CanvasGroup> (obj, false);
    //    if (group != null) {
    //        group.interactable = value;
    //        return;
    //    }
    //    var selectable = GetComponent<Selectable> (obj, false);
    //    if (selectable != null) {
    //        selectable.interactable = value;
    //        if (value) {
    //            SetImageGrey (selectable.image, false);
    //        } else if (grey) {
    //            SetImageGrey (selectable.image, true);
    //        }
    //    }
    //}
    public static void PlayTweener (Object obj) {
        var gameObject = GetGameObject (obj);
        s_TweenerCache.Clear ();
        gameObject.GetComponents (s_TweenerCache);
        if (s_TweenerCache.Count > 0) {
            foreach (var tweener in s_TweenerCache) {
                tweener.Reset ();
                tweener.Play (true);
            }
        }
        s_TweenerCache.Clear ();
    }
    public static void SetText (this Component component, long number) {
        SetText (component, number, true);
    }
    public static void SetText (this Component component, long number, bool format) {
        if (component == null) { return; }
        SetText (component.gameObject, number, format);
    }
    public static void SetText (this Component component, string str) {
        if (component == null) { return; }
        SetText (component.gameObject, str);
    }
    public static void SetText (this GameObject gameObject, long number) {
        SetText (gameObject, number, true);
    }
    public static void SetText (this GameObject gameObject, long number, bool format) {
        SetText (gameObject, format ? number.ToString ("N0", NumberCultureInfo) : number.ToString ());
    }
    public static void SetText (this GameObject gameObject, string str) {
        if (gameObject == null) { return; }
        var text = gameObject.GetComponent<Text> ();
        if (text != null) {
            text.text = str;
            return;
        }
        var mesh = gameObject.GetComponent<TextMesh> ();
        if (mesh != null) {
            mesh.text = str;
        }
    }
    public static void SetWidth (this Graphic component, float width) {
        var graphic = component as Graphic;
        if (graphic != null) {
            graphic.rectTransform.sizeDelta = new Vector2 (width, graphic.rectTransform.sizeDelta.y);
        }
    }
    //public static void SetCountDown (Object obj, Timer timer, float totalTime) {
    //    var go = GetGameObject (obj);
    //    if (go == null || timer == null)
    //        return;
    //    UICountDown.Add (go, timer, totalTime);
    //}
    public static UIDrag SetUIDrag (Object obj, Transform dragParent, Camera UICamera, Action onBeginDrag, Action<Vector2> onDrop, Action onEndDrag) {
        var go = GetGameObject (obj);
        if (go != null) {
            return UIDrag.Add (go, dragParent, UICamera, onBeginDrag, onDrop, onEndDrag);
        }
        return null;
    }
    public static void SetUIDragable (Object obj, bool enable) {
        var drag = GetComponent<UIDrag> (obj, false);
        if (drag != null) {
            drag.Enable = enable;
        }
    }
    public static bool UIRectContainsPoint (Object obj, Vector2 point, Camera camera) {
        var go = GetGameObject (obj);
        if (go == null)
            return false;
        var rectTrans = go.transform as RectTransform;
        return RectTransformUtility.RectangleContainsScreenPoint (rectTrans, point, camera);
    }
    //public static void DOAnchorPos (Object obj, Vector2 endValue, float duration) {
    //    var go = GetGameObject (obj);
    //    if (go == null) return;
    //    var rectTrans = go.transform as RectTransform;
    //    if (rectTrans == null) return;
    //    rectTrans.DOAnchorPos (endValue, duration);
    //}
    // 对齐两个UI
    public static void AlignTwoUI (Object standard, Object otherParent, Object other, Camera camera) {
        var go = GetGameObject (standard);
        var parentRectTrans = GetGameObject (otherParent).transform as RectTransform;
        var otherRectTrans = GetGameObject (other).transform as RectTransform;
        var screenPoint = RectTransformUtility.WorldToScreenPoint (camera, go.transform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle (parentRectTrans, screenPoint, camera, out var localPoint);
        otherRectTrans.localPosition = localPoint;
    }
    // 设置UI随着键盘高度的适配
    public static void SetKeyboardFitter (Object obj) {
        var go = GetGameObject (obj);
        UIKeyboardFitter.Add (go);
    }
    public static void SetRTLeft (Object obj, float left) {
        var rt = GetComponent<RectTransform> (obj, false);
        if (rt != null) {
            rt.offsetMin = new Vector2 (left, rt.offsetMin.y);
        }
    }
    public static void ForceRebuildLayout (Object obj) {
        var rectTrans = GetComponent<RectTransform> (obj, false);
        if (rectTrans != null) {
            LayoutRebuilder.ForceRebuildLayoutImmediate (rectTrans);
        }
    }
    // 设置滑动列表的百分比位置
    public static void SetScrollRectPosition (Object obj, float value) {
        var scrollRect = GetComponent<ScrollRect> (obj, false);
        if (scrollRect != null) {
            if (scrollRect.horizontal)
                scrollRect.horizontalNormalizedPosition = value;
            else if (scrollRect.vertical)
                scrollRect.verticalNormalizedPosition = value;
        }
    }
    // 设置滑动列表的百分比位置
    public static void SetScrollRectPosition (Object obj, Vector2 value) {
        var scrollRect = GetComponent<ScrollRect> (obj, false);
        if (scrollRect != null) {
            scrollRect.normalizedPosition = value;
        }
    }
    // 设置长按功能
    public static void SetLongPress (Object obj, Action onTrigger) {
        var go = GetGameObject (obj);
        if (go == null)
            return;
        UILongPress.Add (go, onTrigger);
    }
    // 重置长按功能
    public static void ResetLongPress (Object obj) {
        var handler = GetComponent<UILongPress> (obj, false);
        if (handler == null)
            return;
        handler.StopTrigger ();
    }
    // 设置屏幕是否可旋转
    public static void SetScreenRotatable (bool rotatable) {
        if (rotatable) {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        } else {
            switch (Screen.orientation) {
                case ScreenOrientation.LandscapeLeft:
                    Screen.autorotateToLandscapeRight = false;
                    break;
                case ScreenOrientation.LandscapeRight:
                    Screen.autorotateToLandscapeLeft = false;
                    break;
            }
        }
    }
    public static bool SetIcon (UnityEngine.Object obj, string assetBundleName, string resourceName) {
        if (obj == null || string.IsNullOrEmpty(assetBundleName)) { return false; }
        var image = GetComponent<Image> (obj, false);
        if (image != null) {
            image.sprite = ResourceManager.Instance.LoadSprite (assetBundleName, resourceName);
            return true;
        }
        var renderer = GetComponent<SpriteRenderer> (obj, false);
        if (renderer != null) {
            renderer.sprite = ResourceManager.Instance.LoadSprite (assetBundleName, resourceName);
            return true;
        }
        var rawImage = GetComponent<RawImage> (obj, false);
        if (rawImage != null) {
            rawImage.texture = ResourceManager.Instance.LoadTexture (assetBundleName, resourceName);
            return true;
        }
        return false;
    }
    // 设置Graphic组件的NativeSize
    public static void SetNativeSize (Object obj) {
        var graphic = GetComponent<Graphic>(obj, false);
        if (graphic != null) {
            graphic.SetNativeSize ();
        }
    }
    public static void SetNativeSizeByMaxsize(UnityEngine.Object obj, float maxSize) {
        var graphic = GetComponent<Graphic>(obj, false);
        if (graphic != null) {
            graphic.SetNativeSize ();
            var size = GetSizeDelta(graphic);
            var max = Mathf.Max(size.x, size.y);
            if (max > maxSize) {
                size *= (maxSize / max);
                SetSizeDelta(obj, size);
            }
        }
    }
}