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

		this._storedEnergy = 0.0f;
		this._aquiredEnergy = 0.0f;
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


		if (this._aquiredEnergy > 0.0f)
		{
			this._storedEnergy += this._aquiredEnergy;
		}
		this._aquiredEnergy = 0.0f;
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
		if (this.materialMass > 0.0f && !materialType.isNonFlammable && !this._isBurning)
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
		materialType.SwitchMesh();
	}

	private void ProcessCalculationSelf()
	{
		float generatedEnergy = 0.0f;

		if (this._isBurning)
		{
			//how much of mass was burned
			float burnedMaterialMass = this.materialType.burnSpeed * Time.deltaTime;

			if(this.materialMass > burnedMaterialMass)
			{
				this.materialMass -= burnedMaterialMass;
			}else{
				burnedMaterialMass = this.materialMass;
				this.materialMass = 0.0f;
			}
			//energy is in GJ - giga jules
			//how much energy was generated in burning process                       K        M         G
			generatedEnergy += burnedMaterialMass * this.materialType.fuelEnergy * 1000.0f * 1000.0f * 10.0f;
		}
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
							//equation 3 is used here to determin coeficients <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

							Vector3 myCellToTargetBoom = tmpCells[positionY][positionX].transform.position - myPosition;
							Vector3 myCellToTargetDirection = myCellToTargetBoom.normalized;

							//distance in floating point number
							float fDistance = myCellToTargetBoom.magnitude;
							float distanceValue = (fDistance - WorldGenerator.energyTransferDistance_min) / (WorldGenerator.energyTransferDistance_max - WorldGenerator.energyTransferDistance_min);
							float distanceCoeficient = Mathf.Lerp(WorldGenerator.distanceCoeficient_max, WorldGenerator.distanceCoeficient_min, distanceValue);

							/* delta T */
							float deltaTemp = Mathf.Abs(myTemperature - tmpCells[positionY][positionX].currentTemperature);
							/* W */
							float windDirectionCoeficient = 1.0f + Vector3.Dot(windDirection, myCellToTargetDirection);

							/* EQ 3 */
							float tmpCoefficient = deltaTemp * (windInfluenceCoeficient * windDirectionCoeficient * windSpeed + myEmissivity * stefan_boltzman_coefficient * Mathf.Pow(deltaTemp,3));

							tmpCoefficient *= Mathf.Pow(distanceCoeficient,WorldGenerator.energyTransferPowerLevel);

							positionCoefitients[energyTransferRadius + indexY][energyTransferRadius + indexX] = tmpCoefficient;

							//Debug.LogFormat("({0},{1}) Coefficient:{2}", positionX, positionY, tmpCoefficient);

							//positionCoefficientSummary += tmpCoefficient;
							//equation 3 is used here to determin coeficients <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
						}
					}
				}
			}
		}

		//positionCoefficientSummary = 0.0f;
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
							//float energyToTransfer = generatedEnergy * (positionCoefitients[energyTransferRadius + indexY][energyTransferRadius + indexX] / positionCoefficientSummary);

							float energyToTransfer = positionCoefitients[energyTransferRadius + indexY][energyTransferRadius + indexX];
							
							if(energyToTransfer > 0.0f)
							{
								tmpCells[positionY][positionX].AquireEnergy(energyToTransfer);
								//Debug.LogFormat("({0},{1}) Coefficient:{2}", positionX, positionY, energyToTransfer);
							}
						}
					}
				}
			}
		}

		//energy radiated into the oblivion

		float temperatureDiffrence = Mathf.Abs(myTemperature - WorldGenerator._instance.globalTemperature);

		float oblivionEnergy = myEmissivity * stefan_boltzman_coefficient * Mathf.Pow(temperatureDiffrence, 3.0f) * 1.0f;

		if(generatedEnergy > oblivionEnergy)
		{
			generatedEnergy -= oblivionEnergy;
		}

		if(this.materialMass <= 0.0f)
		{
			generatedEnergy = 0.0f;
			this.currentTemperature = WorldGenerator._instance.globalTemperature;
			//Debug.LogFormat("({0} {1}) current: {2} global: {3}",x,y, this.currentTemperature, WorldGenerator._instance.globalTemperature);
		}

		//if (positionCoefficientSummary > 0)
		//{
		//	generatedEnergy -= positionCoefficientSummary;
		
		//}
		if (generatedEnergy > 0.0f)
		{
			this._aquiredEnergy += generatedEnergy;
		}

		//Debug.LogFormat("energy generated: {0}, coefficient summary: {1}", generatedEnergy, positionCoefficientSummary);

		if (this.materialMass <= 0.0f && IsBurning)
		{
			PutOffFire();
		}
	}
	private void ProcessCalculationOthers()
	{
		float massSpecificHeatCoefitient = (this.materialMass * this.materialType.specificHeat + this.waterMass * CellMaterial.specificHeat_water);
		float massOnlyHeatCoefficient = this.materialMass * this.materialType.specificHeat;

		if (this.currentTemperature < CellMaterial.vaporizationTemperature && this.materialMass > 0.0f)
		{
			//possible grow of temperature below vaporization temperature
			float deltaTemperature = this._aquiredEnergy / massSpecificHeatCoefitient;

			if (this.currentTemperature + deltaTemperature < CellMaterial.vaporizationTemperature)
			{
				this.currentTemperature += deltaTemperature;
				this._aquiredEnergy = 0.0f;
				return;
			}
			else
			{

				//get to vaporization temperature
				float missingTemperature = CellMaterial.vaporizationTemperature - this.currentTemperature;

				float energyRequiredToGetVaporizationTemperature = missingTemperature * massSpecificHeatCoefitient;

				this.currentTemperature = CellMaterial.vaporizationTemperature;

				this._aquiredEnergy -= energyRequiredToGetVaporizationTemperature;

			}
		}

		if (this.currentTemperature == CellMaterial.vaporizationTemperature && this.waterMass > 0.0f && this._aquiredEnergy > 0.0f)
		{
			//possible mass of vaporized water
			float deltaWaterMassVaporization = this._aquiredEnergy * CellMaterial.specificHeat_water;

			if (this.waterMass > deltaWaterMassVaporization)
			{
				//less energy to fully vaporize
				this.waterMass -= deltaWaterMassVaporization;
				this._aquiredEnergy = 0.0f;
				return;
			}
			else
			{
				//more energy to fully vaporize
				float energyNeededToVaporizeRemainingWater = this.waterMass * CellMaterial.specificHeat_water;
				this.waterMass = 0.0f;
				this._aquiredEnergy -= energyNeededToVaporizeRemainingWater;

			}
		}

		//if (this.materialMass <= 0.0f)
		//{
		//	Debug.LogFormat("{0} {1} temp: {2}", x, y, this.currentTemperature);
		//}

		//if (this._isBurning)
		//{
		//	Debug.LogFormat("Is burning, aquired: {0}",this._aquiredEnergy);
		//}

		if (this.currentTemperature >= CellMaterial.vaporizationTemperature && this.waterMass == 0.0f && this._aquiredEnergy > 0.0f)
		{
			//possible grow in temperature
			float deltaTemperature = this._aquiredEnergy / massOnlyHeatCoefficient;

			if (this.currentTemperature < this.materialType.ignitionTemperature)
			{

				if (deltaTemperature + this.currentTemperature < this.materialType.ignitionTemperature)
				{
					//less energy than needed to ignite

					this.currentTemperature += deltaTemperature;
					this._aquiredEnergy = 0.0f;
					return;
				}
				else
				{

					//more energy than needed to ignite
					float deltaTemperatureMissingToIgnite = this.materialType.ignitionTemperature - this.currentTemperature;
					float energyToIgnite = deltaTemperatureMissingToIgnite * massOnlyHeatCoefficient;

					this.currentTemperature = this.materialType.ignitionTemperature;
					this._aquiredEnergy -= energyToIgnite;

				}
			}else{

				if(deltaTemperature + this.currentTemperature < CellMaterial.maxCellTemperature)
				{
					this.currentTemperature += deltaTemperature;
					this._aquiredEnergy = 0.0f;
				}else{

					float deltaTemperatureMissingToMax = CellMaterial.maxCellTemperature - this.currentTemperature;
					float energyToMaxTemperature = deltaTemperatureMissingToMax * massOnlyHeatCoefficient;

					this.currentTemperature = CellMaterial.maxCellTemperature;
					this._aquiredEnergy -= energyToMaxTemperature;
				}
				
			}
		}

		if(this.currentTemperature >= this.materialType.ignitionTemperature)
		{
			Ignite();
		}
	}

	public void AquireEnergy(float energy)
	{
		this._aquiredEnergy += energy;
	}

	public void PrintStatus()
	{
		Debug.LogFormat("Pos: {0} {1} Temp: {2} Mass: {3} WaterMass: {4}",this.x,this.y,currentTemperature,this.materialMass,this.waterMass);
	}

	#endregion
}
