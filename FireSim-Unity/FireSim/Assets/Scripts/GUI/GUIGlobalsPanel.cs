using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIGlobalsPanel : MonoBehaviour
{
	#region Variables

	[SerializeField]
	private WorldGenerator _currentWorldGenerator = null;

	private float _currentWorldTemperatureSliderValue = 0.0f;
	private float _currentWindDirectionSliderValue = 0.0f;
	private float _currentWindSpeedSliderValue = 0.0f;

	private const string _arrowMaterialColor = "_Color";

	[Header("GUI objects")]
	
	[SerializeField]
	private Slider _worldTemperatureSlider = null;
	[SerializeField]
	private Text _worldTemperatureLabel = null;

	[SerializeField]
	private Slider _worldWindDirectionSlider = null;
	[SerializeField]
	private Text _worldWindDirectionLabel = null;

	[SerializeField]
	private Slider _worldWindSpeedSlider = null;
	[SerializeField]
	private Text _worldWindSpeedLabel = null;

	[SerializeField]
	private Text _currentSeedLabel = null;

	[SerializeField]
	private GameObject _windDirectionArrowGO = null;
	[SerializeField]
	private MeshRenderer _windDirectionArrowMeshRenderer = null;
	[SerializeField]
	private WindZone _windZoneObject = null;


	[SerializeField]
	private Slider _worldSizeSlider = null;
	[SerializeField]
	private Text _worldSizeLabel = null;

	[SerializeField]
	private Slider _cellMassSlider = null;
	[SerializeField]
	private Text _cellMassLabel = null;

	private float _windDirectionArrowMaxAlpha = 0.4f;
	private float _windDirectionArrowTimeToFadeOut = 1.0f;
	private float _windDirectionArrowTimer = 0.0f;

	private float _lastRealTime = 0.0f;
	private float _realDeltaTime = 0.0f;

	private int _currentWorldSize = 0;
	private int _lastWorldSize = 0;

	[Header("Selectable spawn")]
	private float toggleLocalInitialY = -20.0f;
	private float toggleLocalOffsetY = -30.0f;
	[SerializeField]
	private GameObject exampleToogle = null;
	[SerializeField]
	private GameObject toggleParent = null;

	private Toggle _selectableToggles = null;

	#endregion Variables

	#region Monobehaviour Methods

	void Start () 
	{
		this._lastRealTime = Time.realtimeSinceStartup;
		this._windDirectionArrowTimer = this._windDirectionArrowTimeToFadeOut;
		_windZoneObject.windMain = 0f;

		if(this._currentWorldGenerator == null)
		{
			this._currentWorldGenerator = GameObject.FindObjectOfType<WorldGenerator>();
		}

		if (this._currentWorldGenerator != null)
		{
			if (this._worldTemperatureSlider != null)
			{
				float temperatureValue = (_currentWorldGenerator.globalTemperature - WorldGenerator.globalTemperature_min) / (WorldGenerator.globalTemperature_max - WorldGenerator.globalTemperature_min);
				this._worldTemperatureSlider.value = temperatureValue;
				this._currentWorldTemperatureSliderValue = temperatureValue;

				if(this._worldTemperatureLabel != null)
				{
					SetWorldTemperatureLabel(Mathf.Lerp(WorldGenerator.globalTemperature_min, WorldGenerator.globalTemperature_max, temperatureValue));
				}
			}
			if (this._worldWindDirectionSlider != null)
			{
				float windDirectionValue = (this._currentWorldGenerator.windDirection - WorldGenerator.windDirection_min) / (WorldGenerator.windDirection_max - WorldGenerator.windDirection_min);
				this._worldWindDirectionSlider.value = windDirectionValue;
				this._currentWindDirectionSliderValue = windDirectionValue;
				
				if (this._worldWindDirectionLabel != null)
				{
					SetWindDirectionLabel(Mathf.Lerp(WorldGenerator.windDirection_min, WorldGenerator.windDirection_max, windDirectionValue));
				}
			}
			if(this._worldWindSpeedSlider != null)
			{
				float windSpeedValue = (this._currentWorldGenerator.windSpeed - WorldGenerator.windSpeed_min) / (WorldGenerator.windSpeed_max - WorldGenerator.windSpeed_min);
				this._worldWindSpeedSlider.value = windSpeedValue;
				this._currentWindSpeedSliderValue = windSpeedValue;

				if (this._worldWindSpeedLabel != null)
				{
					SetWindSpeedLabel(Mathf.Lerp(WorldGenerator.windSpeed_min, WorldGenerator.windSpeed_max, windSpeedValue));
				}
			}

			if(this._windDirectionArrowGO != null)
			{
				Vector3 windForward = this._windDirectionArrowGO.transform.up; //arrow is rotated by 90 in pitch
				this._currentWorldGenerator.UpdateWindDirection(windForward);
			}

			if (_currentSeedLabel != null) {
				_currentSeedLabel.text = WorldGenerator.Instance.currentSeed.ToString();
			}

			if (this._worldSizeSlider != null)
			{
				int worldSizeValue = this._currentWorldGenerator.sizeX;

				this._worldSizeSlider.wholeNumbers = true;
				this._worldSizeSlider.minValue = WorldGenerator.worldSize_min;
				this._worldSizeSlider.maxValue = WorldGenerator.worldSize_max;
				
				this._worldSizeSlider.value = worldSizeValue;
				
				this._currentWorldSize = this._currentWorldGenerator.sizeX;
				this._lastWorldSize = this._currentWorldGenerator.sizeX;

				if (this._worldSizeLabel != null)
				{
					SetWorldSizeLabel(this._currentWorldSize);
				}
			}
		}

		InitializeSelectableToggle();
	}
	void Update () 
	{
		this.ProcesReadDeltaTime();
		this.ProcesWorldTemperature();
		this.ProcesWind();
		this.ProcesWindDirectionArrow();
		this.ProcessWorldSize();
		this.ProcessCellMass();
	}

	#endregion MonobehaviourMethods

	#region Methods

	private void ProcesWorldTemperature()
	{
		if(this._worldTemperatureSlider != null)
		{
			float worldTemperatureSliderTmpValue = this._worldTemperatureSlider.value;
			if(worldTemperatureSliderTmpValue != this._currentWorldTemperatureSliderValue)
			{
				this._currentWorldTemperatureSliderValue = worldTemperatureSliderTmpValue;
				float worldTemperature = Mathf.Lerp(WorldGenerator.globalTemperature_min, WorldGenerator.globalTemperature_max, worldTemperatureSliderTmpValue);
				this._currentWorldGenerator.globalTemperature = worldTemperature;
				if (this._worldTemperatureLabel != null)
				{
					SetWorldTemperatureLabel(worldTemperature);
				}
			}
		}
	}

	private void ProcesWind()
	{
		if (this._worldWindDirectionSlider != null)
		{
			float worldWindDirectionSliderTmpValue = this._worldWindDirectionSlider.value;
			if (worldWindDirectionSliderTmpValue != this._currentWindDirectionSliderValue)
			{
				//show wind arrow
				this._windDirectionArrowTimer = 0.0f;

				this._currentWindDirectionSliderValue = worldWindDirectionSliderTmpValue;
				float windDirection = Mathf.Lerp(WorldGenerator.windDirection_min, WorldGenerator.windDirection_max, worldWindDirectionSliderTmpValue);
				this._currentWorldGenerator.windDirection = windDirection;
				if (this._worldWindDirectionLabel != null)
				{
					SetWindDirectionLabel(windDirection);
				}
			}
		}

		if (this._worldWindSpeedSlider != null)
		{
			float worldWindSpeedSliderTmpValue = this._worldWindSpeedSlider.value;
			if (worldWindSpeedSliderTmpValue != this._currentWindSpeedSliderValue)
			{
				this._currentWindSpeedSliderValue = worldWindSpeedSliderTmpValue;
				float windSpeed = Mathf.Lerp(WorldGenerator.windSpeed_min, WorldGenerator.windSpeed_max, worldWindSpeedSliderTmpValue);
				this._currentWorldGenerator.windSpeed = windSpeed;
				this._windZoneObject.windMain = windSpeed / 200f;

				if (this._worldWindSpeedLabel != null)
				{
					SetWindSpeedLabel(windSpeed);
				}
			}
		}
	}

	private void ProcesReadDeltaTime()
	{
		float tmpRealTime = Time.realtimeSinceStartup;
		this._realDeltaTime = tmpRealTime - this._lastRealTime;
		this._lastRealTime = tmpRealTime;
	}

	private void ProcesWindDirectionArrow()
	{
		if(this._windDirectionArrowGO != null && this._windDirectionArrowMeshRenderer != null)
		{
			this._windDirectionArrowTimer += Time.deltaTime;
			if(this._windDirectionArrowTimer < this._windDirectionArrowTimeToFadeOut)
			{
				if (!_windDirectionArrowMeshRenderer.enabled) _windDirectionArrowMeshRenderer.enabled = true;

				Material arrowMaterial = this._windDirectionArrowMeshRenderer.material;

				float alpha = Mathf.Lerp(this._windDirectionArrowMaxAlpha, 0.0f, this._windDirectionArrowTimer / this._windDirectionArrowTimeToFadeOut);
				Color arrowColor = arrowMaterial.GetColor(_arrowMaterialColor);
				arrowColor.a = alpha;
				arrowMaterial.SetColor(_arrowMaterialColor, arrowColor);

				Quaternion arrowQuat = this._windDirectionArrowGO.transform.localRotation;
				Vector3 arrowRot = arrowQuat.eulerAngles;
				arrowRot.y = this._currentWorldGenerator.windDirection;
				arrowQuat.eulerAngles = arrowRot;
				this._windDirectionArrowGO.transform.localRotation = arrowQuat;

				Vector3 windForward = this._windDirectionArrowGO.transform.up; //arrow is rotated by 90 in pitch
				this._currentWorldGenerator.UpdateWindDirection(windForward);

			}else{
				if (_windDirectionArrowMeshRenderer.enabled) _windDirectionArrowMeshRenderer.enabled = false;
			}
		}
	}

	private void ProcessWorldSize()
	{
		if (this._worldSizeSlider != null && this._currentWorldGenerator != null)
		{
			this._currentWorldSize = (int)this._worldSizeSlider.value;
			if (this._currentWorldSize != this._lastWorldSize)
			{
				this._currentWorldGenerator.SetNewWorldSize(this._currentWorldSize, this._currentWorldSize);
				this.SetWorldSizeLabel(this._currentWorldSize);
			}
			this._lastWorldSize = this._currentWorldSize;
		}
	}

	private void ProcessCellMass()
	{
		if (this._cellMassSlider != null && this._currentWorldGenerator != null)
		{
			this._currentWorldGenerator.cellMassRandomization = _cellMassSlider.value / 100f;
			SetCellMassLabel(Mathf.RoundToInt(_cellMassSlider.value));
		}
	}

	private void SetWorldTemperatureLabel(float val) {
		val = Mathf.Round(val * 10) / 10f;
		_worldTemperatureLabel.text = val + "°C";
	}

	private void SetWindDirectionLabel(float val) {
		val = Mathf.Round(val * 10) / 10f;
		_worldWindDirectionLabel.text = val + "°";
	}

	private void SetWindSpeedLabel(float val) {
		val = Mathf.Round(val * 10) / 10f;
		_worldWindSpeedLabel.text = val + "km/h";
	}

	private void SetWorldSizeLabel(int value)
	{
		this._worldSizeLabel.text = value + "Cells";
	}

	private void SetCellMassLabel(int value)
	{
		this._cellMassLabel.text = value + "%";
	}

	private void InitializeSelectableToggle()
	{
		WorldGenerator instance = WorldGenerator.Instance;
		int materialCount = instance.materialPrefabs.Count;
		for(int i = 0;i < materialCount;++i)
		{
			GameObject tmpGO = (GameObject)GameObject.Instantiate(this.exampleToogle.gameObject);
			tmpGO.SetActive(true);
			RectTransform tmpToggleTransform = tmpGO.GetComponent<RectTransform>();
			
			tmpToggleTransform.SetParent(this.toggleParent.transform);
			
			Vector2 anchoredPosition = tmpToggleTransform.anchoredPosition;
			anchoredPosition.y = this.toggleLocalInitialY + toggleLocalOffsetY * i;
			anchoredPosition.x = 0.0f;
			tmpToggleTransform.anchoredPosition = anchoredPosition;

			Text tmpToggleText = tmpGO.GetComponentInChildren<Text>();
			if(tmpToggleText != null)
			{
				tmpToggleText.text = instance.materialPrefabs[i].name;
			}

			Toggle tmpToggle = tmpGO.GetComponent<Toggle>();

			tmpToggle.isOn = instance.selectableSpawn[i];

			int index = i;
			UnityEngine.Events.UnityAction<bool> onToggle = (bool arg) =>
			{
				ToggleMaterialByIndex(index, arg);
			};

			tmpToggle.onValueChanged.AddListener(onToggle);
		}
	}
	

	public void NewSeededMap()
	{
		WorldGenerator.Instance.GenerateNewSeed();
		WorldGenerator.Instance.Generate();
		_currentSeedLabel.text = WorldGenerator.Instance.currentSeed.ToString();
	}
	public void GenerateMap()
	{
		WorldGenerator.Instance.Generate();
	}
	public void ClearMap()
	{
		WorldGenerator.Instance.ClearWorld();
	}

	public void ToggleMaterialByIndex(int indexToToggle,bool state)
	{
		//Debug.LogFormat("Params: {0} {1}",indexToToggle,state);
		WorldGenerator.Instance.selectableSpawn[indexToToggle] = state;
	}

	#endregion Methods
}
