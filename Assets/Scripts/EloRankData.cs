using UnityEngine;

[CreateAssetMenu(menuName = "Elo Rank Data")]
public class EloRankData : ScriptableObject
{
    [SerializeField]
    private Sprite[] rankSprites;
    public Sprite[] RankSprites { get { return rankSprites; } }
}
