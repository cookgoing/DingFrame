namespace DingFrame.Module.Event
{

	public struct ErrorCodeInfo
	{
		public int ErrorCode{get; private set;}
		public string[] ErrorArgs{get; private set;}

		public ErrorCodeInfo(int errorCode, string[] errorArgs)
		{
			ErrorCode = errorCode;
			ErrorArgs = errorArgs;
		}
	}
}