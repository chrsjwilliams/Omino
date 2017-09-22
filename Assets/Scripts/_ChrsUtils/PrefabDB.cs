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

    [SerializeField] private GameObject[] _cursors;
    public GameObject[] Cursors
    {
        get { return _cursors; }
    }

    [SerializeField] private GameObject _netCursor;
    public GameObject NetCursor
    {
        get { return _netCursor; }
    }
}
