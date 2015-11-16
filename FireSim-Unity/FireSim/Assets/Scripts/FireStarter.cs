using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class FireStarter : MonoBehaviour {
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
		if (Input.GetMouseButtonDown(0)) {
			if(!EventSystem.current.IsPointerOverGameObject()) {
				ProcesThunder();
			}
		}
	}

	private void ProcesThunder()
	{
		Ray mouseRay = this._mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit = new RaycastHit();
		if (Physics.Raycast(mouseRay, out rayHit))
		{

			Vector3 rot = Vector3.zero;
			rot.x = 270.0f;
			Quaternion quat = Quaternion.identity;
			quat.eulerAngles = rot;

			GameObject go = (GameObject)Instantiate(this._thunder, rayHit.point, quat);
			Destroy(go, 1.5f);

			Cell tmpCell = rayHit.collider.GetComponent<Cell>();
			if(tmpCell != null)
			{
				tmpCell.Ignite();
			}
		}
	}
}
