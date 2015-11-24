using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CellController : MonoBehaviour
{
	#region Variables

	private Cell _selectedCell = null;
	public Cell SelectedCell
	{
		get
		{
			return this._selectedCell;
		}
		set
		{
			this._selectedCell = value;
		}
	}
	private Cell _lastSelectedCell = null;
	private int _cellLayerMask = 0;

	#endregion Variables

	#region Monobahviour Methods
	
	void Awake()
	{
		this._cellLayerMask = 0;
		this._cellLayerMask |= (1 << LayerMask.NameToLayer("cells"));
	}

	void Start () 
	{
	
	}
	
	void Update () 
	{
		this.ProcessCellSelection();
		this.ProcesParams();
	}
	
	#endregion Monobahaviour Methods

	#region Methods

	private void ProcessCellSelection()
	{
		if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			if(this.SelectedCell != null)
			{
				this.SelectedCell.IsSelected = false;
			}

			RaycastHit hit;
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(mouseRay,out hit ,float.MaxValue,this._cellLayerMask))
			{
				this.SelectedCell = hit.collider.gameObject.GetComponent<Cell>();
				
				if(this.SelectedCell != null)
				{
					this.SelectedCell.IsSelected = true;
				}
			}
		}
	}

	private void ProcesParams()
	{
		if(this._selectedCell != null)
		{
			if(this._selectedCell != this._lastSelectedCell)
			{

			}
		}

		this._lastSelectedCell = this._selectedCell;
	}

	#endregion Methods
}
