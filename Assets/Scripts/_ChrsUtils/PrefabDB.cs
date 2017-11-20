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

}
