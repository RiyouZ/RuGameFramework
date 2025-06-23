using RuGameFramework.Core;
using System.Collections.Generic;


namespace RuGameFramework.System
{
	public class SystemContainer
	{
		private Dictionary<SystemType, IRuSystem> _sysDic;

		public SystemContainer() 
		{
			_sysDic = new Dictionary<SystemType, IRuSystem>();
		}

		public SystemContainer (int maxCapacity)
		{
			_sysDic = new Dictionary<SystemType, IRuSystem> (maxCapacity);
		}

		public IRuSystem GetSystem (SystemType type)
		{
			if (!_sysDic.TryGetValue(type, out IRuSystem sys))
			{
				return null;
			}

			return sys;
		}

		public void AddSystem (SystemType systemType)
		{
			if (_sysDic.ContainsKey(systemType))
			{
				return;
			}

			IRuSystem system = SystemRegistrar.Create(systemType);
			system.Init();
			_sysDic.Add(systemType, system);
		}

		public void RemoveSystem (SystemType systemType)
		{
			if (!_sysDic.TryGetValue(systemType, out IRuSystem sys))
			{
				return;
			}

			sys.Dispose();
			_sysDic.Remove(systemType);
		}

		public void Release ()
		{
			if (_sysDic == null)
			{
				return;
			}

			_sysDic.Clear ();
			_sysDic = null;
		}

	}
}

