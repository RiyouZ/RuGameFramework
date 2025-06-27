using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace RuDialog.Flag
{
	[CreateAssetMenu(fileName = "ScriptableFlag", menuName = "Dialog / Flag / ScriptableFlag")]
	public class ScriptableFlag : ScriptableObject
	{
		public List<string> flagList = new List<string>();

		private StringBuilder _sb = new StringBuilder();

		public static ScriptableFlag Create ()
		{
			var flag = CreateInstance<ScriptableFlag>();
			flag.Initialize();
			return flag;
		}

		public void Initialize ()
		{

		}

		public void Add ()
		{

		}

		public void Remove ()
		{

		}

		public void Has ()
		{

		}

		public string GetHash ()
		{
			_sb.Clear();
			_sb.AppendJoin('_', flagList);
			return _sb.ToString();
		}

		public void Clear ()
		{

		}

	}

}
