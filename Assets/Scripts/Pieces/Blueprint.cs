using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : Polyomino
{
    protected static int[,,] factory = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 0,0,0,0,0 },
                { 0,1,1,1,1 },
                { 0,1,1,1,1 },
                { 0,1,1,1,1 },
                { 0,0,0,0,0 }
            }
    };
    protected static int[,,] mine = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 0,0,0,0,0 },
                { 1,1,1,1,1 },
                { 1,1,1,1,1 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            }
    };

    public Blueprint(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
    }

    public Blueprint(BuildingType _buildingType, Player _player) : base(0, 0, _player)
    {
        buildingType = _buildingType;
        owner = _player;

        switch (buildingType)
        {
            case BuildingType.FACTORY:
                holderName = "FactoryHolder";
                piece = factory;
                break;
            case BuildingType.MINE:
                holderName = "MineHolder";
                piece = mine;
                break;
            default:
                break;
        }

    }

    protected bool PiecesShareOwner(Tile tile)
    {
        if (tile.occupyingPiece == null) return false;
        if (tile.occupyingPiece.buildingType == BuildingType.BASE) return false;
        return owner == tile.occupyingPiece.owner;
    }

    public override bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //  CONDITIONS:
        //      Blueprint tiles are contained in the map
        //      All tiles in blueprint are placed on tiles that belong to a single player
        //      All pieces a blueprint will cover do not belong to another blueprint
        //      There is a piece on the tile the blueprint will be placed on
        bool isLegal = false;
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            if (!PiecesShareOwner(Services.MapManager.Map[tile.coord.x, tile.coord.y])) return false;
            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].PartOfStructure()) return false;
            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].IsOccupied())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return isLegal;
    }

    public override void MakePhysicalPiece(bool isViewable)
    {
        holder = new GameObject();
        holder.transform.SetParent(Services.GameScene.transform);
        holder.name = holderName;

        if (piece == null) return;
        tileRelativeCoords = new Dictionary<Coord, Tile>();

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile,
                        holder.transform);

                    Coord myCoord = new Coord(-2 + x, -2 + y);
                    newpiece.Init(myCoord, this);
                    tileRelativeCoords[myCoord] = newpiece;

                    string pieceName = newpiece.name.Replace("(Clone)", "");
                    newpiece.name = pieceName;

                    newpiece.ActivateTile(owner, buildingType);
                    tiles.Add(newpiece);
                }
            }
        }

        SetVisible(!isViewable);
    }

    public override void PlaceAtCurrentLocation()
    {
        placed = true;
        OnPlace();
        foreach (Tile tile in tiles)
        {
            Coord tileCoord = tile.coord;
            Services.MapManager.Map[tileCoord.x, tileCoord.y].SetOccupyingStructure(this);
        }
    }

    protected override void OnPlace()
    {
        switch (buildingType)
        {
            case BuildingType.FACTORY:
                owner.ToggleMineCount(1);
                break;
            case BuildingType.MINE:
                owner.ToggleFactoryCount(1);
                break;
            default:
                break;
        }
        
        //  If Factory increase player's playrate
        //  If Mine increase player's draw rate
    }

    public override void OnInputUp()
    {
        if (selected)
        {
            selected = false;
            if (!placed)
            {
                if (IsPlacementLegal())
                {
                    PlaceAtCurrentLocation();
                    owner.OnPiecePlaced();
                }
                else owner.CancelSelectedBlueprint();
            }
        }
    }
}
