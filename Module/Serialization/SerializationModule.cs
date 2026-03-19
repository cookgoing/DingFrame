namespace DingFrame.Module.Serialization
{
	using System;
	using System.Collections.Generic;

	public sealed class SerializationModule : IModule
	{
		private CSVReaderWriter _CSVReaderWriter;
		private JsonReaderWriter _JsonReaderWriter;
		private ProtoRederWriter _ProtoRederWriter;

		public CSVReaderWriter CSVReaderWriter => _CSVReaderWriter ??= new CSVReaderWriter();
		public JsonReaderWriter JsonReaderWriter => _JsonReaderWriter ??= new JsonReaderWriter();
		public ProtoRederWriter ProtoRederWriter => _ProtoRederWriter ??= new ProtoRederWriter();
	}

	public sealed class SerializeVECollector : Singleton<SerializeVECollector>
	{
		public Dictionary<Type, ISerializeValidator> ValidatorDic{get; private set;}
		public Dictionary<Type, ISerializeEncryptor> EncryptorDic{get; private set;}

		public SerializeVECollector()
		{
			ValidatorDic = new();
			EncryptorDic = new();
		} 

		public void AddValidator(Type serializeEntityType, ISerializeValidator validator) => ValidatorDic[serializeEntityType] = validator;
		public bool RemoveValidator(Type serializeEntityType) => ValidatorDic.Remove(serializeEntityType);
		public void ClearValidator() => ValidatorDic.Clear();
		public ISerializeValidator GetValidator(Type serializeEntityType) => ValidatorDic.ContainsKey(serializeEntityType) ? ValidatorDic[serializeEntityType] : null;
	
	
		public void AddEncryptor(Type serializeEntityType, ISerializeEncryptor encryptor) => EncryptorDic[serializeEntityType] = encryptor;
		public bool RemoveEncryptor(Type serializeEntityType) => EncryptorDic.Remove(serializeEntityType);
		public void ClearEncryptor() => EncryptorDic.Clear();
		public ISerializeEncryptor GetEncryptor(Type serializeEntityType) => EncryptorDic.ContainsKey(serializeEntityType) ? EncryptorDic[serializeEntityType] : null;
	}
}