namespace DingFrame
{
	public interface IUIDStrOwner
	{
		string UID { get; }
		
		void SetUID(string uid);
	}
}