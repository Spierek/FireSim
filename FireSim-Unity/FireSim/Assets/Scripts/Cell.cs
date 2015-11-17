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
	private Color		temperatureColorC = new Color(0f, 0f, 0f, 0.5f);
	private Color		temperatureColorD = new Color(0f, 0f, 0f, 0.1f);

	//fire values;
	private bool		_isBurning = false;
	public bool			IsBurning { get { return this._isBurning; } }
	private float		_aquiredEnergy = 0.0f;
	private float		_storedEnergy = 0.0f;
	private float		initialMass;

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
		if (WorldGenerator.Instance.drawTemperatureGizmos && materialSet && !materialType.isNonFlammable) 
		{
			if (!IsBurning)
				Gizmos.color = Color.Lerp(temperatureColorA, temperatureColorB, Mathf.Clamp01(currentTemperature / materialType.ignitionTemperature));
			else if (materialMass > 0)
				Gizmos.color = Color.Lerp(temperatureColorB, temperatureColorC, 1 - materialMass / initialMass);
			
			if (materialMass == 0)
				Gizmos.color = temperatureColorD;


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
		initialMass = materialMass;
	}

	public void SetTemperature(float temp) {
		currentTemperature = temp;
	}

	public void Ignite() 
	{
		if (this.materialMass > 0.0f && !materialType.isNonFlammable)
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

			//some local variables for fast computing
			int energyTransferRadius = WorldGenerator.energyTransferRadius;

			int myX = this.x;
			int myY = this.y;
			float myTemperature = this.currentTemperature;
			Vector3 myPosition = this.transform.position;

			float windSpeed = WorldGenerator.Instance.windSpeed;

			/* ε (Epsilon) */
			float myEmissivity = this.materialType.emissivity;
			Vector3 windDirection = WorldGenerator.Instance.WindDirectionVector;
			/* σ (Sigma) */
			float stefan_boltzman_coefficient = WorldGenerator.stefan_boltzman_coefficient;

			/* h */
			float windInfluenceCoeficient = WorldGenerator.windInfluenceCoefficient;

			List<List<Cell>> tmpCells = WorldGenerator.Instance.Cells;
			int sizeX = WorldGenerator.Instance.sizeX;
			int sizeY = WorldGenerator.Instance.sizeY;

			int coefficientTableSize =energyTransferRadius * 2 + 1;
			float[][] positionCoefitients = new float[coefficientTableSize][];
			for (int i = 0; i < coefficientTableSize; ++i)
			{
				positionCoefitients[i] = new float[coefficientTableSize];
			}

			//for(int i = 0;i < coefficientTableSize;++i)
			//{
			//	for(int j = 0;j < coefficientTableSize;++j)
			//	{
			//		Debug.LogFormat("{0} {1} is {2}",i,j, positionCoefitients[i][j]);
			//	}
			//}

			float positionCoefficientSummary = 0.0f;

			//calculate coeficients used to devide generated energy that needs to be radiated
			int distance = 0;
			for (int indexY = -energyTransferRadius; indexY <= energyTransferRadius; ++indexY)
			{
				for (int indexX = -energyTransferRadius; indexX <= energyTransferRadius; ++indexX)
				{
					distance = Mathf.Abs(indexX) + Mathf.Abs(indexY);
					int positionX = myX + indexX;
					int positionY = myY + indexY;

					if(distance <= energyTransferRadius)
					{
						if (positionX >= 0 && positionX < sizeX)
						{
							if (positionY >= 0 && positionY < sizeY)
							{
								if (positionX != this.x || positionY != this.y)
								{
									//equation 3 is used here to determin coeficients <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

									Vector3 myCellToTargetBoom = tmpCells[positionY][positionX].transform.position - myPosition;
									Vector3 myCellToTargetDirection = myCellToTargetBoom.normalized;

									/* delta T */
									float deltaTemp = Mathf.Abs(myTemperature - tmpCells[positionY][positionX].currentTemperature);
									/* W */
									float windDirectionCoeficient = 1.0f + Vector3.Dot(windDirection, myCellToTargetDirection);

									/* EQ 3 */
									float tmpCoefficient = deltaTemp * (windInfluenceCoeficient * windDirectionCoeficient * windSpeed + myEmissivity * stefan_boltzman_coefficient * Mathf.Pow(deltaTemp,3));
									//Debug.LogFormat("{0} {1}", windInfluenceCoeficient * windDirectionCoeficient * windSpeed, myEmissivity * stefan_boltzman_coefficient * Mathf.Pow(deltaTemp, 3));
									//Debug.LogFormat("myPositionX: {0} myPositionY: {1} indexX: {2} indexY: {3} positionX: {4} positionY: {5}",myX,myY,indexX,indexY,positionX,positionY);
									//Debug.LogFormat("arraySize: {0} secondSize: {1}", positionCoefitients.Length, positionCoefitients[0].Length);
									//Debug.LogFormat("check {0} {1}", energyTransferRadius + indexX, energyTransferRadius + indexY);

									positionCoefitients[energyTransferRadius + indexY][energyTransferRadius + indexX] = tmpCoefficient;
									//positionCoefficientSummary += tmpCoefficient;
									//equation 3 is used here to determin coeficients <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
								}
							}
						}
					}
				}
			}

			//simulate that map has no borders
			for (int indexY = -energyTransferRadius; indexY < energyTransferRadius; ++indexY)
			{
				for (int indexX = -energyTransferRadius; indexX < energyTransferRadius; ++indexX)
				{
					int x = indexX + energyTransferRadius;
					int y = indexY + energyTransferRadius;

					int positionX = myX + indexX;
					int positionY = myY + indexY;

					int sourceX = energyTransferRadius + (positionX >= 0 && positionX < sizeX ? indexX : -indexX);
					int sourceY = energyTransferRadius + (positionY >= 0 && positionY < sizeY ? indexY : -indexY);

					positionCoefficientSummary += positionCoefitients[sourceY][sourceX];
				}
			}

			//distribute energy basing on coeficients
			for (int indexY = -energyTransferRadius; indexY <= energyTransferRadius; ++indexY)
			{
				for (int indexX = -energyTransferRadius; indexX <= energyTransferRadius; ++indexX)
				{
					distance = Mathf.Abs(indexX) + Mathf.Abs(indexY);
					int positionX = myX + indexX;
					int positionY = myY + indexY;

					if (distance <= energyTransferRadius)
					{
						if (positionX >= 0 && positionX < sizeX)
						{
							if (positionY >= 0 && positionY < sizeY)
							{
								if (positionX != this.x || positionY != this.y)
								{
									float energyToTransfer = generatedEnergy * (positionCoefitients[energyTransferRadius + indexY][energyTransferRadius + indexX] / positionCoefficientSummary);
									tmpCells[positionY][positionX].AquireEnergy(energyToTransfer);
								}
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
		if (!this.IsBurning && !materialType.isNonFlammable)
		{
			this._aquiredEnergy += energy;
		}
	}

	#endregion
}
