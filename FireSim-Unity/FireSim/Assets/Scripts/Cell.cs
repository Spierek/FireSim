using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour 
{
	#region Variables
	public int			x;
	public int			y;

	[Header("Cell Properties")]
	public CellMaterial materialType;
	[Range(0, 2000)]
	public float		currentTemperature;
	public float		materialMass;
	public float		waterMass;
	public float		cellSize;

	private bool		materialSet = false;
	// preset values
	private Vector2			materialMassRange = new Vector2(100, 100);
	private ParticleSystem	fireParticles;

	private Color		temperatureColorA = new Color(0, 0, 1, 0.3f);
	private Color		temperatureColorB = new Color(1, 0.4f, 0.4f, 0.8f);

	//fire values;
	private bool _isBurning = false;
	public bool IsBurning { get { return this._isBurning; } }
	private float _aquiredEnergy = 0.0f;
	private float _storedEnergy = 0.0f;

	#endregion

	#region Monobehaviour
	void Awake () 
	{
		fireParticles = transform.Find("FireParticles").GetComponent<ParticleSystem>();
	}

	void Start()
	{
		this.currentTemperature = WorldGenerator.Instance.globalTemperature;
		this.waterMass = this.materialMass * this.materialType.moisture;
	}

	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.A)) 
		{
			Ignite();
		}

		this.ProcessCalculationSelf();
	  
	}
	void LateUpdate()
	{
		this.ProcessCalculationOthers();
	}

	void OnDrawGizmos() 
	{
		if (WorldGenerator.Instance.drawTemperatureGizmos && materialSet) 
		{
			Gizmos.color = Color.Lerp(temperatureColorA, temperatureColorB, Mathf.Clamp01(currentTemperature / materialType.ignitionTemperature));
			Gizmos.DrawCube(transform.position + transform.up * 0.4f, Vector3.one * 0.9f);
		}
	}
	#endregion

	#region Methods
	public void Setup(int x, int y, float size) {
		this.x = x;
		this.y = y;
		cellSize = size;

		gameObject.name = "Cell(" + x + "," + y + ")";
	}

	public void SetMaterial(GameObject prefab) {
		// spawn material prefab (visualization + values)
		GameObject go = Instantiate(prefab);
		go.transform.parent = transform;
		go.transform.localPosition = new Vector3(0, 0.1f, 0);

		materialType = go.GetComponent<CellMaterial>();
		materialSet = true;
	}

	// uses a seeded random instance for deterministic generation
	public void SetValues(System.Random rand) 
	{
		materialMass = rand.Next((int)materialMassRange.x, (int)materialMassRange.y * 100) / 100f;
	}

	public void Ignite() 
	{
		if (this.materialMass > 0.0f)
		{
			this.waterMass = 0.0f;
			this._isBurning = true;
			currentTemperature = materialType.ignitionTemperature;
			this.fireParticles.Play();
		}
	}
	public void PutOffFire()
	{
		this._isBurning = false;
		this.fireParticles.Stop();
	}

	private void ProcessCalculationSelf()
	{
		if(this.IsBurning)
		{
			float generatedEnergy = 0.0f;

			//how much of mass was burned
			float burnedMaterialMass = this.materialType.burnSpeed * Time.deltaTime;

			//Debug.Log("Mass [0]: " + this.materialMass + ", burned mass: " + burnedMaterialMass);
			//substract burned mass from cell mass
			if(this.materialMass > burnedMaterialMass)
			{
				this.materialMass -= burnedMaterialMass;
			}else{
				burnedMaterialMass = this.materialMass;
				this.materialMass = 0.0f;
			}
			//Debug.Log("Mass [1]: " + this.materialMass);

			//how much energy was generated in burning process
			generatedEnergy += burnedMaterialMass * this.materialType.fuelEnergy * 100000.0f * 100.0f;
			generatedEnergy += this._storedEnergy;
			this._storedEnergy = 0.0f;

			List<List<Cell>> tmpCells = WorldGenerator.Instance.Cells;
			int sizeX = WorldGenerator.Instance.sizeX;
			int sizeY = WorldGenerator.Instance.sizeY;

			for (int i = -1; i < 2;++i )
			{
				for(int j = -1;j < 2;++j)
				{
					int positionX = this.x + i;
					int positionY = this.y + j;
					if (positionX >= 0 && positionX < sizeX)
					{
						if(positionY >= 0 && positionY < sizeY)
						{
							if( positionX != this.x || positionY != this.y)
							{
								tmpCells[positionX][positionY].AquireEnergy(generatedEnergy / 8.0f);
							}
						}
					}
				}
			}

			if (this.materialMass <= 0.0f)
			{
				PutOffFire();
			}
		}
	}
	private void ProcessCalculationOthers()
	{
		float massSpecificHeatCoefitient = (this.materialMass * this.materialType.specificHeat + this.waterMass * CellMaterial.specificHeat_water);

		if(this.currentTemperature < CellMaterial.vaporizationTemperature)
		{
			//possible grow of temperature below vaporization temperature
			float deltaTemperature = this._aquiredEnergy / massSpecificHeatCoefitient;
			
			if (this.currentTemperature + deltaTemperature <= CellMaterial.vaporizationTemperature)
			{
				this.currentTemperature += deltaTemperature;
				this._aquiredEnergy = 0.0f;
			}else{

				//get to vaporization temperature
				float missingTemperature = CellMaterial.vaporizationTemperature - this.currentTemperature;

				float energyRequiredToGetVaporizationTemperature = missingTemperature * massSpecificHeatCoefitient;

				this.currentTemperature = CellMaterial.vaporizationTemperature;

				this._aquiredEnergy -= energyRequiredToGetVaporizationTemperature;

			}
		}

		if(this.currentTemperature == CellMaterial.vaporizationTemperature && this.waterMass > 0.0f)
		{
			float deltaWaterMassVaporization = this._aquiredEnergy * CellMaterial.specificHeat_water;
			if(this.waterMass >= deltaWaterMassVaporization)
			{
				//less energy to fully vaporize
				this.waterMass -= deltaWaterMassVaporization;
				this._aquiredEnergy = 0.0f;
			}else{

				//more energy to fullt vaporize
				float energyToVaporize = this.waterMass * CellMaterial.specificHeat_water;
				this.waterMass = 0.0f;
				this._aquiredEnergy -= energyToVaporize;
				
			}
		}

		if(this.currentTemperature >= CellMaterial.vaporizationTemperature && this.waterMass == 0.0f && this._aquiredEnergy > 0.0f)
		{
			float deltaTemperature = this._aquiredEnergy / massSpecificHeatCoefitient;
			if(deltaTemperature + this.currentTemperature < this.materialType.ignitionTemperature)
			{
				//less energy than needed to ignite

				this.currentTemperature += deltaTemperature;
				this._aquiredEnergy = 0.0f;
			}else{

				//more energy than needed to ignite
				float temperatureToIgnite = this.materialType.ignitionTemperature - this.currentTemperature;
				float energyToIgnite = temperatureToIgnite * massSpecificHeatCoefitient;
				
				this.currentTemperature = this.materialType.ignitionTemperature;
				this._aquiredEnergy -= energyToIgnite;

				Ignite();				
			}
		}

		this._storedEnergy += this._aquiredEnergy;
		this._aquiredEnergy = 0.0f;
	}

	public void AquireEnergy(float energy)
	{
		if (!this.IsBurning)
		{
			this._aquiredEnergy += energy;
		}
	}

	#endregion
}
