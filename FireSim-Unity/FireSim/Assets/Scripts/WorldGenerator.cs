﻿using UnityEngine;
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

	#endregion

	#region Monobehaviour
	void Awake () 
	{
		WorldGenerator._instance = this;
		Generate();
	}
	
	void Update () 
	{

	}

	#endregion

	#region Methods

	private void Generate() 
	{
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
				cell.SetMaterial(materialPrefabs[rand.Next(0, materialPrefabs.Count)]);		// TODO: would be nice to have a percentage-based material type setup? (some stuff is placed rarely)
				cell.SetValues(rand);

				_cells[y].Add(cell);
			}
		}
	}

	public void SetTemperatureDebugGizmos(bool enabled) 
	{
		drawTemperatureGizmos = enabled;
	}

	public void UpdateWindDirection(Vector3 newWindDirectionVector)
	{
		this._windDirectionVector = newWindDirectionVector;
	}

	#endregion
}
