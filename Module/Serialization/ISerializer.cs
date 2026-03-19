namespace DingFrame.Module.Serialization
{
	using System.IO;
	using System.Threading.Tasks;

	public interface ISerializeEntity
	{
		void OnAftSerialize();
		void OnAftDeserialize();
	}

	public interface ISerializeValidator
	{
		bool CanSerialize(string path, ISerializeEntity obj, out int errorCode);
		bool CanDeserialize(string path, string content, out int errorCode);

		void HandleValidate(string path, string content);
	}

	public interface ISerializeValidator<T> : ISerializeValidator where T : ISerializeEntity
	{
		bool CanSerialize(T obj, out int errorCode);
	}

	public interface ISerializeEncryptor
	{
		void Encrypt(string plainText, FileStream outputFileStream);
		string Decrypt(FileStream inputFileStream);

		Task EncryptAsync(string plainText, FileStream outputFileStream);
		Task<string> DecryptAsync(FileStream inputFileStream);
	}

	public interface ISerializer
	{
		string SerializeContent<T>(T obj);
		T DeserializeContent<T>(string content);

		bool Serialize<T>(T obj, string filePath);
		T Deserialize<T>(string filePath);

		Task<bool> SerializeAsync<T>(T obj, string filePath);
		Task<T> DeserializeAsync<T>(string filePath);
	}
}
