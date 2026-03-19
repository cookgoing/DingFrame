namespace DingFrame.Module.Serialization
{
	using System;
	using System.Threading.Tasks;

	public class ProtoRederWriter : ISerializer
	{
		// todo: 等我后面开始引入 Proto 的时候再做这些
		public string SerializeContent<T>(T obj)
		{
			throw new NotImplementedException();
		}

		public T DeserializeContent<T>(string content)
		{
			throw new NotImplementedException();
		}


		public bool Serialize<T>(T obj, string filePath)
		{
			throw new NotImplementedException();
		}

		public T Deserialize<T>(string filePath)
		{
			throw new NotImplementedException();
		}


		public Task<bool> SerializeAsync<T>(T obj, string filePath)
		{
			throw new NotImplementedException();
		}

		public Task<T> DeserializeAsync<T>(string filePath)
		{
			throw new NotImplementedException();
		}
	}
}