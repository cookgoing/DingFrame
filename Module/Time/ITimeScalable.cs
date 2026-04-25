
namespace DingFrame.Module.Time
{
	using UnityEngine;

	public interface ITimeScalable : IUpdateCaller
	{
		GameTimer Timer {get;}
		float Scale {get;}

		new virtual void DFixedUpdate(float dt) => TFixedUpdate(dt * Timer.Scale * Scale);
		new virtual void DUpdate(float dt) => TUpdate(dt * Timer.Scale * Scale);
		new virtual void DLateUpdate(float dt) => TLateUpdate(dt * Timer.Scale * Scale);

		void TFixedUpdate(float dt);
		void TUpdate(float dt);
		void TLateUpdate(float dt);

		void SetTimeScale(float scale);
	}

	public abstract class TimeScalable : ITimeScalable
	{
		public virtual Order.Order UpdateOrder {get; protected set;} = Order.Order.CreateOrder(FrameConfigure.UPDATER_DEFAULT_ORDER);
		public virtual GameTimer Timer {get; protected set;}
		public virtual float Scale {get; protected set;} = 1;

		public virtual void SetTimeScale(float scale) => Scale = scale;

		public virtual void DFixedUpdate(float dt) => TFixedUpdate(dt * Timer.Scale * Scale);
		public virtual void DUpdate(float dt) => TUpdate(dt * Timer.Scale * Scale);
		public virtual void DLateUpdate(float dt) => TLateUpdate(dt * Timer.Scale * Scale);

		public virtual void TFixedUpdate(float dt) {}
		public virtual void TUpdate(float dt) {}
		public virtual void TLateUpdate(float dt) {}
	}

	public abstract class TimeScalableMono : MonoBehaviour, ITimeScalable
	{
		public virtual Order.Order UpdateOrder {get; protected set;} = Order.Order.CreateOrder(FrameConfigure.UPDATER_DEFAULT_ORDER);
		public virtual GameTimer Timer {get; protected set;}
		public virtual float Scale {get; protected set;} = 1;

		public virtual void SetTimeScale(float scale) => Scale = scale;

		public virtual void DFixedUpdate(float dt) => TFixedUpdate(dt * Timer.Scale * Scale);
		public virtual void DUpdate(float dt) => TUpdate(dt * Timer.Scale * Scale);
		public virtual void DLateUpdate(float dt) => TLateUpdate(dt * Timer.Scale * Scale);

		public virtual void TFixedUpdate(float dt) {}
		public virtual void TUpdate(float dt) {}
		public virtual void TLateUpdate(float dt) {}
	}
}