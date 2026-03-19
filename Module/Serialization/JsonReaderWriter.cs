namespace DingFrame.Module.Serialization
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Reflection;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using Newtonsoft.Json.Serialization;
	using DingFrame.Module.Event;
	using DingFrame.Utils;

	public class PrivateSetterContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			if (!property.Writable)
			{
				var propertyInfo = member as PropertyInfo;
				if (propertyInfo != null)
				{
					var hasPrivateSetter = propertyInfo.GetSetMethod(true) != null;
					property.Writable = hasPrivateSetter;
				}
			}

			return property;
		}
	}


	public class JsonReaderWriter : ISerializer
	{
		public JsonSerializerSettings Settings{get; private set;}
		private EventModule eventModule;

		public JsonReaderWriter()
		{
			eventModule = ModuleCollector.GetModule<EventModule>();
			Settings = new JsonSerializerSettings();
			Settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			Settings.Converters.Add(new StringEnumConverter());
			Settings.ContractResolver = new PrivateSetterContractResolver();
			Settings.Formatting = Formatting.Indented;
		}

		public string SerializeContent<T>(T obj) => JsonConvert.SerializeObject(obj, Settings);

		public T DeserializeContent<T>(string content)
		{
			try
			{
				T obj = JsonConvert.DeserializeObject<T>(content, Settings);
				(obj as ISerializeEntity)?.OnAftDeserialize();
				return obj;
			}
			catch (Exception ex)
			{
				DLogger.Error(MiscUtils.ExceptionStr(ex), "JsonReaderWriter.DeserializeContent");
				return default;
			}
		}


		public bool Serialize<T>(T obj, string filePath)
		{
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

			StreamWriter writer = null;
			FileStream outputFileStream = null;
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
					outputFileStream = new(filePath, FileMode.Create);
					validator?.HandleValidate(filePath, content);
					if (encryptor != null) encryptor.Encrypt(content, outputFileStream);
					else (writer =  new StreamWriter(outputFileStream)).Write(content);
					entity?.OnAftSerialize();
					return true;
				}

				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, new string[]{$"filePath: {filePath}"}));
				return false;
			}
			catch (Exception ex)
			{
				DLogger.Error(MiscUtils.ExceptionStr(ex), "JsonReaderWriter");
				return false;
			}
			finally
			{
				writer?.Close();
				outputFileStream?.Close();
			}
		}

		public T Deserialize<T>(string filePath)
		{
			if (!File.Exists(filePath))
			{
				DLogger.Error($"filePath: {filePath} is not existed", "JsonReaderWriter");
				return default;
			}

			StreamReader reader = null;
			FileStream inputFileStream = null;
			try
			{
				Type type = typeof(T);
				ISerializeValidator validator = SerializeVECollector.Instance.GetValidator(type);
				ISerializeEncryptor encryptor = SerializeVECollector.Instance.GetEncryptor(type);

				inputFileStream = new(filePath, FileMode.Open);
				string content = null;
				if (encryptor != null) content = encryptor.Decrypt(inputFileStream);
				else content = (reader = new StreamReader(inputFileStream)).ReadToEnd();
				int errorCode = 0;
				bool canDeserialize = validator?.CanDeserialize(filePath, content, out errorCode) != false;
				if (canDeserialize) return DeserializeContent<T>(content);

				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, new string[]{$"filePath: {filePath}"}));
				return default;
			}
			catch (Exception ex)
			{
				DLogger.Error(MiscUtils.ExceptionStr(ex), "JsonReaderWriter");
				return default;
			}
			finally
			{
				reader?.Close();
				inputFileStream?.Close();
			}
		}


		public async Task<bool> SerializeAsync<T>(T obj, string filePath)
		{
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

			StreamWriter writer = null;
			FileStream outputFileStream = null;
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
					outputFileStream = new(filePath, FileMode.Create);
					validator?.HandleValidate(filePath, content);
					if (encryptor != null) await encryptor.EncryptAsync(content, outputFileStream);
					else await (writer = new StreamWriter(outputFileStream)).WriteAsync(content);
					entity?.OnAftSerialize();
					return true;
				}
				
				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, new string[]{$"filePath: {filePath}"}));
				return false;
			}
			catch (Exception ex)
			{
				DLogger.Error(MiscUtils.ExceptionStr(ex), "JsonReaderWriter");
				return false;
			}
			finally
			{
				if (writer != null)
				{
					await writer.FlushAsync();
					await writer.DisposeAsync();
				}

				await outputFileStream.DisposeAsync();
			}
		}

		public async Task<T> DeserializeAsync<T>(string filePath)
		{
			if (!File.Exists(filePath))
			{
				DLogger.Error($"filePath: {filePath} is not existed", "JsonReaderWriter");
				return default;
			}

			StreamReader reader = null;
			FileStream inputFileStream = null;
			try
			{
				Type type = typeof(T);
				ISerializeValidator validator = SerializeVECollector.Instance.GetValidator(type);
				ISerializeEncryptor encryptor = SerializeVECollector.Instance.GetEncryptor(type);

				inputFileStream = new(filePath, FileMode.Open);
				string content = null;
				if (encryptor != null) content = await encryptor.DecryptAsync(inputFileStream);
				else content = await (reader = new StreamReader(inputFileStream)).ReadToEndAsync();
				int errorCode = 0;
				bool canDeserialize = validator?.CanDeserialize(filePath, content, out errorCode) != false;
				if (canDeserialize) return DeserializeContent<T>(content);

				if (errorCode > 0) eventModule?.Trigger(FrameEventKey.PushErrorCode, new ErrorCodeInfo(errorCode, new string[]{$"filePath: {filePath}"}));
				return default;
			}
			catch (Exception ex)
			{
				DLogger.Error(MiscUtils.ExceptionStr(ex), "JsonReaderWriter");
				return default;
			}
			finally
			{
				reader?.Close();
				inputFileStream?.Close();
			}
		}
	}
}