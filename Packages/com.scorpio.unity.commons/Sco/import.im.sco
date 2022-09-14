//**************c#原生类***************
System = importNamespace("System")
CSharpBoolean = importType("System.Boolean")
CSharpInt32 = importType("System.Int32")
CSharpString = importType("System.String")
CSharpSingle = importType("System.Single")
CSharpDouble = importType("System.Double")

//***************Unity引擎***************
UnityEngine = importNamespace("UnityEngine")

GameObject = importType("UnityEngine.GameObject")
Transform = importType("UnityEngine.Transform")
RectTransform = importType("UnityEngine.RectTransform")
Camera = importType("UnityEngine.Camera")
Application = importType("UnityEngine.Application")
SystemInfo = importType("UnityEngine.SystemInfo")
RuntimePlatform = importType("UnityEngine.RuntimePlatform")
Time = importType("UnityEngine.Time")
Input = importType("UnityEngine.Input")
ParticleSystem = importType("UnityEngine.ParticleSystem")
Screen = importType("UnityEngine.Screen")

KeyCode = importType("UnityEngine.KeyCode")
Mathf = importType("UnityEngine.Mathf")
SceneManager = importType("UnityEngine.SceneManagement.SceneManager")
LoadSceneMode = importType("UnityEngine.SceneManagement.LoadSceneMode")

Physics = importType("UnityEngine.Physics")
Physics2D = importType("UnityEngine.Physics2D")
LayerMask = importType("UnityEngine.LayerMask")
Rigidbody2D = importType("UnityEngine.Rigidbody2D")
PolygonCollider2D = importType("UnityEngine.PolygonCollider2D")

Renderer = importType("UnityEngine.Renderer")
SpriteRenderer = importType("UnityEngine.SpriteRenderer")
MeshRenderer = importType("UnityEngine.MeshRenderer")
SkinnedMeshRenderer = importType("UnityEngine.SkinnedMeshRenderer")
WWWForm = importType("UnityEngine.WWWForm")

WaitForEndOfFrame = importType("UnityEngine.WaitForEndOfFrame")
WaitForFixedUpdate = importType("UnityEngine.WaitForFixedUpdate")
WaitForSeconds = importType("UnityEngine.WaitForSeconds")
WaitForSecondsRealtime = importType("UnityEngine.WaitForSecondsRealtime")

GUIUtility = importType("UnityEngine.GUIUtility")
ColorUtility = importType("UnityEngine.ColorUtility")
RectTransformUtility = importType("UnityEngine.RectTransformUtility")

//***************基础类***************
Vector2 = importType("UnityEngine.Vector2")
Vector3 = importType("UnityEngine.Vector3")
Vector4 = importType("UnityEngine.Vector4")
Vector2Int = importType("UnityEngine.Vector2Int")
Vector3Int = importType("UnityEngine.Vector3Int")
Rect = importType("UnityEngine.Rect")
RectInt = importType("UnityEngine.RectInt")
Color = importType("UnityEngine.Color")
Color32 = importType("UnityEngine.Color32")
Bounds = importType("UnityEngine.Bounds")
BoundsInt = importType("UnityEngine.BoundsInt")
Quaternion = importType("UnityEngine.Quaternion")
Matrix4x4 = importType("UnityEngine.Matrix4x4")

//***************资源类***************
TextAsset = importType("UnityEngine.TextAsset")
AudioClip = importType("UnityEngine.AudioClip")
Material = importType("UnityEngine.Material")
Shader = importType("UnityEngine.Shader")
Sprite = importType("UnityEngine.Sprite")
Texture = importType("UnityEngine.Texture")
Texture2D = importType("UnityEngine.Texture2D")
TextureFormat = importType("UnityEngine.TextureFormat")
FilterMode = importType("UnityEngine.FilterMode")
RenderTexture = importType("UnityEngine.RenderTexture")
RenderTextureFormat = importType("UnityEngine.RenderTextureFormat")
Animator = importType("UnityEngine.Animator")
Animation = importType("UnityEngine.Animation")
Font = importType("UnityEngine.Font")

//***************UGUI***************
Graphics = importType("UnityEngine.Graphics")
Canvas = importType("UnityEngine.Canvas")
CanvasGroup = importType("UnityEngine.CanvasGroup")
RenderMode = importType("UnityEngine.RenderMode")
EventSystem = importType("UnityEngine.EventSystems.EventSystem")
MaskableGraphic = importType("UnityEngine.UI.MaskableGraphic")
CanvasScaler = importType("UnityEngine.UI.CanvasScaler")
GraphicRaycaster = importType("UnityEngine.UI.GraphicRaycaster")
Text = importType("UnityEngine.UI.Text")
Image = importType("UnityEngine.UI.Image")
RawImage = importType("UnityEngine.UI.RawImage")
Button = importType("UnityEngine.UI.Button")
InputField = importType("UnityEngine.UI.InputField")
Toggle = importType("UnityEngine.UI.Toggle")
ToggleGroup = importType("UnityEngine.UI.ToggleGroup")
Slider = importType("UnityEngine.UI.Slider")
Scrollbar = importType("UnityEngine.UI.Scrollbar")
Dropdown = importType("UnityEngine.UI.Dropdown")
ScrollRect = importType("UnityEngine.UI.ScrollRect")
ContentSizeFitter = importType("UnityEngine.UI.ContentSizeFitter")

LayoutRebuilder = importType("UnityEngine.UI.LayoutRebuilder")
LayoutGroup = importType("UnityEngine.UI.LayoutGroup")
GridLayoutGroup = importType("UnityEngine.UI.GridLayoutGroup")
SlicedFilledImage = importType("UnityEngine.UI.SlicedFilledImage")
RectMask2D = importType("UnityEngine.UI.RectMask2D")

//***************自实现UGUI***************
UIEmpty = importType("UnityEngine.UI.UIEmpty")

//**************管理类***************
Game = importType("Game")
PoolManager = importType("Scorpio.Pool.PoolManager").Instance
TimerManager = importType("Scorpio.Timer.TimerManager").Instance
LooperManager = importType("Scorpio.Timer.LooperManager").Instance
ResourceManager = importType("Scorpio.Resource.ResourceManager").Instance
SoundManager = importType("SoundManager").Instance
ScriptManager = importType("ScriptManager").Instance

//**************工具类**************
Util = importType("EngineUtil")
ScorpioUnityUtil = importType("Scorpio.Unity.ScorpioUnityUtil")

//**************工具类**************
ScriptPointerClickHandler = importType("ScriptPointerClickHandler")

//


//**************以下是经常用到的函数***************
AddComponent = ScorpioUnityUtil.AddComponent
GetComponent = ScorpioUnityUtil.GetComponent
DelComponent = ScorpioUnityUtil.DelComponent
GetOrAddComponent = ScorpioUnityUtil.GetOrAddComponent

Run = LooperManager.Run
RunWithKey = LooperManager.RunWithKey

AddGameTimer = TimerManager.AddGameTimer
AddGameTimerMS = TimerManager.AddGameTimerMS

AddRealTimer = TimerManager.AddRealTimer
AddRealTimerMS = TimerManager.AddRealTimerMS

AddClockTimer = TimerManager.AddClockTimer
AddClockTimerMS = TimerManager.AddClockTimerMS

AddWatchTimer = TimerManager.AddWatchTimer
AddWatchTimerMS = TimerManager.AddWatchTimerMS

//**************以下是经常用到的常量***************
IsEditor = Application.isEditor									//是否是编辑器