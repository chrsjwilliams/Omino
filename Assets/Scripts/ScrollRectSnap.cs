using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectSnap : MonoBehaviour
{
    public bool dragging;
    public RectTransform panel;
    public Image[] rules;
    public RectTransform center;

    private float[] distance;
    private int ruleDistance; 
    private int minRuleNumber;

	// Use this for initialization
	public void Init ()
    {
        panel = GameObject.Find("Scroll Panel").GetComponent<RectTransform>();
        center = GameObject.Find("CenterToCompare").GetComponent<RectTransform>();
        int numOfRules = panel.transform.childCount;
        rules = new Image[numOfRules];
        for (int i = 0; i < numOfRules; i++)
        {
            rules[i] = panel.transform.GetChild(i).GetComponent<Image>();
        }

        distance = new float[numOfRules];

        ruleDistance = (int)Mathf.Abs(rules[2].GetComponent<RectTransform>().anchoredPosition.x - rules[1].GetComponent<RectTransform>().anchoredPosition.x);
	}
	
	// Update is called once per frame
	void Update ()
    {
        for (int i = 0; i < rules.Length; i++)
        {
            distance[i] = Mathf.Abs(center.transform.position.x - rules[i].transform.position.x);
        }

        float minDistance = Mathf.Min(distance);

        for (int i = 0; i < rules.Length; i++)
        {
            if (minDistance == distance[i])
            {
                minRuleNumber = i;
                if (minRuleNumber > 5)
                    minRuleNumber = 5;
            }
        }

        if (!dragging)
        {
            LerpToRule(minRuleNumber * -ruleDistance);
        }
    }

    void LerpToRule(int position)
    {
        float posX = Mathf.Lerp(panel.anchoredPosition.x, position, Time.deltaTime * 4f);
        Vector2 newPos = new Vector2(posX, panel.anchoredPosition.y);

        panel.anchoredPosition = newPos;
    }
}
