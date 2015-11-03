using UnityEngine;
using System.Collections;

public class FireStarter : MonoBehaviour 
{
	[SerializeField]
	private Camera _mainCamera = null;

	[SerializeField]
	private GameObject _thunder = null;

	private bool _wasThunderShot = false;

	void Awake()
	{
		if(this._mainCamera == null)
		{
			this._mainCamera = this.GetComponent<Camera>();
		}
	}

	void Start () 
	{
	
	}
	
	void Update () 
	{
		ProcesThunder();
	}

	private void ProcesThunder()
	{
		if(Input.GetMouseButton(0))
		{
			if (!this._wasThunderShot)
			{
				Ray mouseRay = this._mainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit rayHit = new RaycastHit();
				if (Physics.Raycast(mouseRay, out rayHit))
				{

					Vector3 rot = Vector3.zero;
					rot.x = 270.0f;
					Quaternion quat = Quaternion.identity;
					quat.eulerAngles = rot;

					GameObject go = (GameObject)GameObject.Instantiate(this._thunder, rayHit.point, quat);
					GameObject.Destroy(go, 1.5f);

					Cell tmpCell = rayHit.collider.GetComponent<Cell>();
					if(tmpCell != null)
					{
						tmpCell.Ignite();
					}
				}
			}
			this._wasThunderShot = true;
		}else{
			this._wasThunderShot = false;
		}
	}
}
