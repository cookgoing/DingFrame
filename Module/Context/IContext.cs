namespace DingFrame.Module.Context
{
	using System.Threading.Tasks;

	public interface IContext
	{
		string Name { get; }

	 	Task EnterAsync(IContext fromContext, params object[] args);
		Task LeaveAsync(IContext toContext);
	}
}
