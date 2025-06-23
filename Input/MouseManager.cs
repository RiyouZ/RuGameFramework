using System;
using UnityEngine;

namespace RuGameFramework.Input
{
	public class MouseManager : MonoBehaviour , IDisposable
	{
		private Camera _mainCamera = null;

		private Vector3 _mousePosition = Vector2.zero;

		private MouseData _mouseData = new MouseData();

		private GameObject _cursorObj;
		public GameObject Cursor
		{
			set
			{
				_cursorObj = value;
			}
		}

		public Vector3 MousePosition
		{
			get
			{
				return _mousePosition;
			}
		}

		public Vector3 WorldPosition
		{
			get
			{
				return _mouseData.worldPosistion;
			}
		}

		// Start is called before the first frame update
		void Start ()
		{
			Init();
		}

		private void Init ()
		{
			_mainCamera = Camera.main;
			_mousePosition = UnityEngine.Input.mousePosition;
		}

		private void UpdateMouseInfo ()
		{
			_mousePosition = UnityEngine.Input.mousePosition;
			if (_mainCamera == null)
			{
				_mainCamera = Camera.main;
			}
			_mouseData.worldPosistion.z = -_mainCamera.transform.position.z;
			_mouseData.worldPosistion = _mainCamera.ScreenToWorldPoint(_mousePosition);

			// �������ָ������
			if (_cursorObj != null)
			{
				_cursorObj.transform.position = _mouseData.worldPosistion;
			}

		}

		private void UpdateMouseButtonState ()
		{
			if (UnityEngine.Input.GetMouseButtonDown(0))
			{
				_mouseData.LeftState = MouseButtonState.Down;
			} 
			else if (UnityEngine.Input.GetMouseButtonUp(0))
			{
				_mouseData.LeftState = MouseButtonState.Up;
			}

			if (UnityEngine.Input.GetMouseButtonDown(1))
			{
				_mouseData.RightState = MouseButtonState.Down;
			} 
			else if (UnityEngine.Input.GetMouseButtonUp(1))
			{
				_mouseData.RightState = MouseButtonState.Up;
			}
		}

		void Update ()
		{
			UpdateMouseInfo();
			UpdateMouseButtonState();
		}

		public void Dispose ()
		{
			_mouseData = null;
			_mainCamera = null;
		}
	}

}
