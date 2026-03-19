namespace DingFrame.Module.Serialization
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Runtime.Serialization.Formatters.Binary;
	using DingFrame.Module.Event;

	// BinaryFormatter 反序列化的时候，有安全漏洞，容易被攻击者注入非法数据，从而控制应用逻辑，请尽量少用，最好不用
	public class BinaryReaderWriter : ISerializer
	{
		private readonly BinaryFormatter formatter;

		public BinaryReaderWriter() => formatter = new BinaryFormatter();
		
		public string SerializeContent<T>(T obj) => throw new NotImplementedException();

		public T DeserializeContent<T>(string content) => throw new NotImplementedException();


		public bool Serialize<T>(T obj, string filePath)
		{
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

			FileStream stream = null;
			try
			{
				stream = new(filePath, FileMode.Create);
				formatter.Serialize(stream, obj);
				(obj as ISerializeEntity)?.OnAftSerialize();
				return true;
			}
			catch (Exception ex)
			{
				DLogger.Error(ex.Message, "BinaryReaderWriter");
				return false;
			}
			finally
			{
				stream?.Close();
			}
		}

		public T Deserialize<T>(string filePath)
		{
			if (!File.Exists(filePath))
			{
				DLogger.Error($"filePath: {filePath} is not existed", "BinaryReaderWriter");
				return default;
			}

			FileStream stream = null;
			try
			{
				stream = new(filePath, FileMode.Open);
				T obj = (T)formatter.Deserialize(stream);
				(obj as ISerializeEntity)?.OnAftSerialize();
				return obj;
			}
			catch (Exception ex)
			{
				DLogger.Error(ex.Message, "BinaryReaderWriter");
				return default;
			}
			finally
			{
				stream?.Close();
			}
		}


		public async Task<bool> SerializeAsync<T>(T obj, string filePath)
		{
			await Task.CompletedTask;
			return Serialize(obj, filePath);
		}

		public async Task<T> DeserializeAsync<T>(string filePath)
		{
			await Task.CompletedTask;
			return Deserialize<T>(filePath);
		}
	}
}