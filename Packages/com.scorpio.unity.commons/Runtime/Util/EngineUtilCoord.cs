using UnityEngine;
using System;
public static partial class EngineUtil {
    //世界坐标转屏幕坐标
    public static Vector2 WorldToScreen (Camera camera, Vector3 worldPoint) {
        return camera.WorldToScreenPoint (worldPoint);
    }
    //世界坐标转屏幕坐标
    public static Vector2 WorldToScreen(Camera camera, UnityEngine.Object obj) {
        return camera.WorldToScreenPoint(GetPosition(obj));
    }
    //屏幕坐标转世界坐标
    public static Vector3 ScreenToWorld (Camera camera, Vector2 screenPoint) {
        return ScreenToWorld (camera, new Vector3 (screenPoint.x, screenPoint.y, 0));
    }
    //屏幕坐标转世界坐标
    public static Vector3 ScreenToWorld (Camera camera, Vector2 screenPoint, float distance) {
        return ScreenToWorld (camera, new Vector3 (screenPoint.x, screenPoint.y, distance));
    }
    //屏幕坐标转世界坐标
    public static Vector3 ScreenToWorld (Camera camera, Vector3 screenPoint) {
        return camera.ScreenToWorldPoint (screenPoint);
    }
    //屏幕坐标转世界坐标 Vector2
    public static Vector2 ScreenToWorldV2 (Camera camera, Vector2 screenPoint) {
        return ScreenToWorld (camera, screenPoint);
    }
    //屏幕坐标转世界坐标 Vector2
    public static Vector2 ScreenToWorldV2 (Camera camera, Vector3 screenPoint) {
        return ScreenToWorld (camera, screenPoint);
    }
    public static float Distance(Vector3 a, Vector3 b) {
        return Vector3.Distance(a, b);
    }
    public static float Distance(Vector3 a, Vector2 b) {
        return Vector3.Distance(a, b);
    }
    public static float Distance(Vector2 a, Vector3 b) {
        return Vector3.Distance(a, b);
    }
    public static float Distance(Vector2 a, Vector2 b) {
        return Vector2.Distance(a, b);
    }
    //RectTransform 是否包含一个屏幕坐标
    public static bool RectangleContainsScreenPoint(RectTransform rect, Vector2 screenPoint) {
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint);
    }
    //RectTransform 是否包含一个屏幕坐标
    public static bool RectangleContainsScreenPoint(RectTransform rect, Vector2 screenPoint, Camera cam) {
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam);
    }
    //屏幕坐标 转 相对于一个RectTransform的相对坐标
    public static Tuple<bool, Vector2> ScreenPointToLocalPointInRectangle(RectTransform rect, Vector3 screenPoint, Camera cam) {
        return ScreenPointToLocalPointInRectangle(rect, new Vector2(screenPoint.x, screenPoint.y), cam);
    }
    //屏幕坐标 转 相对于一个RectTransform的相对坐标
    public static Tuple<bool, Vector2> ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam) {
        Vector2 localPoint;
        var ret = ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out localPoint);
        return Tuple.Create(ret, localPoint);
    }
      //屏幕坐标 转 相对于一个RectTransform的相对坐标
    public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint) {
        localPoint = Vector2.zero;
        Vector3 position;
        bool result;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out position)) {
            var before = rect.localScale;
            rect.localScale = Vector3.one;
            localPoint = rect.InverseTransformPoint(position);
            rect.localScale = before;
            result = true;
        } else {
            result = false;
        }
        return result;
    }
    //世界坐标 转 相对于一个RectTransform的相对坐标
    public static Tuple<bool, Vector2> WorldToLocalPointInRectangle(RectTransform rect, Vector3 worldPoint, Camera cam) {
        return ScreenPointToLocalPointInRectangle(rect, WorldToScreen(cam, worldPoint), cam);
    }
    //世界坐标 转 相对于一个RectTransform的相对坐标
    public static Tuple<bool, Vector2> ObjectToLocalPointInRectangle(RectTransform rect, GameObject gameObject, Camera cam) {
        return ScreenPointToLocalPointInRectangle(rect, WorldToScreen(cam, gameObject.transform.position), cam);
    }

    //格子转世界坐标
    public static Vector3 CellToWorld (GridLayout layout, Vector3Int cell) {
        return layout.CellToWorld (cell);
    }
    //格子转世界坐标
    public static Vector3 CellToWorld (GridLayout layout, Vector3 cell) {
        return layout.LocalToWorld (layout.CellToLocalInterpolated (cell));
    }
    //世界坐标转格子
    public static Vector3Int WorldToCell (GridLayout layout, Vector3 world) {
        return layout.WorldToCell (world);
    }
    //世界坐标转格子
    public static Vector3 WorldToCellInterpolated (GridLayout layout, Vector3 world) {
        return layout.LocalToCellInterpolated (layout.WorldToLocal (world));
    }
    //屏幕坐标转格子
    public static Vector3Int ScreenToCell (GridLayout layout, Camera camera, Vector2 screenPoint) {
        return ScreenToCell (layout, camera, new Vector3 (screenPoint.x, screenPoint.y, 0));
    }
    //屏幕坐标转格子
    public static Vector3Int ScreenToCell (GridLayout layout, Camera camera, Vector3 screenPoint) {
        return layout.WorldToCell (ScreenToWorld (camera, screenPoint));
    }
    //格子转屏幕坐标
    public static Vector2 CellToScreen (GridLayout layout, Camera camera, Vector3Int cell) {
        return WorldToScreen (camera, layout.CellToWorld (cell));
    }
    //格子转屏幕坐标
    public static Vector2 CellToScreen (GridLayout layout, Camera camera, Vector3 cell) {
        return WorldToScreen (camera, layout.LocalToWorld (layout.CellToLocalInterpolated (cell)));
    }

    //
    public static Vector3 CellToLocal (GridLayout layout, Vector3Int cell) {
        return layout.CellToLocal (cell);
    }
    public static Vector3 CellToLocal (GridLayout layout, Vector3 cell) {
        return layout.CellToLocalInterpolated (cell);
    }
    public static Vector3 LocalToCell (GridLayout layout, Vector3 local) {
        return layout.LocalToCell (local);
    }
    public static Vector3 LocalToWorld (GridLayout layout, Vector3 local) {
        return layout.LocalToWorld (local);
    }
}