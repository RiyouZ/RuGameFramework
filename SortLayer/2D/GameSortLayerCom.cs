using RuGameFramework.Core;
using RuGameFramework.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GamePlay.Component
{
	public class GameSortLayerCom : MonoBehaviour, IRuComponent
	{
		public ComponentType ComType => ComponentType.SortingLayer;

		public GameObject BindObj => this.gameObject;

		private string _sortLayerName = string.Empty;
		public string SortLayerName
		{
			get
			{
				return _sortLayerName; 
			}
			set
			{
				_sortLayerName = value;

				if (_spriteRenderer != null)
				{
					_spriteRenderer.sortingLayerName = _sortLayerName;	
				}

				if (_meshRenderer != null)
				{
					_meshRenderer.sortingLayerName = _sortLayerName;
				}
			}
		}

		private SpriteRenderer _spriteRenderer;
		private MeshRenderer _meshRenderer;

		void Start ()
		{
			InitComponent();
		}

		void LateUpdate ()
		{
			UpdateSpriteSortLayer();
			UpdateMeshRendererSortLayer();
		}

		public void UpdateSpriteSortLayer ()
		{
			if (_spriteRenderer == null)
			{
				return;
			}

			int sortOrder = SortLayerUtil.YAxisConverSortOrderValue(transform.position.y);
			_spriteRenderer.sortingOrder = sortOrder;
		}

		public void UpdateMeshRendererSortLayer ()
		{
			if (_meshRenderer == null)
			{
				return;
			}

			int sortOrder = SortLayerUtil.YAxisConverSortOrderValue(transform.position.y);
			_meshRenderer.sortingOrder = sortOrder;
		}

		public void InitComponent ()
		{
			_meshRenderer = GetComponent<MeshRenderer>();
			if (_meshRenderer != null)
			{
				_meshRenderer.sortingLayerName = SortLayerName;
				return;
			}

			_spriteRenderer = GetComponent<SpriteRenderer>();
			if (_spriteRenderer != null)
			{
				_spriteRenderer.sortingLayerName = SortLayerName;
				return;
			}
		}

		public void Dispose ()
		{
			
		}

		// ����ʱ��ĳ�ʼ��
		public void Init ()
		{
			
		}
	}

}

