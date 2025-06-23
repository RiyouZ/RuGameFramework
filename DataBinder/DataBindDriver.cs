using System.Collections.Generic;
using System.Reflection;
using System;
using RuGameFramework.Data;

namespace RuGameFramework.DataBind
{
	public static class DataBindDriver
	{
		private static BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static Dictionary<Type, IDataDriver> _driverCache = new Dictionary<Type, IDataDriver>();

		// ִ�����ݰ�
		public static void ExecDataBind (IDataDriver obj)
		{
			var type = obj.GetType();

			// �Ѱ󶨹� �����ٴΰ�
			if (_driverCache.TryGetValue(type, out IDataDriver driver))
			{
				return;
			}

			_driverCache.Add(type, obj);

			// ��ȡDriver�е��ֶ�
			var fieldList = type.GetFields(flags);

			object[] funcArgs = new object[1];

			// ���������ֶ�
			for (int i = 0; i < fieldList.Length; i++)
			{
				// ��ȡ�ֶε�����
				var attr = fieldList[i].GetCustomAttribute<DataPropertyAttribute>();
				if (attr == null)
				{
					continue;
				}
				
				// ��ȡ�󶨵�����
				var data = DataRegistrar.GetData(attr.TargetType);
				if (data == null)
				{
					continue;
				}

				// ��ȡ��Ӧ�������ֶ�
				FieldInfo field = attr.TargetType.GetField(attr.Field);
				if (field == null)
				{
					continue;
				}

				// ��ȡDataProperty���Ե�Get
				object dataValue = field.GetValue(data);
				if (dataValue == null)
				{
					continue;
				}

				// ��ȡ�󶨵����
				object comValue = fieldList[i].GetValue(obj);
				if (comValue == null)
				{
					continue;
				}

				MethodInfo bindFunc = field.FieldType.GetMethod("BindToUI", flags);
				funcArgs[0] = comValue;
				bindFunc.Invoke(dataValue, funcArgs);
			}
		}

		// ���
		public static void UnBindData (IDataDriver obj)
		{
			var type = obj.GetType();

			// δ�󶨹� ������
			if (!_driverCache.TryGetValue(type, out IDataDriver driver))
			{
				return;
			}

			// ��ȡDriver�е��ֶ�
			var fieldList = type.GetFields(flags);

			// ���������ֶ�
			for (int i = 0; i < fieldList.Length; i++)
			{
				// ��ȡ�ֶε�����
				var attr = fieldList[i].GetCustomAttribute<DataPropertyAttribute>();
				if (attr == null)
				{
					continue;
				}

				// ��ȡ�󶨵�����
				var data = DataRegistrar.GetData(attr.TargetType);
				if (data == null)
				{
					continue;
				}

				// ��ȡ��Ӧ�������ֶ�
				FieldInfo field = attr.TargetType.GetField(attr.Field);
				if (field == null)
				{
					continue;
				}

				// ��ȡDataProperty���Ե�Get
				object dataValue = field.GetValue(data);
				if (dataValue == null)
				{
					continue;
				}

				MethodInfo bindFunc = field.FieldType.GetMethod("UnBindUI", flags);
				bindFunc.Invoke(dataValue, null);
			}
		}
	}
}


