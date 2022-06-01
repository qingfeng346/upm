// using System;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// public static class RectTransformUtil {
//     //RectTransform 是否包含一个屏幕坐标
//     public static bool RectangleContainsScreenPoint(RectTransform rect, Vector2 screenPoint) {
//         return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint);
//     }
//     //RectTransform 是否包含一个屏幕坐标
//     public static bool RectangleContainsScreenPoint(RectTransform rect, Vector2 screenPoint, Camera cam) {
//         return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam);
//     }
//     //屏幕坐标 转 相对于一个RectTransform的相对坐标
//     public static Tuple<bool, Vector2> ScreenPointToLocalPointInRectangle(RectTransform rect, Vector3 screenPoint, Camera cam) {
//         return ScreenPointToLocalPointInRectangle(rect, new Vector2(screenPoint.x, screenPoint.y), cam);
//     }
//     //屏幕坐标 转 相对于一个RectTransform的相对坐标
//     public static Tuple<bool, Vector2> ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam) {
//         Vector2 localPoint;
//         var ret = ScreenPointToLocalPointInRectangle(rect, screenPoint, cam, out localPoint);
//         return Tuple.Create(ret, localPoint);
//     }
//     //屏幕坐标 转 相对于一个RectTransform的相对坐标
//     public static bool ScreenPointToLocalPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint) {
//         localPoint = Vector2.zero;
//         Vector3 position;
//         bool result;
//         if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out position)) {
//             var before = rect.localScale;
//             rect.localScale = Vector3.one;
//             localPoint = rect.InverseTransformPoint(position);
//             rect.localScale = before;
//             result = true;
//         } else {
//             result = false;
//         }
//         return result;
//     }


//     //鼠标坐标 转 相对于一个RectTransform的相对坐标
//     public static Tuple<bool, Vector2> InputToLocalPointInRectangle(RectTransform rect, Camera cam) {
//         return ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, cam);
//     }
//     //射线
//     public static Ray ScreenPointToRay(Camera cam, Vector2 screenPos) {
//         return RectTransformUtility.ScreenPointToRay(cam, screenPos);
//     }
//     //屏幕坐标转 相对于一个RectTransform的世界坐标
//     public static Tuple<bool, Vector3> ScreenPointToWorldPointInRectangle(RectTransform rect, Vector2 screenPoint, Camera cam) {
//         Vector3 worldPoint;
//         var ret = RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out worldPoint);
//         return Tuple.Create(ret, worldPoint);
//     }
//     //世界坐标 转 屏幕坐标
//     public static Vector2 WorldToScreenPoint(Camera cam, Vector3 worldPoint) {
//         return RectTransformUtility.WorldToScreenPoint(cam, worldPoint);
//     }
//     //屏幕坐标 转 世界坐标
//     public static Vector3 ScreenToWorldPoint(Vector2 position, Camera camera) {
//         return ScreenToWorldPoint(new Vector3(position.x, position.y, 0), camera);
//     }
//     //屏幕坐标 转 世界坐标
//     public static Vector3 ScreenToWorldPoint(Vector3 position, Camera camera) {
//         return camera.ScreenToWorldPoint(position);
//     }

//     //格子坐标转屏幕坐标
//     public static Vector2 CellToScreenPoint(Tilemap tilemap, Vector3Int cell, Camera cam) {
//         return WorldToScreenPoint(cam, tilemap.GetCellCenterWorld(cell));
//     }
//     //格子坐标转世界坐标
//     public static Vector3 CellToWorldPoint(Tilemap tilemap, float x, float y) {
//         return tilemap.LocalToWorld(tilemap.CellToLocalInterpolated(new Vector3(x + .5f, y + .5f, 0)));
//     }
//     //世界坐标转格子坐标
//     public static Vector3Int WorldToCell(GridLayout layout, Vector3 worldPosition) {
//         return layout.WorldToCell(worldPosition);
//     }
//     //屏幕坐标转格子坐标
//     public static Vector3Int ScreenToCell(GridLayout layout, Vector2 position, Camera camera) {
//         return ScreenToCell(layout, new Vector3(position.x, position.y), camera);
//     }
//     //屏幕坐标转格子坐标
//     public static Vector3Int ScreenToCell(GridLayout layout, Vector3 position, Camera camera) {
//         return layout.WorldToCell(ScreenToWorldPoint(position, camera));
//     }


//     //格子坐标 转 相对一个RectTransform的相对坐标
//     public static Tuple<bool, Vector2> CellToLocalPointInRectangle(RectTransform rect, Camera rectCamera, Tilemap tilemap, Vector3Int cell, Camera tileCamera) {
//         return ScreenPointToLocalPointInRectangle(rect, CellToScreenPoint(tilemap, cell, tileCamera), rectCamera);
//     }
//     //设置一个物体到格子坐标
//     public static void SetPosition(GameObject gameObject, GridLayout gridLayout, float x, float y) {
//         gameObject.transform.position = gridLayout.LocalToWorld (gridLayout.CellToLocalInterpolated (new Vector3 (x + .5f, y + .5f, 0)));
//     }
// }