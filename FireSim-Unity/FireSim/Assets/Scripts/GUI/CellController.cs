﻿using UnityEngine;
using UnityEngine.UI;
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
			ProcessCellParamPanel();
		}
	}
	private Cell _lastSelectedCell = null;
	private int _cellLayerMask = 0;

	[SerializeField]
	private GameObject _cellControlPanel = null;

	[SerializeField]
	private GameObject _igniteButton = null;
	[SerializeField]
	private Text _igniteText = null;

	[SerializeField]
	private Slider _cellMassSlider = null;
	[SerializeField]
	private Text _cellMassLabel = null;

	[SerializeField]
	private Slider _cellMoistureSlider = null;
	[SerializeField]
	private Text _cellMoistureLabel = null;

	private float _lastCellMassSliderValue = 0.0f;
	private float _lastCellMoistureValue = 0.0f;

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

	private void ProcessCellParamPanel()
	{
		if(this._selectedCell != null)
		{
			this._cellControlPanel.SetActive(true);
			if(this._selectedCell.IsBurning)
			{
				this._igniteButton.SetActive(false);
			}else{
				this._igniteButton.SetActive(true);
			}
			
			if(this._selectedCell.materialMass > 0.0f)
			{
				if(this._selectedCell.IsBurning)
				{
					this._igniteText.text = "Cell is burning";
				}else{
					this._igniteText.text = "Cell is not burning";
				}
			}
			else
			{
				this._igniteText.text = "Cell is all burned out";
			}

		}else{
			this._cellControlPanel.SetActive(false);
		}
	}

	private void ProcessCellSelection()
	{
		if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftAlt)) )
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
			}else{
				this.SelectedCell = null;
			}
		}
	}

	private void ProcesParams()
	{
		if(this._selectedCell != null)
		{
			if(this._selectedCell != this._lastSelectedCell)
			{
				this.InitPanel();
			}

			//stuff
			float massSliderValue = this._cellMassSlider.value;
			if(massSliderValue != this._lastCellMassSliderValue)
			{
				this._selectedCell.materialMass = massSliderValue * WorldGenerator.cellMass_max;
				SetMassLabel(this._selectedCell.materialMass);
			}
			this._lastCellMassSliderValue = massSliderValue;
		}
		this._lastSelectedCell = this._selectedCell;
	}

	private void InitPanel()
	{
		if(this._selectedCell != null)
		{
			float massSliderValue = this._selectedCell.materialMass / WorldGenerator.cellMass_max;
			this._lastCellMassSliderValue = massSliderValue;
			this._cellMassSlider.value = massSliderValue;
			SetMassLabel(this._selectedCell.materialMass);
		}
	}

	private void SetMassLabel(float mass)
	{
		if(this._cellMassLabel != null)
		{
			this._cellMassLabel.text = mass + "kg";
		}
	}

	public void Ignite()
	{
		if(this.SelectedCell != null)
		{
			this.SelectedCell.Ignite();
			
		}
	}

	#endregion Methods
}
