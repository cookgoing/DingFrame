
namespace DingFrame
{
    public sealed partial class ErrorCode
    {
		public const int HashDifferent = 1;//Hash值可以看作一个对象的唯一标识，当对象改变之后，Hash值也会改变
		public const int NoPath = 2;
		public const int SerializeFailed = 3;
		public const int DeserializeFailed = 4;
		public const int EmptyString = 5;

    }
}

