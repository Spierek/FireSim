using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {
	#region Variables
	public int x;
	public int y;
	#endregion

	#region Monobehaviour
	void Start () {
	
	}
	
	void Update () {
	
	}
	#endregion

	#region Methods
	public void Setup(int x, int y) {
		this.x = x;
		this.y = y;

		gameObject.name = "Cell(" + x + "," + y + ")";

		//TODO: randomize material, setup visuals
	}
	#endregion
}
