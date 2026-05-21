namespace DingFrame.Module.UUI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	using DingFrame.Order;
	using DingFrame.Module.Serialization;
	using DingFrame.Utils;

	public sealed class UUIModule : IModule, IGameStateListener
	{
		public const int LAYER_ORDER = 1000;

		public Order ListenOrder {get; private set;} = Order.CreateOrder(FrameConfigure.GAMESTATE_DEFAULT_ORDER);

		private SerializationModule serializationModule;
		public Transform Root {get; private set;}
		public Transform Views {get; private set;}
		public Camera UICam {get; private set;}
		public EventSystem EventSystem {get; private set;}
		internal Dictionary<UILayer, Transform> LayerTransDic{get; private set;}

		public Dictionary<Type, UIMetaInfo> UIMetaInfos{get; private set;}
		public Dictionary<UILayer, List<IView>> CreatedView{get; private set;}
		public Stack<IStackView> StackViews{get; private set;}
		public IStackView CurStackView => StackViews.Count > 0 ? StackViews.Peek() : null;
		public Dictionary<IView, ViewHandler> UIHandlerDic{get; private set;}
		public Dictionary<IView, IView> BGDic{get; private set;}//<view, bgView>

		public static UUIModule Create()
		{
			UUIModule module = new();
			module.CreateUIObj();
			return module;
		}
		private async void CreateUIObj()
		{
			string path = "Assets/AARes/ui_common/prefabs/UIModule.prefab";
			GameObject uiObj = await Addressables.InstantiateAsync(path).Task;
			GameObject gameLaunch = GameObject.FindWithTag(FrameConfigure.GAME_LAUNCH_TAG);
			Root = uiObj.transform;
			Views = Root.Find("Views");
			UICam = Root.Find("UICamera").GetComponent<Camera>();
			EventSystem = Root.Find("EventSystem").GetComponent<EventSystem>();
			LayerTransDic = new (Enum.GetValues(typeof(UILayer)).Length);
			Root.SetParent(gameLaunch != null ? gameLaunch.transform : null);
		
			Array enumArr = Enum.GetValues(typeof(UILayer));
			int idx = 0;
			foreach(object enumValue in enumArr)
			{
				string enumStr = Enum.GetName(typeof(UILayer), enumValue);
				GameObject layerObj = new ($"UI-{enumStr}", typeof(RectTransform));
				Canvas canvas = layerObj.AddComponent<Canvas>();
				RectTransform layerTran = layerObj.GetComponent<RectTransform>();

				layerTran.SetParent(Views);
				layerTran.StretchToParent();
				canvas.overrideSorting = true;
				canvas.sortingOrder = idx++ * LAYER_ORDER;
				canvas.vertexColorAlwaysGammaSpace = true;
				LayerTransDic.Add((UILayer)enumValue, layerTran);
			}
		}

		public void Init() 
		{
			GameStateListenerCollector.Instance.AddGameStateListener(this);

			UIMetaInfos = new(20);
			CreatedView = new(10);
			StackViews = new(10);
			UIHandlerDic = new (10);
			BGDic = new();
		}
		public void Dispose() 
		{
			UIMetaInfos.Clear();
			CreatedView.Clear();
			StackViews.Clear();
			UIHandlerDic.Clear();
			BGDic.Clear();

			UIMetaInfos = null;
			CreatedView = null;
			StackViews = null;
			UIHandlerDic = null;
			BGDic = null;

			GameStateListenerCollector.Instance.RemoveGameStateListener(this);
		}

		public void GameEnter() => serializationModule = ModuleCollector.GetModule<SerializationModule>();

		public void ParseMetaInfoFromPath(string filePath)
		{
			UIMetaInfo[] metaInfoArr = serializationModule.JsonReaderWriter.Deserialize<UIMetaInfo[]>(filePath);
			foreach(UIMetaInfo metaInfo in metaInfoArr)
			{
				Type type = Type.GetType(metaInfo.Type);
				UIMetaInfos[type] = metaInfo;
			}
		}
		public void ParseMetaInfoFromStr(string content)
		{
			UIMetaInfo[] metaInfoArr = serializationModule.JsonReaderWriter.DeserializeContent<UIMetaInfo[]>(content);
			foreach(UIMetaInfo metaInfo in metaInfoArr)
			{
				Type type = Type.GetType(metaInfo.Type);
				UIMetaInfos[type] = metaInfo;
			}
		}
	
		public V GetView<V>(UILayer? layer = null) where V : class, IView => GetView(typeof(V), layer) as V;
		public IView GetView(Type viewType, UILayer? layer = null)
		{
			List<IView> viewList = new(50);

			if (layer == null) foreach(var list in CreatedView.Values) viewList.AddRange(list);
			else if (!CreatedView.TryGetValue(layer.Value, out viewList)) return null;

			foreach(IView view in viewList) if (view.GetType() == viewType) return view;

			return null;
		}

		public async Task<V> OpenView<V>(ViewHandler uiHandler, params object[] args) where V : class, IView => (await OpenView(typeof(V), uiHandler, args)) as V;
		public async Task<IView> OpenView(Type viewType, ViewHandler uiHandler, params object[] args)
		{
			var viewInfo = await CreateView(viewType);
			if (viewInfo == null) return null;

			bool isSucess = true;
			IView view = viewInfo.Value.view;
			HandleOpenManipulator(view, uiHandler);
			await HanldeOpenMetaInfo(view);

			if (isSucess && view is IStackView stackView && !await HandleOpenStackView(stackView)) isSucess = false;
			if (isSucess && !HandleOpenView(view, args)) isSucess = false;
			if (isSucess && view is IAniView aniView && !await HandleOpenAniView(aniView)) isSucess = false;

			if (!isSucess)
			{
				if (view is IStackView stackView2 
				&& StackViews.TryPeek(out IStackView stackView3) 
				&& stackView2 == stackView3) StackViews.Pop();

				DestroyView(view);
				return null;
			}
			return view;
		}

		public async Task CloseView(IView view)
		{
			bool isSucess = true;
			if (isSucess && view is IAniView aniView && !await HandleCloseAniView(aniView)) isSucess = false;
			if (isSucess && !HandleCloseView(view)) isSucess = false;
			if (isSucess && view is IStackView stackView && !await HandleCloseStackView(stackView)) isSucess = false;
			
			if (!isSucess) return;

			if (!HanldeCloseMetaInfo(view)) return;
			HandleCloseManipulator(view);
			DestroyView(view);
		}
		// there is no statckView handle in here, you should handle it outside
		public void CloseViewInstantly(IView view)
		{
			if (view is IAniView aniView) 
			{
				aniView.OnPreCloseAni();
				aniView.SetAniState(ViewAniState.Idle);
			}
			if (!HandleCloseView(view)) return;

			if (!HanldeCloseMetaInfo(view)) return;
			HandleCloseManipulator(view);
			DestroyView(view);
		}

		public void ClearView(ICollection<IView> exceptView = null)
		{
			List<IStackView> willDeletedStackView = new (StackViews.Count);
			foreach (List<IView> viewList in CreatedView.Values)
				for (int i = viewList.Count - 1; i >= 0; --i)
				{
					IView view = viewList[i];
					if (exceptView?.Contains(view) ?? false) continue;

					if (view is IStackView stackView) willDeletedStackView.Add(stackView);
					CloseViewInstantly(view);
				}

			IStackView topView;
			Stack<IStackView> tmpStack = new (StackViews.Count);
			while (StackViews.TryPop(out topView))
			{
				if (willDeletedStackView.Contains(topView)) continue;
				tmpStack.Push(topView);
			}
			while(tmpStack.TryPop(out topView)) StackViews.Push(topView);
		}
		public void ClearView(params UILayer[] exceptLayers)
		{
			List<IStackView> willDeletedStackView = new (StackViews.Count);
			foreach (List<IView> viewList in CreatedView.Values)
				for (int i = viewList.Count - 1; i >= 0; --i)
				{
					IView view = viewList[i];
					if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo)) 
					{
						DLogger.Error($"[error][UUIModule.ClearView]. the view{view.GetType()} has no metaInfo");
						continue;
					}
					if (metaInfo.Layer == UILayer.Background) continue;
					if (Array.Exists(exceptLayers, item => item == metaInfo.Layer)) continue;

					if (view is IStackView stackView) willDeletedStackView.Add(stackView);
					CloseViewInstantly(view);
				}

			IStackView topView;
			Stack<IStackView> tmpStack = new (StackViews.Count);
			while (StackViews.TryPop(out topView))
			{
				if (willDeletedStackView.Contains(topView)) continue;
				tmpStack.Push(topView);
			}
			while(tmpStack.TryPop(out topView)) StackViews.Push(topView);
		}

		public void ClearStack()
		{
			while (StackViews.TryPop(out IStackView topView)) 
				CloseViewInstantly(topView);
		}

		public void HideView(params UILayer[] exceptLayers)
		{
			foreach (List<IView> viewList in CreatedView.Values)
			{
				for (int i = viewList.Count - 1; i >= 0; --i)
				{
					IView view = viewList[i];
					if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo))
					{
						DLogger.Error($"[error][UUIModule.HideView]. the view{view.GetType()} has no metaInfo");
						continue;
					}
					if (Array.Exists(exceptLayers, item => item == metaInfo.Layer)) continue;

					view.Root.gameObject.SetActive(false);
				}
			}
		}
		public void ShowView(params UILayer[] exceptLayers)
		{
			foreach (List<IView> viewList in CreatedView.Values)
			{
				for (int i = viewList.Count - 1; i >= 0; --i)
				{
					IView view = viewList[i];
					if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo))
					{
						DLogger.Error($"[error][UUIModule.HideView]. the view{view.GetType()} has no metaInfo");
						continue;
					}
					if (Array.Exists(exceptLayers, item => item == metaInfo.Layer)) continue;

					view.Root.gameObject.SetActive(true);
				}
			}
		}

		private async Task<(UIMetaInfo metaInfo, V view)?> CreateView<V>() where V : IView
		{
			Type viewType = typeof(V);
			var result = await CreateView(viewType);
			if (result == null) return null;

			return (result.Value.metaInfo, (V)result.Value.view);
		}
		private async Task<(UIMetaInfo metaInfo, IView view)?> CreateView(Type viewType)
		{
			if (!UIMetaInfos.TryGetValue(viewType, out UIMetaInfo metaInfo))
			{
				DLogger.Error($"no metaInfo. viewType: {viewType}", "UUIModule");
				return null;
			}

			if (!LayerTransDic.TryGetValue(metaInfo.Layer, out Transform layerTransform))
			{
				DLogger.Error($"HanldeMetaInfo failed. no this layer trans: {metaInfo.Layer}", "UUIModule");
				return null;
			}

			GameObject obj = await Addressables.InstantiateAsync(metaInfo.Path, layerTransform).Task;
			try
			{
				UILayer layer = metaInfo.Layer;
				Canvas canvas = obj.AddComponentIfNo<Canvas>();
				IView view = (IView)Activator.CreateInstance(viewType);
				view.Attach(obj.transform);
				canvas.overrideSorting = true;
				canvas.sortingOrder = metaInfo.Order;

				if (!CreatedView.TryGetValue(layer, out List<IView> viewList))
				{
					viewList = new();
					CreatedView.Add(layer, viewList);
				}
				viewList.Add(view);

				return (metaInfo, view);
			}
			catch (Exception ex)
			{
				DLogger.Error($"create View failed. \n ex: {ex.Message}\n		{ex.StackTrace}", "UUIModule");
				Addressables.ReleaseInstance(obj);
				return null;
			}
		}
		private void DestroyView(IView view)
		{
			if (!view.IsObjCreated) return;

			Addressables.ReleaseInstance(view.Root.gameObject);
			view.Detach();

			if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo)) return;
			if (!CreatedView.TryGetValue(metaInfo.Layer, out List<IView> viewList)) return;

			viewList.Remove(view);
		}
	
		private bool HandleOpenManipulator(IView view, ViewHandler uiHandler)
		{
			if (uiHandler == null) return false;

			uiHandler.AttachView(view);
			UIHandlerDic[view] = uiHandler;
			return true;
		}
		private bool HandleCloseManipulator(IView view)
		{
			if (!UIHandlerDic.TryGetValue(view, out ViewHandler uiHandler)) return false;

			uiHandler.DetachView();
			UIHandlerDic.Remove(view);
			return true;
		}
	
		private async Task<bool> HanldeOpenMetaInfo(IView view)
		{
			if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo))
			{
				DLogger.Error($"no metaInfo. viewType: {view.GetType()}", "UUIModule");
				return false;
			}

			if (metaInfo.IsFullScreen)
			{
				if (CreatedView.TryGetValue(UILayer.MainView, out List<IView> mainViewList))
					foreach (IView mainView in mainViewList) 
						if (mainView != view)
							mainView.Root.gameObject.SetActive(false);

				foreach (var kv in BGDic)
				{
					IView ownerView = kv.Key;
					IView blackView = kv.Value;
					if (blackView == view) continue;
					if (UIMetaInfos.TryGetValue(ownerView.GetType(), out UIMetaInfo ownerMetaInfo) && ownerMetaInfo.Layer > metaInfo.Layer) continue;
					
					blackView.Root.gameObject.SetActive(false);
				}
			}

			if (!string.IsNullOrEmpty(metaInfo.BackgroundType))
			{
				Type backgroundType = Type.GetType(metaInfo.BackgroundType);
				IView bgView = await OpenView(backgroundType, null);
				BGDic[view] = bgView;
				if (UIHandlerDic.TryGetValue(view, out ViewHandler uiHandler)) uiHandler.AttachBgView(bgView);
			}
			return true;
		}
		private bool HanldeCloseMetaInfo(IView view)
		{
			if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo))
			{
				DLogger.Error($"no metaInfo. viewType: {view.GetType()}", "UUIModule");
				return false;
			}

			if (metaInfo.IsFullScreen)
			{
				if (CreatedView.TryGetValue(UILayer.MainView, out List<IView> mainViewList))
					foreach (IView mainView in mainViewList)
						if (mainView != view)
							mainView.Root.gameObject.SetActive(true);

				foreach (var kv in BGDic)
				{
					IView ownerView = kv.Key;
					IView blackView = kv.Value;
					if (blackView == view) continue;
					if (UIMetaInfos.TryGetValue(ownerView.GetType(), out UIMetaInfo ownerMetaInfo) && ownerMetaInfo.Layer > metaInfo.Layer) continue;
					
					blackView.Root.gameObject.SetActive(false);
				}
			}

			if (BGDic.TryGetValue(view, out IView bgView))
			{
				if (UIHandlerDic.TryGetValue(view, out ViewHandler uiHandler)) uiHandler.DetachBgView();
				BGDic.Remove(view);
				CloseView(bgView).Forget();
			}
			return true;
		}

		private bool HandleOpenView(IView view, params object[] args)
		{
			if (view?.State != ViewState.Closed)
			{
				DLogger.Error($"HandleOpenView failed. view state: {view?.State}");
				return false;
			}

			view.SetState(ViewState.Opened);
			view.OnOpen(args);
			return true;
		}
		private bool HandleCloseView(IView view)
		{
			if (view?.State != ViewState.Opened)
			{
				DLogger.Error($"HandleCloseView failed. view state: {view?.State}; view:{view?.GetType()}");
				return false;
			}

			view.SetState(ViewState.Closed);
			view.OnClose();
			return true;
		}
	
		private async Task<bool> HandleOpenAniView(IAniView aniView)
		{
			if (aniView?.AniState != ViewAniState.Idle)
			{
				DLogger.Error($"HandleOpenAniView failed. Ani state: {aniView?.AniState}");
				return false;
			}

			aniView.SetAniState(ViewAniState.Opening);
			await aniView.PlayOpenAni();
			aniView.SetAniState(ViewAniState.Idle);
			aniView.OnAftOpenAni();
			return true;
		}
		private async Task<bool> HandleCloseAniView(IAniView aniView)
		{
			if (aniView?.AniState != ViewAniState.Idle)
			{
				DLogger.Error($"HandleCloseAniView failed. Ani state: {aniView?.AniState}");
				return false;
			}

			aniView.OnPreCloseAni();
			aniView.SetAniState(ViewAniState.Closing);
			await aniView.PlayCloseAni();
			aniView.SetAniState(ViewAniState.Idle);
			return true;
		}
	
		private async Task<bool> HandleOpenStackView(IStackView stackView)
		{
			if (stackView == null) return false;

			if (!StackViews.TryPeek(out IStackView topView)) goto End;
			if (topView == stackView) return false;

			if (topView is AniView topAniView) await topAniView.PlayCloseAni();
			topView.OnPause(stackView);
			
		End:
			StackViews.Push(stackView);
			return true;
		}
		private async Task<bool> HandleCloseStackView(IStackView stackView)
		{
			if (stackView == null) return false;

			IStackView topView;
			while (StackViews.TryPop(out topView) && topView != stackView)
				CloseViewInstantly(topView);

			if (topView == null) goto End;
			if (!StackViews.TryPeek(out topView)) return true;
			
			topView.OnResume(topView);
			if (topView is AniView topAniView) await topAniView.PlayOpenAni();

		End:
			return true;
		}
	}
}