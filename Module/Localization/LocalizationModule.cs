namespace DingFrame.Module.Localization
{
	using System;
	using System.Threading.Tasks;
	using UnityEngine;
	using DingFrame.Order;
	using DingFrame.Module;
	using DingFrame.Module.Event;
	
	public interface ILocalizationHandler
	{
		string DefaultStr{get;}
		string DefaultImgPath{get;}

		Task LoadLanguageFile(int languageId);
		Task ReleaseLanguageFile(int languageId);

		string GetStr(int hashID);
		string GetImgPath(int hashID);
	}

	public sealed class LocalizationModule : IModule, IGameStateListener
	{
		private Order listenOrder = Order.CreateOrder(FrameConfigure.GAMESTATE_DEFAULT_ORDER);
		public Order ListenOrder => listenOrder;

		private static LocalizationModule single;

		public static string LanguageStr(string str) => str;
		public static string LanguageImgPath(string path) => path;
		public static string GetStr(int hashID)
		{
			single ??= ModuleCollector.GetModule<LocalizationModule>();
			if (single == null) throw new Exception("[LocalizationModule.GetStr] no LocalizationModule");
			if (single.handler == null) throw new Exception("[LocalizationModule.GetStr] no handler");

			return single.handler.GetStr(hashID);
		}
		public static string GetImgPath(int hashID)
		{
			single ??= ModuleCollector.GetModule<LocalizationModule>();
			if (single == null) throw new Exception("[LocalizationModule.GetStr] no LocalizationModule");
			if (single.handler == null) throw new Exception("[LocalizationModule.GetStr] no handler");

			return single.handler.GetImgPath(hashID);
		} 

		private ILocalizationHandler handler;
		public int? LanguageId { get; private set; }

		public void GameEnter() => LanguageId = PlayerPrefs.GetInt(FrameConfigure.LANGUAGE_KEY, 0);
		
		public void SetLocalizationHandler(ILocalizationHandler handler) => this.handler = handler;

		public async Task<bool> ChangeLanuage(int newLanguageId)
		{
			int? preLanguageId = LanguageId;
			if (newLanguageId == preLanguageId) return false;

			LanguageId = newLanguageId;
			PlayerPrefs.SetInt(FrameConfigure.LANGUAGE_KEY, LanguageId.Value);
			if (preLanguageId.HasValue) await handler?.ReleaseLanguageFile(preLanguageId.Value);
			await handler?.LoadLanguageFile(LanguageId.Value);

			ModuleCollector.GetModule<EventModule>().Trigger(FrameEventKey.ChangeLanguage, new object[] { preLanguageId, LanguageId });
			return true;
		}
	}
}
