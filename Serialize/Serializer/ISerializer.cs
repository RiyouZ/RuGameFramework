using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuGameFramework.Serialize
{

	public interface ISerializer { }
	public interface ISerializer<TByte> : ISerializer
	{
		// ���л�
		TByte Serialize<TData> (TData data);	

		// �����л�
		TData Deserialize<TData> (TByte tbyte);

		// �����л��������ж���
		void  DeserializeOverwrite<TData> (TByte tbyte, TData writeObj);


	}

}
