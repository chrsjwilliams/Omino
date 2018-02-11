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

    [SerializeField] private GameObject pieceHolder;
    public GameObject PieceHolder
    {
        get { return pieceHolder; }
    }

    [SerializeField] private GameObject ringTimer;
    public GameObject RingTimer { get { return ringTimer; } }

    [SerializeField] private GameObject floatingText;
    public GameObject FloatingText { get { return floatingText; } }

    [SerializeField] private GameObject resourceToken;
    public GameObject ResourceToken { get { return resourceToken; } }

    [SerializeField] private GameObject structureParticles;
    public GameObject StructureParticles { get { return structureParticles; } }

    [SerializeField] private GameObject tooltip;
    public GameObject Tooltip { get { return tooltip; } }

    [SerializeField] private GameObject shield;
    public GameObject Shield { get { return shield; } }

    [SerializeField] private GameObject queueBar;
    public GameObject QueueBar { get { return queueBar; } }

    [SerializeField] private GameObject dustCloud;
    public GameObject DustCloud { get { return dustCloud; } }

    [SerializeField] private GameObject fireBurst;
    public GameObject FireBurst { get { return fireBurst; } }

    [SerializeField] private GameObject rotationUI;
    public GameObject RotationUI { get { return rotationUI; } }
}
