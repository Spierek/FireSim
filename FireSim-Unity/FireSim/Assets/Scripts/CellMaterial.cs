using UnityEngine;
using System.Collections;

// basically a container for material variables, inherits from MonoBehaviour so that it's tied to an instantiated visual object for given material
public class CellMaterial : MonoBehaviour {
	#region Variables
	[Range(100, 2000)]
	public float combustionTemperature;
	[Range(5, 20)]
	public float fuelEnergy;
	[Range(0.05f, 5)]
	public float burnSpeed;
	[Range(5, 25)]
	public float heatTransferCoefficient;
	[Range(0, 1)]
	public float emissivity;
	#endregion
}
