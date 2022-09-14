/*
UI Attribute 字段说明
Resource		string 			资源路径
Component		Array<string> 	添加的组件
BackMode		number			背景模态类型 0 无 1 显示黑色背景 2 显示黑色背景并点击关闭UI 3 显示透明碰撞 4 点击透明碰撞并关闭UI
AnimationActive boolean			是否使用开启打开关闭动画
AnimationPath	string			动画gameObject路径
ShowAnimation	string			打开UI动画名称
HideAnimation	string			关闭UI动画名称
DontShutdown	boolean			Shutdown时是否保留此UI
EscapeType		number			UI对于Esc键的处理 0 关闭UI  1 传递Esc给下一个UI  2 阻断Esc
Queue			boolean			UI是否按顺序打开,false及时打开
Layer			number(0-16)	UI层级 数字越大绘制层级越高
Single			boolean			是否都是单独的,多次调用Open时会排序打开
QueueSort		number			UI 队列的优先级,越小打开越早,需手动调用SortOpenQueue
*/
UIVirtualWidth = 750
UIVirtualHeight = 1334
UI = {
    RootUI = null,      				//RootUI 顶层Canvas
    RootCamera = null,					//UI的摄像机
    RootHideUI = null,					//隐藏的UI GameObject

    BlackBack = null,   				//透明黑色背景
    BlackObject = null,					//当前黑色背景所在的UI
    ColliderBack = null,				//模态背景
    ColliderObject = null,				//当前模态背景所在的UI

    LayerObjects = {},					//每个UI层级的信息
    MaxLayer = 16,						//UI层级数量

    ForceOpen = false,					//强制打开UI
    Objects = {},       				//所有UI的信息
    Showing = [],       				//正在显示的所有UI
    OpenQueue = [],						//即将打开的UI
    OpeningUI = [],						//正在显示的带队列的所有UI

    DefaultLayer = 5,

    Initialize(gameObject) {
        this.OnCreateUI = new Delegate()
        this.OnDestroyUI = new Delegate()
        this.OnShowUI = new Delegate()
        this.OnHideUI = new Delegate()
        this.OnShowUIFinished = new Delegate()
        this.OnHideUIFinished = new Delegate()
        this.InitRoot(gameObject)
        this.InitBlack()
        this.InitCollider()
        this.InitLayer()
        this.InitModal()
    }
    NewCanvas (name, width, height, sortingOrder) {
        var obj = new GameObject (name);
        var canvas = obj.AddComponent(Canvas);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = obj.AddComponent(CanvasScaler);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2 (width, height);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        return canvas;
    }
    InitRootGameObject() {
        var gameObject = new GameObject("____UI")
        var uiLayer = LayerMask.NameToLayer("UI")
        var rootUI = this.NewCanvas("RootUI", UIVirtualWidth, UIVirtualHeight, 100)
        Util.AddChild(gameObject, rootUI)
        var rootHideUI = new GameObject("HideUI")
        Util.AddChild(gameObject, rootHideUI)
        Util.SetActive(rootHideUI, false)

        var cameraObject = new GameObject("Camera")
        Util.AddChild(gameObject, cameraObject)
        Util.SetLocalPosition(cameraObject, UIVirtualWidth / 2, UIVirtualHeight / 2, -9900)
        camera = Util.AddComponent(cameraObject, Camera)
        camera.cullingMask = 1L << uiLayer
        camera.clearFlags = 3
        camera.depth = 50
        camera.orthographic = true
        camera.orthographicSize = UIVirtualHeight / 2
        camera.nearClipPlane = -100
        camera.farClipPlane = 11000
        camera.allowHDR = false
        camera.allowDynamicResolution = false
        camera.allowMSAA = false
        camera.useOcclusionCulling = false

        var canvas = Util.GetComponent(rootUI, Canvas)
        canvas.renderMode = RenderMode.ScreenSpaceCamera
        canvas.worldCamera = camera
        canvas.sortingOrder = 100
        canvas.planeDistance = 10000
        var canvasScaler = Util.GetComponent(rootUI, CanvasScaler)
        canvasScaler.screenMatchMode = 0
        canvasScaler.matchWidthOrHeight = 1

        rootUI.gameObject.layer = uiLayer
        rootHideUI.layer = uiLayer
        Util.AddComponent(rootUI, GraphicRaycaster)
        return gameObject
    }
    InitRoot(gameObject) {
        if (gameObject == null) {
            gameObject = this.InitRootGameObject()
        }
        Util.AddChild(Game.GlobalGameObject, gameObject)
        this.RootUI = Util.FindChild(gameObject, "RootUI", Canvas)
        this.RootHideUI = Util.FindChild(gameObject, "HideUI")
        this.RootCamera = this.RootUI.worldCamera
    } 
	InitBlack() {
		var black = new GameObject ("Image").AddComponent(Image);
		black.name = "Black"
		Util.SetColor(black, Color(0, 0, 0, 0.7))
		Util.SetSizeDelta(black, 5000, 5000)
		this.BlackBack = Util.GetTransform(black)
		this.HideBlack()
		ScriptPointerClickHandler.Register(black, this.OnClickBlack.bind(this))
	}
	InitCollider() {
		var collider = GameObject("Collider")
		collider.AddComponent(UIEmpty)
		Util.SetSizeDelta(collider, 5000, 5000)
		this.ColliderBack = Util.GetTransform(collider)
		this.HideCollider()
		ScriptPointerClickHandler.Register(collider, this.OnClickCollider.bind(this))
	}
	InitLayer() {
		this.LayerObjects = {}
		for (var i = 0, this.MaxLayer) {
			var layer = {}
			layer.Showing = []
			var obj = GameObject("Layer" + i)
			Util.AddChild(this.RootUI, obj)
			var canvas = Util.AddComponent(obj, Canvas)
			canvas.overrideSorting = true
			canvas.sortingOrder = i * 1000
			Util.AddComponent(obj, GraphicRaycaster)
            Util.SetAnchorMin(obj, Vector2.zero)
            Util.SetAnchorMax(obj, Vector2.one)
            Util.SetOffsetMin(obj, Vector2.zero)
            Util.SetOffsetMax(obj, Vector2.zero)
			layer.gameObject = obj
			this.LayerObjects[i] = layer
		}
	}
	InitModal() {
		var modal = new GameObject("Modal")
		Util.AddChild(this.RootUI, modal)
		modal.AddComponent(UIEmpty)
		var canvas = Util.AddComponent(modal, Canvas)
		canvas.overrideSorting = true
		canvas.sortingOrder = 20000
		Util.AddComponent(modal, GraphicRaycaster)
		Util.SetSizeDelta(modal, 5000, 5000)
		this.modalObject = Util.GetTransform(modal)
		this.HideModal()
	}
    ShowModal() {
		Util.SetActive(this.modalObject, true)
	}
	HideModal() {
		Util.SetActive(this.modalObject, false)
	}


	IsCloseAll() {
		var noQueue = this.OpeningUI.length() == 0 && this.OpenQueue.length() == 0
		var noBack = Util.GetParent(this.BlackBack) == this.RootHideUI && Util.GetParent(this.ColliderBack) == this.RootHideUI
		return noQueue && noBack
	}
	HideBlack() {
		Util.AddChild(this.RootHideUI, this.BlackBack)
	}
	HideCollider() {
		Util.AddChild(this.RootHideUI, this.ColliderBack)
	}
	OnClickBlack() {
		if (this.BlackObject != null && this.BlackObject.isShowFinished) {
			if (this.BlackObject.backMode == 2) {
				this.Hide(this.BlackObject.index)
			} else {
				this.BlackObject.components.forEach((com) => {
					com.OnClickBlack?.();
				})
			}
		}
	}
	OnClickCollider() {
		if (this.ColliderObject != null && this.ColliderObject.isShowFinished) {
			if (this.ColliderObject.backMode == 4) {
				this.Hide(this.ColliderObject.index)
			} else {
				this.BlackObject.components.forEach((com) => {
					com.OnClickCollider?.();
				})
			}
		}
	}
	function CreateUI(index) {
		if (this.Objects.containsKey(index)) { return this.Objects[index]; }
		var attribute = _G[index + "_ATTR"]
		if (attribute == null) {
			logError("找不到UI的配置文件 : " + index)
            return null;
		}
		var gameObject = ResourceManager.InstantiateResource("ui", attribute.Resource)
		gameObject.name = "${attribute.Resource}(${index})"
		var layer = attribute.Layer ?? this.DefaultLayer
		var animation = Util.FindChild(gameObject, "Main")
		var main = Util.FindChild(gameObject, "Main")
		var canvasGroup = Util.GetComponent(main, CanvasGroup, true)	//没有就自动添加一个
		var object = {
			attribute = attribute, 										//UI 属性
			index = index, 												//UI 名字
			gameObject = gameObject, 									//UI GameObject
			transform = Util.GetTransform(gameObject),					//UI Transform
			main = main,												//UI 动画层
            animation = animation,										//UI 动画
			canvasGroup = canvasGroup,									//UI Main  CanvasGroup
            layer = layer,												//UI 层级
			layerObject = this.LayerObjects[layer].gameObject,			//UI 层级GameObject

			backMode = attribute.BackMode ?? 0			            	//UI 背景类型
			animationActive = attribute.AnimationActive ?? false		//UI 开启动画
			showAnimation = attribute.ShowAnimation ?? "openpanel",		//UI 开启动画名字
			hideAnimation = attribute.HideAnimation ?? "closepanel",	//UI 关闭动画名字
			ignoreEscape = attribute.IgnoreEscape ?? false,				//UI 忽略Esc 忽略esc 直接判断下一个UI
			blockEscape = attribute.BlockEscape ?? false,				//UI 阻断Esc 阻断esc 直接阻断esc事件
			openSound = attribute.OpenSound ?? "ui_open",				//UI 打开音效, 如果想禁用打开音效需要设置成 ""
			closeSound = attribute.CloseSound ?? "ui_close",			//UI 关闭音效, 如果想禁用关闭音效需要设置成 ""
			single = attribute.Single ?? false,							//UI 是否都是单独的,多次调用Open时会排序打开
			queue = attribute.Queue ?? false,							//UI 是否按队列打开
			queueSort = attribute.QueueSort ?? 0,						//UI 队列的优先级,越小打开越早
		}
		var components = attribute.Component
		var coms = []
		if (components != null) {
			components.forEach((name) => {
				var value = _G[name]
				var com = isMap(value) ? clone(value) : value()		//如果是map就clone 否则就是 class 直接new一个对象
				com.Initialized = true
				com.UIObject = object
				com.Hide = function(args) { UI.Hide(index, args) }
				com.IsShow = function() { return UI.IsShow(index) }
				coms.add(AddComponent(gameObject, com, name))
			})
		}
		object.components = coms										//UI 所有脚本组件
		object.PlayShowAnimation = function (onFinished) {
			if (this.animationActive) {
				if (this.animation != null && this.showAnimation != null) {
					Util.PlayAnimator(this.animation, this.showAnimation, onFinished)
                    return true
				}
			}
			return false
		}
		object.BeginShow = function() {
			this.isShowFinished = false
			this.canvasGroup.blocksRaycasts = false
		}
		object.EndShow = function() {
			this.isShowFinished = true
			this.canvasGroup.blocksRaycasts = true
		}
		this.Objects[index] = object
		this.OnCreateUI(index, attribute, gameObject)
		return object
	}
	//强制打开界面,不走队列,慎用......
	function ShowForce(index, ...args) {
		this.ForceOpen = true
		this.Show(index, args...)
		this.ForceOpen = false
	}
	//没有打开队列是再打开界面
	function ShowNoQueue(index, ...args) {
		if (this.OpeningUI.length() == 0) {
			this.Show(index, args...)
		}
	}
	//将界面插入到队列的最前端,只用于确定性的界面连续打开
	function ShowNext(index, ...args) {
		return this.Show_impl(index, true, args...)
	}
	function Show(index, ...args) {
		return this.Show_impl(index, false, args...)
	}
	function Show_impl(index, first, ...args) {
		if (index == null || index == "") { return }
		var object = this.CreateUI(index)
		if (object == null) { return }
		if (object.queue) {
			if (this.ForceOpen) {
				this.OpeningUI.addUnique(index)
			} else if (this.OpeningUI.length() == 0 && !this.isParpareOpen) {
				this.OpeningUI.add(index)
			} else if (object.single) {
				if (this.OpenQueue.length() == 0 || !first) {
					this.OpenQueue.add({ index : index, args : args })
				} else {
					this.OpenQueue.insert(0, { index : index, args : args })
				}
				if (!this.Showing.contains(index)) {
					Util.AddChild(this.RootHideUI, object.gameObject)
				}
				return
			} else if (!this.OpeningUI.contains(index)) {
				var element = this.OpenQueue.find((data) => { return data.index == index} )
				if (element) {
					element.args = args
				} else {
					if (this.OpenQueue.length() == 0 || !first) {
						this.OpenQueue.add({ index : index, args : args })
					} else {
						this.OpenQueue.insert(0, { index : index, args : args })
					}
					Util.AddChild(this.RootHideUI, object.gameObject)
				}
				return
			}
		}
		var isShow = this.Showing.contains(index)
		var layer = this.LayerObjects[object.layer]
		var gameObject = object.gameObject
		Util.AddChild(object.layerObject, gameObject)
		this.Showing.remove(index)
		this.Showing.add(index)
		layer.Showing.remove(index)
		layer.Showing.add(index)
		object.transform.SetAsLastSibling()				//transform放到最下面
		this.UpdateBlack()
		this.UpdateCollider()
		//匹配宽度
		// if (object.attribute.MatchWidth && UIWidth < UIVirtualWidth) {
		// 	var scale = UIWidth / UIVirtualWidth
		// 	Util.SetLocalScale(gameObject, scale, scale, scale)
		// }
		if (isShow) {
			//UI已经是打开状态，调用UpdateShow
			foreach (var pair in pairs(object.components)) {
				if (pair.value.UpdateShow != null) { pair.value.UpdateShow(args...) }
			}
			return gameObject
		}
		this.ShowModal()
		object.BeginShow()
		//完全打开UI
		var showFinished = () => {
			this.HideModal()
			object.EndShow()
			foreach (var pair in pairs(object.components)) {
				if (pair.value.OnShowFinished != null) { pair.value.OnShowFinished(args...) }
			}
			this.OnShowUIFinished(index)
		}
		var animation = object.PlayShowAnimation(showFinished)
		//打开UI回调
		foreach (var pair in pairs(object.components)) {
			var component = pair.value
			if (component.OnShow != null) {
				component.OnShow(args...) 
			}
		}
		this.OnShowUI(index)
		if (!animation) { showFinished() }
		return gameObject
	}
	function SortOpenQueue() {
		this.OpenQueue.sort((a, b) => {
			return this.Objects[a.index].queueSort - this.Objects[b.index].queueSort
		})
		var excludes = []
		this.OpenQueue.forEach((ui_OpenData) => {
			var queueExclude = this.Objects[ui_OpenData.index].queueExclude
			if (queueExclude != null && queueExclude.count() > 0 && this.OpenQueue.find((_) => { return queueExclude.contains(_.index) }) != null) {
				excludes.add(ui_OpenData)
			}
		})
		if (excludes.count() > 0) {this.OpenQueue -= excludes}
	}
	function Hide(index, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
		this.Hide_impl(index, false, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
	}
	function HideForce(index, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
		this.Hide_impl(index, true, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
	}
	/*
	index	名称
	args	参数
	force	强制关闭，立刻关闭
	delay	延迟关闭，强制关闭时此字段无效
	*/
	function Hide_impl(index, force, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
		if (index == null || index == "") { return; }
		if (!this.Showing.contains(index)) { return }
		var object = this.Objects[index]
		this.Showing.remove(index)
		this.LayerObjects[object.layer].Showing.remove(index)
		//关闭时把界面响应关闭,以免关闭时可以点击Button
		object.canvasGroup.blocksRaycasts = false
		object.isShowFinished = false
		var gameObject = object.gameObject
		//关闭动画播放完毕
		var hideFinished = () => {
			this.HideCollider()		//
			this.HideBlack()		//先调用一次HideBlack,防止 Black 被 Destroy 的时候 销毁掉
			this.HideModal()
			if (object.attribute.Destroy) {
				this.OnDestroyUI(index, gameObject)
				this.Objects.remove(index)
				Util.Destroy(gameObject)
			} else {
				Util.AddChild(this.RootHideUI, gameObject)
			}
			this.UpdateBlack()
			this.UpdateCollider()
			this.OnHideUIFinished(index)
		}
		if (object.animationActive) {
			if (object.animation != null && object.hideAnimation != null) {
				Util.PlayAnimator(object.animation, object.hideAnimation, hideFinished)
			} else {
				hideFinished()
			}
		} else {
			hideFinished()
		}
		this.OnHideUI(index)
		foreach (var pair in pairs(object.components)) {
			var component = pair.value
			if (component.Events != null) {
				component.Events.forEach(function(event) {
					EventDispatcher.RemoveListener(EventID[event], component[event].bind(component))
				})
			}
			if (component.OnHide != null) {
				component.OnHide(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) 
			}
		}
		if (object.queue) {
			this.OpeningUI.remove(index)
			//延迟一帧再顺序打开，方便处理同一帧打开的界面
			if (this.OpenQueue.length() > 0) {
				this.isParpareOpen = true					//在延迟的一帧内记录准备打开状态，这一状态下调用show 不会添加到OpeningUI
			}
			Run(() => {
				if (this.OpeningUI.length() == 0 && this.OpenQueue.length() > 0) {
					this.isParpareOpen = false
					var next = this.OpenQueue.popFirst()
					this.Show(next.index, next.args...)
				}
			})
		} else {
			var layer = this.LayerObjects[object.layer]
			if (layer.Showing.count() > 0) {
				var topUI = this.Objects[layer.Showing.last()]
				foreach (var pair in pairs(topUI.components)) {
					pair.value.OnFocus?.()
				}
			}
		}
	}
	function IsShow(index) {
		if (index == null || index == "") { return false; }
		return this.Showing.contains(index)
	}
	function HideArray(hides, ...args) {
		foreach (var pair in pairs(hides)) {
			this.HideForce(pair.value, args...)
		}
	}
	function HideAll() {
		this.QueueObjects = {}
		this.OpenQueue = []
		this.HideArray(clone(this.Showing))
	}
	function HideDefault() {
		this.HideLayer(this.DefaultLayer)
	}
	function HideLayer(layer) {
		var hides = []
		foreach (var pair in pairs(this.Showing)) {
			var index = pair.value
			if (this.Objects[index].layer == layer) {
				array.add(hides, index)
			}
        }
		this.HideArray(hides)
	}
	function UpdateBlack() {
		this.BlackObject = null
		this.HideBlack()
		for (var i = 0, this.MaxLayer) {
			var layer = this.LayerObjects[this.MaxLayer - i]
			var Showing = layer.Showing
			var count = Showing.length() - 1
			for (var j = 0, count) {
				var object = this.Objects[Showing[count - j]]
				if (object.backMode == 1 || object.backMode == 2) {
					Util.AddChild(object.transform, this.BlackBack)
					this.BlackBack.SetAsFirstSibling()
					this.BlackObject = object
					return
				}
			}
		}
	}

	function UpdateCollider() {
		this.ColliderObject = null
		this.HideCollider()
		for (var i = 0, this.MaxLayer) {
			var layer = this.LayerObjects[this.MaxLayer - i]
			var Showing = layer.Showing
			var count = Showing.length() - 1
			for (var j = 0, count) {
				var object = this.Objects[Showing[count - j]]
				if (object.backMode == 3 || object.backMode == 4) {
					Util.AddChild(object.transform, this.ColliderBack)
					this.ColliderBack.SetAsFirstSibling()
					this.ColliderObject = object
					return
				}
			}
		}
	}
	function Call(index, call, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
		if (this.Objects.containsKey(index)) {
			foreach (var com in pairs(this.Objects[index].components)) {
				var func = com.value[call]
				if (func != null) {
					return func.call(com.value, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
				}
			}
		}
	}
	function Get(index) {
		if (this.Objects.containsKey(index)) {
			return this.Objects[index].components.first()
		}
	}
	function GetValue(index, name) {
		if (this.Objects.containsKey(index)) {
			foreach (var com in pairs(this.Objects[index].components)) {
				var value = com.value[name]
				if (value != null) {
					return value
				}
			}
		}
		return null
	}
	function DestroyUI(key) {
		if (this.Showing.contains(key)) { return }
		var object = this.Objects[key]
		if (object == null) { return }
		this.HideCollider()
		this.HideBlack()
		this.Objects.remove(key)
		this.OnDestroyUI(key, object.gameObject)
		Util.Destroy(object.gameObject)
	}
	//卸载所有已关闭的UI
	function Shutdown(force) {
		this.HideCollider()
		this.HideBlack()
		this.OpenQueue = []
		this.OpeningUI = []
		var destroys = []
		this.Objects.forEach((key, object) => {
			if (!this.Showing.contains(key) && (force || !object.attribute.DontShutdown)) {
				destroys.add(key)
			}
		})
		destroys.forEach((key) => {
			var object = this.Objects[key]
			this.Objects.remove(key)
			this.OnDestroyUI(key, object.gameObject)
			Util.Destroy(object.gameObject)
		})
	}
	// 当按下返回键
	function OnEscape() {
		// 最后一个保留UI无法关闭
		if(this.Showing.count() > 1) {
			var index = this.Showing.count() - 1
			while (index >= 0) {
				var lastUIIndex = this.Showing[index]
				var object = this.Objects[lastUIIndex]
				//忽略 esc
				if (object.ignoreEscape) {
					index -= 1
					continue
				}
				//阻断esc
				if (object.blockEscape) {
					return true
				}
				//既没有忽略也没有阻断esc 则直接判断 OnEscape 函数
				if(object.components[0].OnEscape != null) {
					//返回false 忽略此UI的 esc , 返回 null 也会阻断
					if (object.components[0].OnEscape() == false) {
						index -= 1
						continue
					}
				} else {
					this.Hide(lastUIIndex)
				}
				return true
			}
		}
		return false
	}
	// 设置是否忽略返回键
	function SetIgnoreEscape(index, ignore) {
		var object = this.Objects[index]
		if (object != null) {
			object.ignoreEscape = ignore
		}
	}
}

/*  主动弹出界面排序
界面index                               QueueSort
UI_TUTORIAL                             -100
UI_STAGE								-99

UI_ENTER_MAP_ANIM_COVER					-98

UI_VIP_PAY								-90

UI_LOGIN_7_REWARD                       -89
UI_ANNIVERSARY_REWARD_20_RACE           -88
UI_ANNIVERSARY_REWARD_RACE              -88
UI_LOGIN_30_REWARD                      -87

UI_RETURN_GIFT                          -79
UI_RETURNING_VIEW                       -78

UI_TIMED_PACK                           -69

UI_PIGGY_BANK                           -59
UI_GEM_BANK                             -59

UI_BULLETIN								-39
UI_BULLETIN_OPEN						-38
UI_BLACKFRIDAY_OPEN						-37

UI_MAP_ENERGYRACE						-30

UI_NPC_SKIN_CARD_EXPIRE					-20
UI_MESSAGE_BOX_BIG_IMAGE_AUTOOPEN       -19
UI_BATTLEPASS_REWARD                    -18
UI_ORDER_RACE_FINISH					-17
UI_COOKING_RACE_FINISH					-16
UI_MAZE_RACE_FINISH						-15
UI_MERGE_RACE_FINISH					-14
UI_MINING_RACE_FINISH					-13
UI_ADS_CHAIN							-12
UI_COMMON_FINISH						-11


UI_THEME_MAP_REMIND                     200
*/