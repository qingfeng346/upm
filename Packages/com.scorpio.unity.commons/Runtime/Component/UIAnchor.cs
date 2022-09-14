using System;
using System.Collections;
using UnityEngine;
[ExecuteInEditMode]
[AddComponentMenu ("UIAnchor")]
public class UIAnchor : MonoBehaviour {
    private static Vector3[] ParentScale = new Vector3[64];
    private static bool isCustomScreenRect = false;
    private static Rect customScreenRect = Rect.zero;
    public static Rect safeArea {
        get { return isCustomScreenRect ? customScreenRect : new Rect (0, 0, Screen.width, Screen.height); }
        set { isCustomScreenRect = true; customScreenRect = value; OnSafeAreaChanged?.Invoke (); }
    }
    public static Action OnSafeAreaChanged;
    public enum Side {
        BottomLeft,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        Center,
    }
    [SerializeField] public Side side = Side.Center;
    [SerializeField] public Vector2 offset = Vector2.zero;
    [SerializeField] public bool ignoreScale = true;
    RectTransform mRectTransform;
    RectTransform rectTransform => mRectTransform ?? (mRectTransform = transform as RectTransform);
    void Start () {
        StartCoroutine (StartUpdatePosition ());
    }
    void OnEnable () {
        OnSafeAreaChanged += UpdatePosition;
        StartCoroutine (StartUpdatePosition ());
    }
    void OnDisable () {
        OnSafeAreaChanged -= UpdatePosition;
    }
    IEnumerator StartUpdatePosition () {
        for (var i = 0; i < 5; ++i) {
            UpdatePosition ();
            yield return null;
        }
    }
    public void SetOffset (float x, float y) {
        offset = new Vector2 (x, y);
        UpdatePosition ();
    }
    public void SetSide (Side side) {
        this.side = side;
        UpdatePosition ();
    }
    public void UpdatePosition () {
        var canvas = GetComponentInParent<Canvas> ();
        if (canvas == null) { return; }
        var parent = rectTransform.parent as RectTransform;
        if (parent == null) { return; }
        var screenPoint = Vector2.zero;
        switch (side) {
            case Side.BottomLeft:
                screenPoint = new Vector2 (safeArea.xMin, safeArea.yMin);
                break;
            case Side.Left:
                screenPoint = new Vector2 (safeArea.xMin, safeArea.center.y);
                break;
            case Side.TopLeft:
                screenPoint = new Vector2 (safeArea.xMin, safeArea.yMax);
                break;
            case Side.Top:
                screenPoint = new Vector2 (safeArea.center.x, safeArea.yMax);
                break;
            case Side.TopRight:
                screenPoint = new Vector2 (safeArea.xMax, safeArea.yMax);
                break;
            case Side.Right:
                screenPoint = new Vector2 (safeArea.xMax, safeArea.center.y);
                break;
            case Side.BottomRight:
                screenPoint = new Vector2 (safeArea.xMax, safeArea.yMin);
                break;
            case Side.Bottom:
                screenPoint = new Vector2 (safeArea.center.x, safeArea.yMin);
                break;
            case Side.Center:
                screenPoint = safeArea.center;
                break;
        }
        if (ignoreScale) {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, canvas.worldCamera, out var localPoint)) {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = localPoint + offset;
            }
        } else {
            //先把父级缩放全部置为0,再计算锚点
            var index = 0;
            var trans = transform;
            while (true) {
                ParentScale[index++] = trans.localScale;
                trans.localScale = Vector3.one;
                trans = trans.parent;
                if (trans == null || trans.GetComponent<Canvas>() != null) { break; }
            }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, canvas.worldCamera, out var localPoint)) {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = localPoint + offset;
            }
            index = 0;
            trans = transform;
            while (true) {
                trans.localScale = ParentScale[index++];
                trans = trans.parent;
                if (trans == null || trans.GetComponent<Canvas>() != null) { break; }
            }
        }
    }
    void OnTransformParentChanged () {
        UpdatePosition ();
    }
#if UNITY_EDITOR
    void Update () {
        UpdatePosition ();
    }
#endif
}