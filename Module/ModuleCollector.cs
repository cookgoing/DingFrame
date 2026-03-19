namespace DingFrame.Module
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	
	public sealed class ModuleCollector : Singleton<ModuleCollector>
	{
		internal HashSet<IModule> Modules{get; private set;}
		
		public static T GetModule<T>() where T : class, IModule
		{
			if (Instance == null) 
			{
				DLogger.Error("ModuleCollector has no instance.", "ModuleCollector");
				return null;
			}

			foreach(IModule moudle in Instance.Modules)
			{
				if (moudle == null || moudle is not T) continue;

				return moudle as T;
			}

			return null;
		}


		public ModuleCollector() => Modules = new HashSet<IModule>();

		public void AddModule(IModule updater) => Modules.Add(updater);
		public bool RemoveModule(IModule updater) => Modules.Remove(updater);

		public void ForEach(Action<IModule> action)
		{
			if (action == null)
			{
				DLogger.Error("action == null", "ModuleCollector");
				return;
			}

			var list = Modules.ToList();
			foreach (IModule moudle in list) action(moudle);
		}
	}
}