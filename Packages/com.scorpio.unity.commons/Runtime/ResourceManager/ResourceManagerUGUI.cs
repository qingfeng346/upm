using UnityEngine;
using UnityEngine.UI;

public partial class ResourceManager {
    //public Text NewText { get { var text = new GameObject ("Text").AddComponent<Text> (); text.font = DefaultFont; return text; } }
    public Image NewImage => new GameObject ("Image").AddComponent<Image> ();
    public RawImage NewRawImage => new GameObject ("RawImage").AddComponent<RawImage> ();
    public Canvas NewCanvas (string name, int width, int height, int sortingOrder) {
        var obj = new GameObject (name);
        var canvas = obj.AddComponent<Canvas> ();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = obj.AddComponent<CanvasScaler> ();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2 (width, height);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        return canvas;
    }
    public Canvas NewCanvas3D (string name, Camera camera) {
        var obj = new GameObject (name);
        var canvas = obj.AddComponent<Canvas> ();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        obj.AddComponent<CanvasScaler> ();
        return canvas;
    }
}