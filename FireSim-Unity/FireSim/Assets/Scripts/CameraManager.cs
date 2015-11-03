using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	#region Variables

	private Transform _transform = null;

	private Vector3 _initialPosition = Vector3.zero;
	private Quaternion _initialRotation = Quaternion.identity;

	private float _rotationBoomLength = 5.0f;

	private Vector3 _lastMousePosition = Vector3.zero;

	private float _cameraTranslationSpeed = 0.02f;
	private float _cameraScrollSpeed = 0.8f;

	private bool _wasResetKeyPressed = false;

	#endregion Variables

	#region Monobahaviour Methods
	
	void Awake()
	{
		this._transform = this.GetComponent<Transform>();
	}

	void Start () 
	{
		this._initialPosition = this._transform.localPosition;
		this._initialRotation = this._transform.localRotation;

		this._lastMousePosition = Input.mousePosition;
	}
	
	void Update () 
	{
		ProcesTranslation();
		//ProcesRotation();
		ProcesReset();

		this._lastMousePosition = Input.mousePosition;
	}

	#endregion Monobehaviour Methods

	#region Methods

	private void ProcesTranslation()
	{
		Vector3 cameraPosition = this._transform.localPosition;

		Vector3 forward = this._transform.forward;
		Vector3 right = this._transform.right;
		Vector3 up = this._transform.up;

		Vector3 currentMousePosition = Input.mousePosition;
		Vector3 deltaMousePosition = currentMousePosition - this._lastMousePosition;

		if(Input.GetMouseButton(1))
		{
			cameraPosition -= right * deltaMousePosition.x * this._cameraTranslationSpeed;
			cameraPosition -= up * deltaMousePosition.y * this._cameraTranslationSpeed;
		}
		cameraPosition += forward * Input.mouseScrollDelta.y * this._cameraScrollSpeed;

		this._transform.position = cameraPosition;
	}

	private void ProcesRotation()
	{
		Vector3 initialForward = this._transform.forward;
		Quaternion quat = Quaternion.identity;
		
	}

	private void ProcesReset()
	{
		if(Input.GetKey(KeyCode.R))
		{
			if(!this._wasResetKeyPressed)
			{
				this._transform.localPosition = this._initialPosition;
				this._transform.localRotation = this._initialRotation;
			}
			this._wasResetKeyPressed = true;
		}else{
			this._wasResetKeyPressed = false;
		}
	}

	#endregion Methods
}
