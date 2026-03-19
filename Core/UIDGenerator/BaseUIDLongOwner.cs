namespace DingFrame
{
	public class BaseUIDLongOwner : IUIDLongOwner
	{
		public long UID { get; protected set;}

		public virtual void SetUID(long uid) => UID = uid;
		public virtual void GenerateUID() => SetUID(UIDGenerator.GenerateSnowID());
	}
}