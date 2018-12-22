using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditModeUIEntryAnimation : Task
{
    private const float animDuration = 0.3f;
    private const float staggerTime = 0.05f;
    private const float buttonOffset = 10f;

    private float timeElapsed;
    private Vector3 editModeBuildingStartPosition;
    private Vector3 editModeBuildingTargetPosition;
    private Vector3 expansionToggleStartPosition;
    private Vector3 expansionToggleTargetPosition;
    private Vector3[] buttonStartPositions;
    private Vector3[] buttonTargetPositions;

    private List<Button> terrainButtons;
    private EditModeBuilding editModeBuilding;
    private GameObject expansionToggle;

    public EditModeUIEntryAnimation(EditModeBuilding editModeBuilding_, List<Button> terrainButtons_, GameObject expansionToggle_)
    {
       
        editModeBuilding = editModeBuilding_;
        terrainButtons = terrainButtons_;
        expansionToggle = expansionToggle_;
    }

    protected override void Init()
    {
        timeElapsed = 0;

        buttonStartPositions = new Vector3[terrainButtons.Count];
        buttonTargetPositions = new Vector3[terrainButtons.Count];

        editModeBuildingTargetPosition = editModeBuilding.holder.transform.localPosition;
        editModeBuilding.holder.transform.localPosition += buttonOffset * Vector3.down;
        editModeBuildingStartPosition = editModeBuilding.holder.transform.localPosition;

        expansionToggleTargetPosition = expansionToggle.transform.localPosition;
        expansionToggle.transform.localPosition += 300 * Vector3.up;
        expansionToggleStartPosition = expansionToggle.transform.localPosition;

        expansionToggle.gameObject.SetActive(false);


        for (int i = 0; i < terrainButtons.Count; i++)
        {
            Button terrainButton = terrainButtons[i];
            buttonTargetPositions[i] = terrainButton.transform.localPosition;

            terrainButton.transform.localPosition += 400 * Vector3.down;
    
            buttonStartPositions[i] = terrainButton.transform.localPosition;
            terrainButton.transform.localPosition = buttonStartPositions[i];
            terrainButtons[i].gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    internal override void Update ()
    {
        timeElapsed += Time.deltaTime;
        editModeBuilding.holder.gameObject.SetActive(true);

        editModeBuilding.holder.transform.localPosition = Vector3.Lerp(
              editModeBuildingStartPosition,
              editModeBuildingTargetPosition,
              EasingEquations.Easing.QuadEaseOut(
                Mathf.Min(1, (timeElapsed - (staggerTime)) / animDuration)));

        expansionToggle.SetActive(true);
        expansionToggle.transform.localPosition = Vector3.Lerp(
              expansionToggleStartPosition,
              expansionToggleTargetPosition,
              EasingEquations.Easing.QuadEaseOut(
                Mathf.Min(1, (timeElapsed - (staggerTime)) / animDuration)));

        for (int i = 0; i < terrainButtons.Count; i++)
        {
            Button button = terrainButtons[i];
            button.gameObject.SetActive(true);

            button.transform.localPosition = Vector3.Lerp(
                buttonStartPositions[i],
                buttonTargetPositions[i],
                EasingEquations.Easing.QuadEaseOut(
                    Mathf.Min(1, (timeElapsed - (i * staggerTime)) / animDuration)));
        }

        if (timeElapsed >= animDuration + 1 * staggerTime)
        {
            for (int i = 0; i < terrainButtons.Count; i++)
            {
                terrainButtons[i].transform.localPosition = buttonTargetPositions[i];
            }
                SetStatus(TaskStatus.Success);
        }
    }
}
