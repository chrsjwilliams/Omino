using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditOptionsSceneScript : Scene<TransitionData>
{
    public bool useExpansion { get; protected set; }
    public Button expansionButton;

    public int dimension { get; protected set; }
    public Slider dimensionSlider;

    public BuildingType[] availableTech;

	// Use this for initialization
	void Start () {
        useExpansion = true;
        dimension = 10;
        Services.GameManager.mode = TitleSceneScript.GameMode.Edit;
	}

    internal override void OnEnter(TransitionData data)
    {

    }

    internal override void OnExit()
    {
    }

    public void StartEditing()
    {
        Level editLevel = new Level();
        editLevel.SetLevelData(dimension, dimension);
        Services.GameManager.SetCurrentLevel(editLevel);
        Services.Scenes.Swap<EditSceneScript>();
    }


    public void ToggleDimension()
    {
        dimension = (int)dimensionSlider.value;
        dimensionSlider.GetComponentInChildren<TextMeshProUGUI>().text = dimension + "x" + dimension;
    }


    // Update is called once per frame
    void Update () {
		
	}
}
