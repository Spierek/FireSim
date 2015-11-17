using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class FireStarter : MonoBehaviour 
{
	enum ClickType
	{
		CT_IGNITE = 0,
		CT_INFO = 1,

		CT_COUNT
	}

	[SerializeField]
	private Camera _mainCamera = null;

	[SerializeField]
	private GameObject _thunder = null;

	void Awake() {
		if (this._mainCamera == null) {
			this._mainCamera = this.GetComponent<Camera>();
		}
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) 
		{
			if(!EventSystem.current.IsPointerOverGameObject()) 
			{
				if(Input.GetKey(KeyCode.LeftAlt))
				{
					ProcesThunder(ClickType.CT_INFO);
				}else{
					ProcesThunder(ClickType.CT_IGNITE);
				}
			}
		}
	}

	private void ProcesThunder(ClickType clickType = ClickType.CT_IGNITE)
	{
		Ray mouseRay = this._mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit = new RaycastHit();
		if (Physics.Raycast(mouseRay, out rayHit))
		{

			Vector3 rot = Vector3.zero;
			rot.x = 270.0f;
			Quaternion quat = Quaternion.identity;
			quat.eulerAngles = rot;

			

			Cell tmpCell = rayHit.collider.GetComponent<Cell>();
			if(tmpCell != null)
			{
				switch(clickType)
				{
					case ClickType.CT_IGNITE:
						GameObject go = (GameObject)Instantiate(this._thunder, rayHit.point, quat);
						Destroy(go, 1.5f);
						tmpCell.Ignite();
						break;
					case ClickType.CT_INFO:
						tmpCell.PrintStatus();
						break;
				}
			}
		}
	}
}
