namespace DingFrame.GameLaunch
{
	using System.Threading.Tasks;
	
	public interface ILauncherListener
	{
		Task OnGameLaunch();
	}
}