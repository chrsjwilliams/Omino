using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Destructor : Polyomino
{
    public List<Polyomino> piecesInRange { get; private set; }
    public Destructor(int _units, int _index, Player _player) 
        : base(_units, _index, _player)
    {
        piecesInRange = new List<Polyomino>();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        iconSr.sprite = owner.splashDamage ?
                Services.UIManager.splashIcon : Services.UIManager.destructorIcon;
    }

    public override bool IsPlacementLegal(List<Polyomino> adjacentPieces)
    {
        bool connectedToBase = false;
        for (int i = 0; i < adjacentPieces.Count; i++)
        {
            if (adjacentPieces[i].connected || adjacentPieces[i] is Structure)
            {
                connectedToBase = true;
                break;
            }
        }
        if (!connectedToBase) return false;

        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.owner == owner) return false;
            if (mapTile.IsOccupied() && mapTile.occupyingPiece is Structure) return false;
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.shieldDurationRemaining > 0) return false;
        }
        
        return true;
    }

    protected override List<Tile> GetIllegalTiles()
    {
        List<Tile> illegalTiles = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord))
            {
                illegalTiles.Add(tile);
            }
            else
            {
                Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                bool occupied = mapTile.IsOccupied();
                if ((occupied && mapTile.occupyingPiece.owner == owner) ||
                    (occupied && mapTile.occupyingPiece is Structure) ||
                    (occupied && mapTile.occupyingPiece.shieldDurationRemaining > 0))
                {
                    illegalTiles.Add(tile);
                }
            }
        }
        return illegalTiles;
    }

    private bool LegalNotCountingShield()
    {
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            if (!Services.MapManager.ConnectedToBase(this, new List<Polyomino>())) return false;
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.owner == owner) return false;
            if (mapTile.IsOccupied() && mapTile.occupyingPiece is Structure) return false;
        }

        return true;
    }

    public bool StoppedByShield()
    {
        return !IsPlacementLegal() && LegalNotCountingShield();
    }

    protected override void OnPlace()
    {
        base.OnPlace();
        iconSr.enabled = false;
        List<Polyomino> piecesToRemove = GetPiecesInRange();
        if (piecesToRemove.Count > 0)
            Services.AudioManager.CreateTempAudio(Services.Clips.PieceDestroyed, 1);
        for (int i = piecesToRemove.Count - 1; i >= 0; i--)
        {
            piecesToRemove[i].Remove();
        }
        foreach(Tile tile in tiles)
        {
            tile.StartSettlingToNormalPiece();
        }
        //MakeFireBurst();
    }

    void MakeFireBurst()
    {
        List<Vector3> burstLocations = new List<Vector3>();
        foreach (Tile tile in tiles)
        {
            if (!burstLocations.Contains(tile.transform.position))
            {
                burstLocations.Add(tile.transform.position);
            }
            if (owner.splashDamage)
            {
                foreach (Coord dir in Coord.Directions())
                {
                    Coord adjCoord = tile.coord.Add(dir);
                    Vector3 adjPos = new Vector3(adjCoord.x, adjCoord.y, 0);
                    if (!burstLocations.Contains(adjPos))
                    {
                        burstLocations.Add(adjPos);
                    }
                }
            }
        }
        foreach (Vector3 pos in burstLocations)
        {
            GameObject.Instantiate(Services.Prefabs.FireBurst, pos, Quaternion.identity);
        }
    }

    protected override void CleanUpUI()
    {
        base.CleanUpUI();
        UnhighlightTargetedPieces();
    }

    void UnhighlightTargetedPieces()
    {
        for (int i = 0; i < piecesInRange.Count; i++)
        {
            if (!piecesInRange[i].dead)
            {
                piecesInRange[i].TurnOffGlow();
            }
        }
    }

    public override void SetLegalityGlowStatus()
    {
        base.SetLegalityGlowStatus();
        UnhighlightTargetedPieces();
        if (IsPlacementLegal())
        {
            piecesInRange = GetPiecesInRange();
            for (int i = 0; i < piecesInRange.Count; i++)
            {
                piecesInRange[i].SetGlow(Color.magenta);
            }
        }
    }

    public List<Polyomino> GetPiecesInRange()
    {
        List<Polyomino> enemyPiecesInRange = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.occupyingPiece != null && mapTile.occupyingPiece.owner != owner &&
                !enemyPiecesInRange.Contains(mapTile.occupyingPiece))
            {
                enemyPiecesInRange.Add(mapTile.occupyingPiece);
            }
        }
        if (owner.splashDamage)
        {
            List<Polyomino> adjacentEnemyPieces =
                GetAdjacentPolyominos(Services.GameManager.Players[owner.playerNum % 2]);
            for (int i = 0; i < adjacentEnemyPieces.Count; i++)
            {
                Polyomino enemyPiece = adjacentEnemyPieces[i];
                if (!enemyPiecesInRange.Contains(enemyPiece) && !(enemyPiece is Structure)
                    && enemyPiece.shieldDurationRemaining <= 0)
                {
                    enemyPiecesInRange.Add(enemyPiece);
                }
            }
        }
        return enemyPiecesInRange;
    }

    public override void MakePhysicalPiece()
    {
        base.MakePhysicalPiece();
        Services.GameEventManager.Register<SplashDamageStatusChange>(OnSplashDamageStatusChange);
    }

    public override void DestroyThis()
    {
        Services.GameEventManager.Unregister<SplashDamageStatusChange>(OnSplashDamageStatusChange);
        base.DestroyThis();
    }

    private void OnSplashDamageStatusChange(SplashDamageStatusChange e)
    {
        if (e.player == owner)
        {
            iconSr.sprite = owner.splashDamage ?
                Services.UIManager.splashIcon : Services.UIManager.destructorIcon;
        }
    }

    protected override void AssignLocation(Coord coord)
    {
        base.AssignLocation(coord);
        if (owner.splashDamage) Remove();
        iconSr.enabled = false;

    }

    protected override Polyomino CreateSubPiece()
    {
        Destructor destructorMonomino = new Destructor(1, 0, owner);
        destructorMonomino.MakePhysicalPiece();
        return destructorMonomino;
    }
}
