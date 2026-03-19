namespace DingFrame.Module.Context
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using DingFrame.Module.Event;

	public class ContextModule : IModule
	{
		internal Dictionary<string, IContext> ContextDic {get; private set;}
		public IContext CurContext {get; private set;}

		public void Init() => ContextDic = new(10);


		public T GetContext<T>() where T : IContext
		{
			foreach(IContext context in ContextDic.Values)
			{
				if (context.GetType() != typeof(T)) continue;

				return (T)context;
			}

			return default;
		}

		public IContext GetContext(string contextName)
		{
			if (!ContextDic.ContainsKey(contextName)) return null;

			return ContextDic[contextName];
		}


		public bool AddContext(IContext context)
		{
			if (ContextDic.ContainsKey(context.Name))
			{
				DLogger.Error($"context is already added. name: {context.Name}", "ContextModule");
				return false;
			}

			ContextDic.Add(context.Name, context);
			return true;
		}

		public bool RemoveContext(IContext context)
		{
			if (!ContextDic.ContainsKey(context.Name)) return false;

			ContextDic.Remove(context.Name);
			return true;
		}
	
		
		public async Task MoveToContextAsync(IContext context, params object[] args)
		{
			if (context == default)
			{
				DLogger.Error("the context that want to move. is null");
				return;
			}

			IContext preContext = CurContext;
			await (preContext?.LeaveAsync(context) ?? Task.CompletedTask);

			CurContext = context;
			await CurContext.EnterAsync(preContext, args);

			ModuleCollector.GetModule<EventModule>().Trigger(FrameEventKey.ChangeContext, new object[]{preContext, CurContext});
		}
	
		public async Task MoveToContextAsync<T>(params object[] args) where T : IContext
		{
			IContext context = GetContext<T>();
			if (context == default) return;

			await MoveToContextAsync(context, args);
		}
	}
}