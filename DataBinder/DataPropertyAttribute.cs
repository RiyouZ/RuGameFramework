using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.DataBind
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class DataPropertyAttribute :  Attribute
	{
		// �����·��
		private Type _targetType;
		public Type TargetType => _targetType;
		// ��󶨵�����
		private string _field;
		public string Field => _field;

		public DataPropertyAttribute (Type type, string field)
		{
			_targetType = type;
			_field = field;
		}

		public DataPropertyAttribute (string type, string field)
		{
			_targetType = Reflection.ReflectionUtility.GetTypeForAssembly(type);
			_field = field;
		}
	}
}

	