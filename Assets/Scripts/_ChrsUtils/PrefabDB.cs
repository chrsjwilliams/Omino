using UnityEngine;


[CreateAssetMenu (menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject
{

    [SerializeField] private Player _player;
    public Player Player
    {
        get { return _player; }
    }

    [SerializeField] private GameObject[] _scenes;
    public GameObject[] Scenes
    {
        get { return _scenes; }
    }

    [SerializeField] private Tile _tile;
    public Tile Tile
    {
        get { return _tile; }
    }

    [SerializeField] private GameObject mapTile;
    public GameObject MapTile { get { return mapTile; } }

    [SerializeField] private GameObject pieceHolder;
    public GameObject PieceHolder
    {
        get { return pieceHolder; }
    }

    [SerializeField] private GameObject floatingText;
    public GameObject FloatingText { get { return floatingText; } }

    [SerializeField] private GameObject tooltip;
    public GameObject Tooltip { get { return tooltip; } }

    [SerializeField] private GameObject dustCloud;
    public GameObject DustCloud { get { return dustCloud; } }

    [SerializeField] private GameObject fireBurst;
    public GameObject FireBurst { get { return fireBurst; } }

    [SerializeField] private GameObject structClaimEffect;
    public GameObject StructClaimEffect { get { return structClaimEffect; } }

    [SerializeField] private GameObject tutorialTooltip;
    public GameObject TutorialTooltip { get { return tutorialTooltip; } }

    [SerializeField] private GameObject dangerEffect;
    public GameObject DangerEffect { get { return dangerEffect; } }

    [SerializeField] private GameObject rippleEffect;
    public GameObject RippleEffect { get { return rippleEffect; } }

    [SerializeField] private GameObject rankUp;
    public GameObject RankUp { get { return rankUp; } }

    [SerializeField] private GameObject rankDown;
    public GameObject RankDown { get { return rankDown; } }

    [SerializeField] private GameObject starsplosion;
    public GameObject Starsplosion { get { return starsplosion; } }
    
    [SerializeField] private GameObject placementfetti;
    public GameObject Placementfetti { get { return placementfetti; } }
    
    [SerializeField] private GameObject structfetti;
    public GameObject Structfetti { get { return structfetti; } }
}
