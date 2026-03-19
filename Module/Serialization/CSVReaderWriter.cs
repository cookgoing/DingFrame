namespace DingFrame.Module.Serialization
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Text;
	using DingFrame.Module.Event;

	public class CSVReaderWriter : ISerializer
	{
		public interface CSVConversion<T>
		{
			string ToString();
			void FromString(string content);
		}

		private EventModule eventModule;

		public CSVReaderWriter() => eventModule = ModuleCollector.GetModule<EventModule>();

		public string SerializeContent<T>(T obj)
		{
			if (obj is not CSVConversion<T> csvConversion)
			{
				DLogger.Error("obj is not CSVConversion", "CSVReaderWriter");
				return null;
			}

			return csvConversion.ToString();
		}

		public T DeserializeContent<T>(string content)
		{
			T obj;
			if (typeof(T).IsValueType || typeof(T) == typeof(string)) obj = default;
			else obj = (T)Activator.CreateInstance(typeof(T));

			if (obj is not CSVConversion<T> csvConversion)
			{
				DLogger.Error("obj is not CSVConversion", "CSVReaderWriter");
				return obj;
			}

			csvConversion.FromString(content);
			(obj as ISerializeEntity)?.OnAftDeserialize();
			return obj;
		}


		public bool Serialize<T>(T obj, string filePath)
		{
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

			if (obj is not CSVConversion<T> csvConversion)
			{
				DLogger.Error("obj is not CSVConversion", "CSVReaderWriter");
				return false;
			}

			StreamWriter writer = null;
			try
			{
				ISerializeEntity entity = obj as ISerializeEntity;
				Type type = obj.GetType();
				ISerializeValidator validator = SerializeVECollector.Instance.GetValidator(type);
				ISerializeEncryptor encryptor = SerializeVECollector.Instance.GetEncryptor(type);
				int errorCode = 0;
				bool canSerialize = entity == null || validator?.CanSerialize(filePath, entity, out errorCode) != false;
				if (canSerialize)
				{
					string content = SerializeContent(obj);
					using FileStream outputFileStream = new(filePath, FileMode.Create);
					validator?.HandleValidate(filePath, content);
					if (encryptor != null) encryptor.Encrypt(content, outputFileStream);
					else (writer = new StreamWriter(outputFileStream)).Write(content);
					entity?.OnAftSerialize();
					return true;
				}

				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, null));
				return false;
			}
			catch (Exception ex)
			{
				DLogger.Error(ex.Message, "CSVReaderWriter");
				return false;
			}
			finally
			{
				writer?.Close();
			}
		}

		public T Deserialize<T>(string filePath)
		{
			if (!File.Exists(filePath))
			{
				DLogger.Error($"filePath: {filePath} is not existed", "CSVReaderWriter");
				return default;
			}

			StreamReader reader = null;
			try
			{
				Type type = typeof(T);
				ISerializeValidator validator = SerializeVECollector.Instance.GetValidator(type);
				ISerializeEncryptor encryptor = SerializeVECollector.Instance.GetEncryptor(type);

				using FileStream inputFileStream = new(filePath, FileMode.Open);
				string content = null;
				if (encryptor != null) content = encryptor.Decrypt(inputFileStream);
				else content = (reader = new StreamReader(inputFileStream)).ReadToEnd();

				int errorCode = 0;
				bool canDeserialize = validator?.CanDeserialize(filePath, content, out errorCode) != false;
				if (canDeserialize) return DeserializeContent<T>(content);

				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, null));
				return default;
			}
			catch (Exception ex)
			{
				DLogger.Error(ex.Message, "JsonReaderWriter");
				return default;
			}
			finally
			{
				reader?.Close();
			}
		}


		public async Task<bool> SerializeAsync<T>(T obj, string filePath)
		{
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

			if (obj is not CSVConversion<T> csvConversion)
			{
				DLogger.Error("obj is not CSVConversion", "CSVReaderWriter");
				return false;
			}

			StreamWriter writer = null;
			try
			{
				ISerializeEntity entity = obj as ISerializeEntity;
				Type type = obj.GetType();
				ISerializeValidator validator = SerializeVECollector.Instance.GetValidator(type);
				ISerializeEncryptor encryptor = SerializeVECollector.Instance.GetEncryptor(type);
				int errorCode = 0;
				bool canSerialize = entity == null || validator?.CanSerialize(filePath, entity, out errorCode) != false;
				if (canSerialize)
				{
					string content = SerializeContent(obj);
					using FileStream outputFileStream = new(filePath, FileMode.Create);
					validator?.HandleValidate(filePath, content);
					if (encryptor != null) await encryptor.EncryptAsync(content, outputFileStream);
					else await (writer = new StreamWriter(outputFileStream)).WriteAsync(content);
					entity?.OnAftSerialize();
					return true;
				}

				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, null));
				return false;
			}
			catch (Exception ex)
			{
				DLogger.Error(ex.Message, "CSVReaderWriter");
				return false;
			}
			finally
			{
				writer?.Close();
			}
		}

		public async Task<T> DeserializeAsync<T>(string filePath)
		{
			if (!File.Exists(filePath))
			{
				DLogger.Error($"filePath: {filePath} is not existed", "CSVReaderWriter");
				return default;
			}

			StreamReader reader = null;
			try
			{
				Type type = typeof(T);
				ISerializeValidator validator = SerializeVECollector.Instance.GetValidator(type);
				ISerializeEncryptor encryptor = SerializeVECollector.Instance.GetEncryptor(type);

				using FileStream inputFileStream = new(filePath, FileMode.Open);
				string content = null;
				if (encryptor != null) content = await encryptor.DecryptAsync(inputFileStream);
				else content = await (reader = new StreamReader(inputFileStream)).ReadToEndAsync();

				int errorCode = 0;
				bool canDeserialize = validator?.CanDeserialize(filePath, content, out errorCode) != false;
				if (canDeserialize) return DeserializeContent<T>(content);

				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, null));
				return default;
			}
			catch (Exception ex)
			{
				DLogger.Error(ex.Message, "JsonReaderWriter");
				return default;
			}
			finally
			{
				reader?.Close();
			}
		}
	}
}