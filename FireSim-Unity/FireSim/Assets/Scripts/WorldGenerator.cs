using UnityEngine;
using System.Collections.Generic;

public class WorldGenerator : MonoBehaviour 
{
	#region Variables
	public static WorldGenerator _instance = null;
	public static WorldGenerator Instance { get { return WorldGenerator._instance; } }

	//const to be used with edge conditions
	
	public const float globalTemperature_min = -40.0f;
	public const float globalTemperature_max = 40.0f;

	public const float windDirection_min = 0.0f;
	public const float windDirection_max = 360.0f;

	public const float windSpeed_min = 0.0f;
	public const float windSpeed_max = 30.0f;

	public const float windInfluenceCoefficient = 1.0f;

	public const float stefan_boltzman_coefficient = 5.67f * 0.00000001f;

	public const int energyTransferRadius = 2;

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

	private Vector3 _windDirectionVector = Vector3.zero;
	public Vector3 WindDirectionVector { get { return this._windDirectionVector; } }

	[Header("Debug")]
	public bool			drawTemperatureGizmos = false;

	private List<List<Cell>> _cells = new List<List<Cell>>();
	public List<List<Cell>> Cells { get { return this._cells; } }

	private bool _isWorldGenerated = false;
	public bool[] selectableSpawn = null;

	#endregion

	#region Monobehaviour
	void Awake () 
	{
		WorldGenerator._instance = this;
		if( (this.selectableSpawn != null && this.selectableSpawn.Length != this.materialPrefabs.Count) || this.selectableSpawn == null )
		{
			int materialCount = this.materialPrefabs.Count;
			this.selectableSpawn = new bool[materialCount];
			for(int i = 0;i < materialCount;++i)
			{
				this.selectableSpawn[i] = true;
			}
		}
	}
	#endregion

	#region Methods
	public void GenerateNewSeed() {
		currentSeed = Random.Range(0, 123456);
	}

	public void Generate() 
	{
		if(this._isWorldGenerated)
		{
			this.ClearWorld();
		}

		int materialCount = this.materialPrefabs.Count;
		List<GameObject> selectedMaterials = new List<GameObject>();
		for(int i = 0;i < materialCount;++i)
		{
			if(this.selectableSpawn[i])
			{
				selectedMaterials.Add(this.materialPrefabs[i]);
			}
		}
		int selectedCount = selectedMaterials.Count;

		GameObject go;
		Cell cell;
		System.Random rand;
		WangDoubleHash hashObject = new WangDoubleHash(currentSeed);
		float height;

		for (int y = 0; y < sizeY; ++y) 
		{
			_cells.Add(new List<Cell>());

			for (int x = 0; x < sizeX; ++x) 
			{
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
				cell.SetMaterial(selectedMaterials[rand.Next(0, selectedCount)]);		// TODO: would be nice to have a percentage-based material type setup? (some stuff is placed rarely)
				cell.SetValues(rand);
				cell.SetTemperature(globalTemperature);

				_cells[y].Add(cell);
			}
		}
		this._isWorldGenerated = true;
	}

	public void ClearWorld()
	{
		for(int y = 0;y < this.sizeY;++y)
		{
			for(int x = 0;x < this.sizeX;++x)
			{
				if(this._cells[y][x] != null)
				{
					GameObject.Destroy(this._cells[y][x].gameObject);
				}
			}
			this._cells[y].Clear();
		}
		this._isWorldGenerated = false;
	}

	public void SetTemperatureDebugGizmos(bool enabled) 
	{
		drawTemperatureGizmos = enabled;
	}

	public void UpdateWindDirection(Vector3 newWindDirectionVector)
	{
		this._windDirectionVector = newWindDirectionVector;
	}

	//void OnDrawGizmos()
	//{
	//	Gizmos.DrawSphere(this.transform.position + Vector3.down, 0.5f);
	//	Gizmos.DrawLine(this.transform.position + Vector3.down, this.transform.position + Vector3.down + this.WindDirectionVector * 5.0f);
	//}

	#endregion
}
