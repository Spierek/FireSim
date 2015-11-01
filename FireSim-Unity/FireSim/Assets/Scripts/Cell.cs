using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {
	#region Variables
	public int			x;
	public int			y;
	public CellMaterial cellMaterial;
	#endregion

	#region Monobehaviour
	void Start () {
	
	}
	
	void Update () {
	
	}
	#endregion

	#region Methods
	public void Setup(int x, int y) {
		this.x = x;
		this.y = y;

		gameObject.name = "Cell(" + x + "," + y + ")";

		//TODO: randomize material, setup visuals
	}

	public void SetMaterial(GameObject prefab) {
		GameObject go = Instantiate(prefab);
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero;

		cellMaterial = go.GetComponent<CellMaterial>();
	}
	#endregion
}
