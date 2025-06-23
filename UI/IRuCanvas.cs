using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuGameFramework.UI
{
	public interface IRuCanvas
	{
		// �󶨵�UI��Ϸ���� 
		public GameObject UIGameObject {get;}
		// UI�����·��
		public string CanvasPath {set; get;}
		// �ɽ�����
		public bool IsInteractable {set; get;}
		public Transform CanvasTs {get;}
		// UI�㼶��
		public string UILayer {set; get;}
		// UI��SortOrder
		public int UIOrder {set; get;}

		// ��ʼ��
		public void Init ();

		// ����UI
		public void UIReset ();

		// ����
		public void UIDestory ();

		// ��ʾ
		public void Show ();

		// ����
		public void Hide ();
		
		public void ShowFast ();
		public void HideFast ();
		
		// ���ò���
		public void Configure (ICanvasArgs args);

	}
}

