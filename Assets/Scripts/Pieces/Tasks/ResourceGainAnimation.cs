using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ResourceGainAnimation : Task
{
    private int numResources;
    private GameObject[] resourceTokens;
    private bool[] resourceTokensMade;
    private bool[] resourceTokensDestroyed;
    private float timeElapsed;
    private float duration;
    private float staggerTime;
    private Vector3 basePos;
    private Vector3 endPos;

    public ResourceGainAnimation(int numResources_, Vector3 basePos_, int playerNum)
    {
        numResources = numResources_;
        basePos = basePos_;
        endPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
            Services.UIManager.resourceCounters[playerNum - 1]
            .GetComponentInChildren<Image>().transform.position);
        endPos = new Vector3(endPos.x, endPos.y, -1);
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Polyomino.resourceGainAnimDur;
        staggerTime = Polyomino.resourceGainAnimStaggerTime;
        resourceTokens = new GameObject[numResources];
        resourceTokensMade = new bool[numResources];
        resourceTokensDestroyed = new bool[numResources];
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < numResources; i++)
        {
            if(timeElapsed >= i * staggerTime && timeElapsed <= i * staggerTime + duration)
            {
                if (!resourceTokensMade[i])
                {
                    GameObject resourceToken = GameObject.Instantiate(Services.Prefabs.ResourceToken);
                    resourceToken.transform.position = basePos;
                    resourceToken.transform.rotation = Quaternion.Euler(0, 0, 45);
                    resourceTokens[i] = resourceToken;
                    resourceTokensMade[i] = true;
                }
                Vector3 noiseOffset = (-Polyomino.resourceGainAnimNoiseMag / 2 * new Vector3(1, 1, 0)) + new Vector3(Mathf.PerlinNoise(i * 100000, Time.time * Polyomino.resourceGainAnimNoiseSpeed),
                    Mathf.PerlinNoise(i * 10000000, Time.time * Polyomino.resourceGainAnimNoiseSpeed), 0)
                    * Polyomino.resourceGainAnimNoiseMag;
                if(timeElapsed - (i *staggerTime) <= duration / 2)
                {
                    noiseOffset = Vector3.Lerp(Vector3.zero, noiseOffset,
                        EasingEquations.Easing.QuadEaseOut((timeElapsed - 
                        (i * staggerTime)) / (duration / 2)));
                }
                else
                {
                    noiseOffset = Vector3.Lerp(noiseOffset, Vector3.zero,
                        EasingEquations.Easing.QuadEaseIn((timeElapsed -
                        (i * staggerTime) - (duration/2)) / (duration / 2)));
                }
                resourceTokens[i].transform.position = Vector3.Lerp(basePos, endPos,
                    EasingEquations.Easing.QuadEaseOut((timeElapsed - (i * staggerTime)) / duration))
                    + noiseOffset;
            }
            else if (resourceTokensMade[i] && !resourceTokensDestroyed[i])
            {
                GameObject.Destroy(resourceTokens[i]);
                resourceTokensDestroyed[i] = true;
            }
        }

        if (timeElapsed >= duration + (numResources * staggerTime))
            SetStatus(TaskStatus.Success);
    }
}
