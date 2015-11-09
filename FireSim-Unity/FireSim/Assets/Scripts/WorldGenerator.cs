using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour {
	#region Variables
	public static WorldGenerator Instance;

	//const to be used with edge conditions
	
	public const float globalTemperature_min = -40.0f;
	public const float globalTemperature_max = 40.0f;

	public const float windDirection_min = 0.0f;
	public const float windDirection_max = 360.0f;

	public const float windSpeed_min = 0.0f;
	public const float windSpeed_max = 30.0f;

	[Header("Prefabs")]
	public GameObject			cellPrefab;
	public List<GameObject>		materialPrefabs = new List<GameObject>();

	[Header("World Params")]
	public int			currentSeed = 12345;
	public int			sizeX;
	public int			sizeY;
	public float		yOffsetJitter = 0.1f;
	[Range(1, 10)]
	public float		cellSize = 1;
	[Range(globalTemperature_min, globalTemperature_max)]
	public float		globalTemperature = 20.0f;
	[Range(windDirection_min, windDirection_max)]
	public float		windDirection = 0.0f;
	[Range(windSpeed_min, windSpeed_max)]
	public float		windSpeed = 10.0f;

	[Header("Debug")]
	public bool			drawTemperatureGizmos = false;

	public List<List<Cell>> cells = new List<List<Cell>>();
	#endregion

	#region Monobehaviour
	void Awake () {
		Instance = this;

		Generate();
	}
	
	void Update () {

	}
	#endregion

	#region Methods
	private void Generate() {
		GameObject go;
		Cell cell;
		System.Random rand;
		WangDoubleHash hashObject = new WangDoubleHash(currentSeed);
		float height;

		for (int y = 0; y < sizeY; ++y) {
			cells.Add(new List<Cell>());

			for (int x = 0; x < sizeX; ++x) {
				// get seed for current cell
				int cellSeed = (int)hashObject.GetHash(x, y);
				rand = new System.Random(cellSeed);

				// spawn world cell
				go = Instantiate(cellPrefab);
				cell = go.GetComponent<Cell>();

				go.transform.parent = transform;
				height = (rand.Next(0, (int)(yOffsetJitter * 100)) + 50) / 100f;
				go.transform.localPosition = new Vector3(x - sizeX / 2 + 1, height, -(y - sizeY / 2 + 1));

				// setup cell
				cell.Setup(x, y, cellSize);
				cell.SetMaterial(materialPrefabs[rand.Next(0, materialPrefabs.Count)]);		// TODO: would be nice to have a percentage-based material type setup? (some stuff is placed rarely)
				cell.SetValues(rand);

				cells[y].Add(cell);
			}
		}
	}

	public void SetTemperatureDebugGizmos(bool enabled) {
		drawTemperatureGizmos = enabled;
	}
	#endregion
}
