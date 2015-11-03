using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour {
	#region Variables
	public int			x;
	public int			y;

	[Header("Cell Properties")]
	public CellMaterial materialType;
	[Range(0, 2000)]
	public float		currentTemperature;
	public float		materialMass;
	public float		cellSize;

	// preset values
	private Vector2			materialMassRange = new Vector2(1, 100);
	private ParticleSystem	fireParticles;
	#endregion

	#region Monobehaviour
	void Awake () {
		fireParticles = transform.Find("FireParticles").GetComponent<ParticleSystem>();
	}
	
	void Update () {
		if (Input.GetKeyDown(KeyCode.A)) {
			Ignite();
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
	}

	// uses a seeded random instance for deterministic generation
	public void SetValues(System.Random rand) {
		materialMass = rand.Next((int)materialMassRange.x, (int)materialMassRange.y * 100) / 100f;
	}

	public void Ignite() {
		currentTemperature = materialType.ignitionTemperature;
		fireParticles.Play();
	}
	#endregion
}
