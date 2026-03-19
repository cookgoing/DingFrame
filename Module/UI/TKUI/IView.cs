namespace DingFrame.Module.TKUI
{
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.UIElements;

	public enum ViewState
	{
		Closed,
		Opened,
	}

	public enum ViewAniState
	{
		Idle,
		Opening,
		Closing,
	}


	public interface IView
	{
		GameObject Obj {get;}
		VisualElement Root {get;}
		ViewState State {get;}
		UIDocument Document{get;}
		bool IsObjCreated{get;}

		void AttachObj(GameObject obj, VisualElement root);
		void SetState(ViewState state);
		void OnOpen(params object[] args);
		void OnClose();
	}

	public interface IAniView : IView
	{
		ViewAniState AniState {get;}

		void SetAniState(ViewAniState aniState);

		Task PlayOpenAni();
		Task PlayCloseAni();

		void OnAftOpenAni();
		void OnPreCloseAni();
	}

	public interface IStackView : IView
	{
		void OnPause(IStackView toView);
		void OnResume(IStackView fromView);
	}


	public abstract class BaseView : IView
	{
		private UIDocument document;
		public GameObject Obj{get; protected set;}
		public VisualElement Root{get; protected set;}
		public ViewState State{get; protected set;}
		public UIDocument Document => document ??= Obj.GetComponent<UIDocument>();
		public bool IsObjCreated => Obj != null;

		public virtual void AttachObj(GameObject obj, VisualElement root)
		{
			Obj = obj;
			Root = root;
		}

		public void SetState(ViewState state) => State = state;

		public abstract void OnOpen(params object[] args);
		public abstract void OnClose();
	}

	public abstract class AniView : BaseView, IAniView
	{
		public ViewAniState AniState {get; protected set;}

		public void SetAniState(ViewAniState aniState) => AniState = aniState;

		public abstract Task PlayOpenAni();
		public abstract Task PlayCloseAni();
		public abstract void OnAftOpenAni();
		public abstract void OnPreCloseAni();
	}
}