using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour {
	#region Variables
	public GameObject	cellPrefab;

	[Header("World Params")]
	public int			sizeX;
	public int			sizeY;
	public float		yOffsetJitter = 0.1f;

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
		Cell cell;

		for (int y = 0; y < sizeY; ++y) {
			cells.Add(new List<Cell>());

			for (int x = 0; x < sizeX; ++x) {
				go = Instantiate(cellPrefab);
				cell = go.GetComponent<Cell>();

				go.transform.parent = transform;
				go.transform.localPosition = new Vector3(x - sizeX / 2 + 1, Random.Range(-yOffsetJitter, yOffsetJitter), -(y - sizeY / 2 + 1));

				cell.Setup(x, y);
				cells[y].Add(cell);
			}
		}
	}
	#endregion
}
