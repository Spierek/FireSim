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

	#endregion Variables

	#region Monobehaviour Methods

	void Start () 
	{
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
					this._worldTemperatureLabel.text = Mathf.Lerp(WorldGenerator.globalTemperature_min, WorldGenerator.globalTemperature_max, temperatureValue).ToString();
				}
			}
			if (this._worldWindDirectionSlider != null)
			{
				float windDirectionValue = (this._currentWorldGenerator.windDirection - WorldGenerator.windDirection_min) / (WorldGenerator.windDirection_max - WorldGenerator.windDirection_min);
				this._worldWindDirectionSlider.value = windDirectionValue;
				this._currentWindDirectionSliderValue = windDirectionValue;
				
				if (this._worldWindDirectionLabel != null)
				{
					this._worldWindDirectionLabel.text = Mathf.Lerp(WorldGenerator.windDirection_min, WorldGenerator.windDirection_max, windDirectionValue).ToString();
				}
			}
			if(this._worldWindSpeedSlider != null)
			{
				float windSpeedValue = (this._currentWorldGenerator.windSpeed - WorldGenerator.windSpeed_min) / (WorldGenerator.windSpeed_max - WorldGenerator.windSpeed_min);
				this._worldWindSpeedSlider.value = windSpeedValue;
				this._currentWindSpeedSliderValue = windSpeedValue;

				if (this._worldWindSpeedLabel != null)
				{
					this._worldWindSpeedLabel.text = Mathf.Lerp(WorldGenerator.windSpeed_min, WorldGenerator.windSpeed_max, windSpeedValue).ToString();
				}
			}
		}
	}
	void Update () 
	{
		this.ProcesWorldTemperature();
		this.ProcesWind();
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
					this._worldTemperatureLabel.text = worldTemperature.ToString();
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
				this._currentWindDirectionSliderValue = worldWindDirectionSliderTmpValue;
				float windDirection = Mathf.Lerp(WorldGenerator.windDirection_min, WorldGenerator.windDirection_max, worldWindDirectionSliderTmpValue);
				this._currentWorldGenerator.windDirection = windDirection;
				if (this._worldWindDirectionLabel != null)
				{
					this._worldWindDirectionLabel.text = windDirection.ToString();
				}
			}
		}

		if (this._worldWindSpeedSlider != null)
		{
			float worldWindSpeedSliderTmpValue = this._worldWindSpeedSlider.value;
			if (worldWindSpeedSliderTmpValue != this._currentWindSpeedSliderValue)
			{
				this._currentWindDirectionSliderValue = worldWindSpeedSliderTmpValue;
				float windSpeed = Mathf.Lerp(WorldGenerator.windSpeed_min, WorldGenerator.windSpeed_max, worldWindSpeedSliderTmpValue);
				this._currentWorldGenerator.windSpeed = windSpeed;
				if (this._worldWindSpeedLabel != null)
				{
					this._worldWindSpeedLabel.text = windSpeed.ToString();
				}
			}
		}
	}

	#endregion Methods
}
