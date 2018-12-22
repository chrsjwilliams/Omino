using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TechReference : MonoBehaviour
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI description;
    [SerializeField]
    private Image back;
    [SerializeField]
    public bool availableInEditMode { get { return available; } private set { available = value; } }
    private bool available = true;

    public void Init(BuildingType techType)
    {
        icon.sprite = Services.TechDataLibrary.GetIcon(techType);
        TechBuilding tech = TechBuilding.GetBuildingFromType(techType);
        nameText.text = tech.GetName().ToLower();
        description.text = tech.GetDescription().ToLower();
        back.color = Services.GameManager.NeutralColor;
    }

    public void TurnOnTech()
    {
        availableInEditMode = true;
        back.color = new Color(108f/255f, 174f/255f, 117f/255f);
        icon.color = new Color(108f / 255f, 174f / 255f, 117f / 255f);
    }

    public void ToggleTechAvailability()
    {
        if (Services.GameManager.mode != TitleSceneScript.GameMode.Edit) return;

        availableInEditMode = !availableInEditMode;
        if (availableInEditMode)
        {
            back.color = new Color(108f / 255f, 174f / 255f, 117f / 255f);
            icon.color = new Color(108f / 255f, 174f / 255f, 117f / 255f);
        }
        else
        {
            back.color = Color.white;
            icon.color = Color.white;
        }
        
    }
}
