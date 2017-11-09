using UnityEngine;
using System.Collections;

public class FloatText : Task
{
    private string text;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timeElapsed;
    private float duration;
    private TextMesh textMesh;
    private Player player;
    private Color baseColor;
    private Color alphaOutColor;

    public FloatText(string text_, Vector3 startPos_, Player player_, 
        float dist, float duration_)
    {
        text = text_;
        startPos = startPos_;
        player = player_;
        duration = duration_;
        Vector3 direction = player.playerNum == 1 ? Vector3.right : Vector3.left;
        targetPos = startPos + (dist * direction);
    }

    protected override void Init()
    {
        Quaternion rot = 
            player.playerNum == 1 ? Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 90);
        textMesh = GameObject.Instantiate(Services.Prefabs.FloatingText, startPos,
            rot, Services.GameScene.transform).GetComponent<TextMesh>();
        textMesh.GetComponent<MeshRenderer>().sortingOrder = 10;
        textMesh.text = text;
        timeElapsed = 0;
        baseColor = textMesh.color;
        alphaOutColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        textMesh.transform.position = Vector3.Lerp(startPos, targetPos, 
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        textMesh.color = Color.Lerp(baseColor, alphaOutColor,
            EasingEquations.Easing.ExpoEaseIn(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        GameObject.Destroy(textMesh.gameObject);
    }
}
