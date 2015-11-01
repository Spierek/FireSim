using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour {
	#region Variables
	public GameObject	cellPrefab;

	[Header("World Params")]
	public int			sizeX;
	public int			sizeY;

	//[HideInInspector]
	public List<List<Cell>> cells = new List<List<Cell>>();
	#endregion

	#region Monobehaviour
	void Awake () {
		Generate();
	}
	
	void Update () {
	
	}
	#endregion

	#region Methods
	private void Generate() {
		GameObject go;
		for (int y = 0; y < sizeY; ++y) {
			cells.Add(new List<Cell>());

			for (int x = 0; x < sizeX; ++x) {
				go = Instantiate(cellPrefab);

				go.transform.parent = transform;
				go.transform.localPosition = new Vector3(x - sizeX / 2 + 1, 0, -(y - sizeY / 2 + 1));
				go.name = "Cell(" + x + "," + y + ")";

				cells[y].Add(go.GetComponent<Cell>());
			}
		}
	}
	#endregion
}
