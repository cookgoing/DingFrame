namespace DingFrame.Module.UUI
{
	using System.Threading.Tasks;
	using UnityEngine;

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
		Transform Root {get;}
		Canvas Canvas {get;}
		ViewState State {get;}
		bool IsObjCreated{get;}

		bool Attach(Transform root);
		void Detach();
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
		public Transform Root{get; protected set;}
		public Canvas Canvas{get; protected set;}
		public ViewState State{get; protected set;}
		public bool IsObjCreated => Root != null;

		public virtual bool Attach(Transform root)
		{
			if (!root)
			{
				DLogger.Error($"no root. type: {GetType()}");
				return false;
			}
			
			Root = root;
			Canvas = root != null ? root.GetComponent<Canvas>() : null;
			if (!Canvas)
			{
				DLogger.Error($"the view [{GetType()}] has no Canvas");
				return false;
			}
			
			return true;
		}
		public virtual void Detach()
		{
			Root = null;
			Canvas = null;
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