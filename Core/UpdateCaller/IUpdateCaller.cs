namespace DingFrame
{
	public interface IUpdateCaller
	{
		Order.Order UpdateOrder { get; }

		virtual void DFixedUpdate(float dt) { }
		virtual void DUpdate(float dt) { }
		virtual void DLateUpdate(float dt) { }
	}
}
