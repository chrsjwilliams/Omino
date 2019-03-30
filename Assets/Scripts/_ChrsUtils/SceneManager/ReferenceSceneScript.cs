using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceSceneScript : Scene<TransitionData> {

    [SerializeField]
    private GameObject techRefPrefab;
    private const float referenceBaseX = -250;
    private const float referenceBaseY = -200;
    private const float referenceYSpacing = 250;
    [SerializeField]
    private GameObject referenceHolder;
    private CanvasScroller canvasScroller;

    internal override void Init()
    {
        base.Init();
        for (int i = 0; i < TechBuilding.techTypes.Length; i++)
        {
            
            
                
        
            TechReference techRef = Instantiate(techRefPrefab, referenceHolder.transform).GetComponent<TechReference>();
            techRef.Init(TechBuilding.techTypes[i]);
            techRef.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(referenceBaseX, 
                referenceBaseY - i * referenceYSpacing);
            if (Services.GameManager.mode == TitleSceneScript.GameMode.Edit)
            {
                techRef.TurnOnTech();
            }
        }
        canvasScroller = referenceHolder.GetComponent<CanvasScroller>();
        canvasScroller.minY = referenceHolder.GetComponent<RectTransform>().anchoredPosition.y;
        canvasScroller.maxY = canvasScroller.minY + ((TechBuilding.techTypes.Length - 3) * referenceYSpacing);
    }


    internal override void OnEnter(TransitionData data)
    {
                                                                                     

        base.OnEnter(data);

    }

    // Update is called once per frame
    void Update () {
		
	}
}
