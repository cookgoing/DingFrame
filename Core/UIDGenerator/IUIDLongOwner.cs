namespace DingFrame
{
	public interface IUIDLongOwner
	{
		long UID { get; }
		
		void SetUID(long uid);
	}
}