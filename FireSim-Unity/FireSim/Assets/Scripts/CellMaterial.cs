using UnityEngine;
using System.Collections;

// basically a container for material variables, inherits from MonoBehaviour so that it's tied to an instantiated visual object for given material
public class CellMaterial : MonoBehaviour
{
	#region Constants

	public const float specificHeat_water = 4200.0f;
	public const float vaporizationHeat = 2257.0f;
	public const float vaporizationTemperature = 100.0f;

	#endregion Constants

	#region Variables
	[Range(100, 2000)]
	public float ignitionTemperature;
	[Range(5, 20)]
	public float fuelEnergy;
	[Range(0.05f, 5)]
	public float burnSpeed;
	[Range(5, 25)]
	public float heatTransferCoefficient;
	[Range(0, 1)]
	public float emissivity;
	[Range(0.0f, 60.0f)]
	public float moisture;

	public float specificHeat;

	#endregion
}
