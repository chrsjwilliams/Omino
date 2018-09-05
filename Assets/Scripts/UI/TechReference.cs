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

    public void Init(BuildingType techType)
    {
        icon.sprite = Services.TechDataLibrary.GetIcon(techType);
        TechBuilding tech = TechBuilding.GetBuildingFromType(techType);
        nameText.text = tech.GetName().ToLower();
        description.text = tech.GetDescription().ToLower();
        back.color = Services.GameManager.NeutralColor;
    }
}
