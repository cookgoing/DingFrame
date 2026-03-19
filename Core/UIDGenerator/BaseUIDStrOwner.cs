namespace DingFrame
{
	public class BaseUIDStrOwner : IUIDStrOwner
	{
		public string UID { get; protected set;}

		public virtual void SetUID(string uid) => UID = uid;
		public virtual void GenerateUID() => SetUID(UIDGenerator.GenerateTimeStrID());
	}
}