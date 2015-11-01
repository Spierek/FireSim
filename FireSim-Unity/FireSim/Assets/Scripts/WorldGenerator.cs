using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour {
	#region Variables
	[Header("Prefabs")]
	public GameObject			cellPrefab;
	public List<GameObject>		materialPrefabs = new List<GameObject>();

	[Header("World Params")]
	public int			sizeX;
	public int			sizeY;
	public float		yOffsetJitter = 0.1f;
	[Range(1, 10)]
	public float		cellSize = 1;
	[Range(-40, 40)]
	public float		globalTemperature = 20;
	[Range(0, 30)]
	public float		airHumidity = 10;
	[Range(0, 30)]
	public float		windSpeed = 10;

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

				cell.Setup(x, y, cellSize);
				cell.SetMaterial(materialPrefabs[Random.Range(0, materialPrefabs.Count)]);		// TODO: would be nice to have a percentage-based material type setup? (some stuff is placed rarely)
				cells[y].Add(cell);
			}
		}
	}
	#endregion
}
