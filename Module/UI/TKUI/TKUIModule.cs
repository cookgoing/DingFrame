namespace DingFrame.Module.TKUI
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.UIElements;
	using DingFrame.Order;
	using DingFrame.Module.Serialization;
	using DingFrame.Module.AssetLoader;

	public sealed class TKUIModule : MonoBehaviour, IModule, IGameStateListener
	{
		private Order listenOrder = Order.CreateOrder(FrameConfigure.GAMESTATE_DEFAULT_ORDER);
		public Order ListenOrder => listenOrder;

		private AssetLoadModule assetLoadModule;
		SerializationModule serializationModule;
		internal Dictionary<UILayer, Transform> LayerTransDic{get; private set;} = new (Enum.GetValues(typeof(UILayer)).Length);

		public Dictionary<Type, UIMetaInfo> UIMetaInfos{get; private set;}
		public Dictionary<UILayer, List<IView>> CreatedView{get; private set;}
		public Stack<IStackView> StackViews{get; private set;}
		public IStackView CurStackView => StackViews.Peek();
		public Dictionary<IView, ViewHandler> UIHandlerDic{get; private set;}
		public Dictionary<IView, IView> BGDic{get; private set;}//<view, bgView>
		

		public static TKUIModule Create()
		{
			GameObject uiObj = GameObject.FindGameObjectWithTag(FrameConfigure.TOOL_KIT_ROOTUI_TAG);
			TKUIModule module = null;
			if (uiObj != null && (module = uiObj.GetComponent<TKUIModule>()) != null)
			{
				DLogger.Error("TKUIModule is alread exist.", "TKUIModule");
				return module;
			}

			uiObj ??= new GameObject("TKUIModule");
			uiObj.tag = FrameConfigure.TOOL_KIT_ROOTUI_TAG;
			module = uiObj.AddComponent<TKUIModule>();
			DontDestroyOnLoad(uiObj);

			Array enumArr = Enum.GetValues(typeof(UILayer));
			foreach(object enumValue in enumArr)
			{
				string enumStr = Enum.GetName(typeof(UILayer), enumValue);
				GameObject layerObj = new GameObject($"UI-{enumStr}");

				layerObj.transform.parent = uiObj.transform;
				module.LayerTransDic.Add((UILayer)enumValue, layerObj.transform);
			}

			return module;
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


		public void GameEnter()
		{
			assetLoadModule = ModuleCollector.GetModule<AssetLoadModule>();
			serializationModule = ModuleCollector.GetModule<SerializationModule>();
		}


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
			List<IView> viewList = new List<IView>(50);

			if (layer == null) foreach(var list in CreatedView.Values) viewList.AddRange(list);
			else if (!CreatedView.TryGetValue(layer.Value, out viewList)) return null;

			foreach(IView view in viewList) if (view.GetType() == viewType) return view;

			return null;
		}


		public V OpenView<V>(ViewHandler uiHandler, params object[] args) where V : class, IView => OpenView(typeof(V), uiHandler, args) as V;
		public IView OpenView(Type viewType, ViewHandler uiHandler, params object[] args)
		{
			var viewInfo = CreateView(viewType);
			if (viewInfo == null) return null;

			IView view = viewInfo.Value.view;
			HandleOpenManipulator(view, uiHandler);
			HanldeOpenMetaInfo(view);

			if (!HandleOpenView(view, args)) return view;
			if (view is IAniView aniView) HandleOpenAniView(aniView).ConfigureAwait(false);
			if (view is IStackView stackView) HandleOpenStackView(stackView);
			return view;
		}

		public async Task CloseView(IView view)
		{
			Task aniViewTask = Task.CompletedTask;
			if (view is IStackView stackView) HandleCloseStackView(stackView);
			if (view is IAniView aniView) aniViewTask = HandleCloseAniView(aniView);
			if (!HandleCloseView(view)) return;

			if (!HanldeCloseMetaInfo(view)) return;
			HandleCloseManipulator(view);
			await aniViewTask;
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
			List<IStackView> willDeletedStackView = new List<IStackView>(StackViews.Count);
			foreach (List<IView> viewList in CreatedView.Values)
				for (int i = viewList.Count - 1; i >= 0; --i)
				{
					IView view = viewList[i];
					if (exceptView?.Contains(view) ?? false) continue;

					if (view is IStackView stackView) willDeletedStackView.Add(stackView);
					CloseViewInstantly(view);
				}

			IStackView topView;
			Stack<IStackView> tmpStack = new Stack<IStackView>(StackViews.Count);
			while (StackViews.TryPop(out topView))
			{
				if (willDeletedStackView.Contains(topView)) continue;
				tmpStack.Push(topView);
			}
			while(tmpStack.TryPop(out topView)) StackViews.Push(topView);
		}

		public void ClearView(params UILayer[] exceptLayers)
		{
			List<IStackView> willDeletedStackView = new List<IStackView>(StackViews.Count);
			foreach (List<IView> viewList in CreatedView.Values)
				for (int i = viewList.Count - 1; i >= 0; --i)
				{
					IView view = viewList[i];
					if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo)) 
					{
						DLogger.Error($"[error][TKUIModule.ClearView]. the view{view.GetType()} has no metaInfo");
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
						DLogger.Error($"[error][TKUIModule.HideView]. the view{view.GetType()} has no metaInfo");
						continue;
					}
					if (Array.Exists(exceptLayers, item => item == metaInfo.Layer)) continue;

					view.Root.style.display = DisplayStyle.None;
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
						DLogger.Error($"[error][TKUIModule.HideView]. the view{view.GetType()} has no metaInfo");
						continue;
					}
					if (Array.Exists(exceptLayers, item => item == metaInfo.Layer)) continue;

					view.Root.style.display = DisplayStyle.Flex;
				}
			}
		}


		private (UIMetaInfo metaInfo, V view)? CreateView<V>() where V : IView
		{
			Type viewType = typeof(V);
			var result = CreateView(viewType);
			if (result == null) return null;

			return (result.Value.metaInfo, (V)result.Value.view);
		}

		private (UIMetaInfo metaInfo, IView view)? CreateView(Type viewType)
		{
			if (!UIMetaInfos.TryGetValue(viewType, out UIMetaInfo metaInfo))
			{
				DLogger.Error($"no metaInfo. viewType: {viewType}", "TKUIModule");
				return null;
			}

			GameObject obj = new GameObject(viewType.Name);
			UIDocument uiDoc = obj.AddComponent<UIDocument>();
			PanelSettings panelSettings = assetLoadModule.Load<PanelSettings>(metaInfo.PanelSettingPath);
			VisualTreeAsset treeAsset = assetLoadModule.Load<VisualTreeAsset>(metaInfo.UXMLPath);
			uiDoc.panelSettings = panelSettings;
			uiDoc.visualTreeAsset = treeAsset;

			try
			{
				UILayer layer = metaInfo.Layer;
				IView view = (IView)Activator.CreateInstance(viewType);
				view.AttachObj(obj, uiDoc.rootVisualElement);

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
				DLogger.Error($"create View failed. \n ex: {ex.Message}\n		{ex.StackTrace}", "TKUIModule");
				Destroy(obj);
				return null;
			}
		}

		private void DestroyView(IView view)
		{
			if (!view.IsObjCreated) return;

			Destroy(view.Obj);
			view.AttachObj(null, null);

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

			uiHandler.AttachView(null);
			UIHandlerDic.Remove(view);
			return true;
		}


		private bool HanldeOpenMetaInfo(IView view)
		{
			if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo))
			{
				DLogger.Error($"no metaInfo. viewType: {view.GetType()}", "TKUIModule");
				return false;
			}

			if (!LayerTransDic.TryGetValue(metaInfo.Layer, out Transform layerTransform))
			{
				DLogger.Error($"HanldeMetaInfo failed. no this layer trans: {metaInfo.Layer}", "TKUIModule");
				return false;
			}

			int viewCount = CreatedView[metaInfo.Layer]?.Count ?? 0;

			view.Obj.transform.SetParent(layerTransform);
			view.Document.sortingOrder = (int)metaInfo.Layer * 1000 + viewCount * 10;

			if (metaInfo.IsFullScreen)
			{
				if (CreatedView.TryGetValue(UILayer.MainView, out List<IView> mainViewList))
				{
					foreach (IView mainView in mainViewList) if (mainView != view) mainView.Root.style.display = DisplayStyle.None;
				}

				foreach (var kv in BGDic)
				{
					IView ownerView = kv.Key;
					IView blackView = kv.Value;
					if (blackView == view) continue;
					if (UIMetaInfos.TryGetValue(ownerView.GetType(), out UIMetaInfo ownerMetaInfo) && ownerMetaInfo.Layer > metaInfo.Layer) continue;
					
					blackView.Root.style.display = DisplayStyle.None;
				}
			}

			if (!string.IsNullOrEmpty(metaInfo.BackgroundType))
			{
				Type backgroundType = Type.GetType(metaInfo.BackgroundType);
				IView bgView = OpenView(backgroundType, null);
				BGDic[view] = bgView;
				if (UIHandlerDic.TryGetValue(view, out ViewHandler uiHandler)) uiHandler.AttachBgView(bgView);
			}
			return true;
		}

		private bool HanldeCloseMetaInfo(IView view)
		{
			if (!UIMetaInfos.TryGetValue(view.GetType(), out UIMetaInfo metaInfo))
			{
				DLogger.Error($"no metaInfo. viewType: {view.GetType()}", "TKUIModule");
				return false;
			}

			if (metaInfo.IsFullScreen)
			{
				if (CreatedView.TryGetValue(UILayer.MainView, out List<IView> mainViewList))
				{
					foreach (IView mainView in mainViewList) if (mainView != view) mainView.Root.style.display = DisplayStyle.Flex;
				}

				foreach (var kv in BGDic)
				{
					IView ownerView = kv.Key;
					IView blackView = kv.Value;
					if (blackView == view) continue;
					if (UIMetaInfos.TryGetValue(ownerView.GetType(), out UIMetaInfo ownerMetaInfo) && ownerMetaInfo.Layer > metaInfo.Layer) continue;
					
					blackView.Root.style.display = DisplayStyle.Flex;
				}
			}

			if (BGDic.TryGetValue(view, out IView bgView))
			{
				if (UIHandlerDic.TryGetValue(view, out ViewHandler uiHandler)) uiHandler.AttachBgView(null);
				BGDic.Remove(view);
				_ = CloseView(bgView);
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
			if (UIHandlerDic.TryGetValue(view, out ViewHandler uiHandler)) uiHandler.OnViewOpen();
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


		private bool HandleOpenStackView(IStackView stackView)
		{
			if (stackView == null) return false;

			IStackView topView = StackViews.Peek();
			if (topView == stackView) return false;

			topView?.OnPause(stackView);
			
			StackViews.Push(stackView);
			return true;
		}

		private bool HandleCloseStackView(IStackView stackView)
		{
			if (stackView == null) return false;

			IStackView topView;
			while (StackViews.TryPop(out topView) && topView != stackView)
				CloseViewInstantly(topView);

			if (topView == null) return false;

			StackViews.Peek()?.OnResume(topView);
			return true;
		}

	}
}