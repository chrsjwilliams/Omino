using System.Collections.Generic;
using UnityEngine;

public abstract class Polyomino
{
    public List<Tile> tiles = new List<Tile>();
    public GameObject holder { get; protected set; }

    protected int variations;
    protected int width;
    protected int length;
    protected int[,,] piece;

    //  One version of the Polyominoes class could be 
    //  it takes in an int and churns out the
    //  corresponding polyomino

    //  public Polyominoes(int units) { //  other code; }

    public abstract void GenerateTemplate();

    public bool WithinBounds()
    {
        bool withinBounds = false;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (0 < tiles[i].coord.x && tiles[i].coord.x < Services.MapManager.MapWidth - 1 &&
               0 < tiles[i].coord.y && tiles[i].coord.y < Services.MapManager.MapLength - 1)
            {
                withinBounds = true;
            }
        }

        return withinBounds;
    }

    protected void OnPlacePiece(PlacePieceEvent e)
    {
        //  This stops the pieces from being connected to the player
        //  Check if all pieces are on the map

        if (WithinBounds())
        {
            if (e.player.playerNum == 1)
            {
                holder.transform.parent = Services.GameManager.ownedByPlayer1.transform;
            }
            else if (e.player.playerNum == 2)
            {
                holder.transform.parent = Services.GameManager.ownedByPlayer2.transform;
            }
        }
        else
        {
            //  Some sort of negative feedback
        }
    }

    public void Create(int index, Player player)
    {
        //  Index sanitizing. This can be moved up into the Player class
        if (index > 0 || index < 0)
            index = 0;

        //  Parents the polyomino to the player so the player
        //  can move around the board with the piece
        holder.transform.parent = player.cursor;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    //  A pieces is created like this to prevent ArrayOutOfBounds Exceptions 
                    //  This way the player cursor can move to any space on the map with any
                    //  any piece
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile);

                    Coord myCoord = new Coord(player.cursorPos.X + x, player.cursorPos.Y + y);
                    newpiece.Init(myCoord, player.playerNum);

                    string pieceName = newpiece.name.Replace("(Clone)", "") + "[X: " + x + ", Y: " + y + "]";
                    newpiece.name = pieceName;

                    newpiece.transform.parent = holder.transform;

                    newpiece.ActivateTile(player);
                    tiles.Add(newpiece);
                }
            }
        }
    }

    public void PlaceAtCurrentLocation()
    {
        //place the piece on the board where it's being hovered now
        OnPlace();
    }

    public virtual bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //CONDITIONS:
        //is contiguous with a structure connected to either the base or a fortification
        //doesn't overlap with any existing pieces or is a destructor
        return false;
    }

    protected virtual void OnPlace()
    {
        //do whatever special stuff this piece does when you place it 
        //(e.g. destroy overlapping pieces for a destructor)
    }

    public void Reposition(Vector3 pos)
    {
        //change localposition of the piece container in player UI to value
    }

    public void MakePhysicalPiece()
    {
        //create all the tile gameobjects for the physical representation of the piece
        //put them inside a container to move around
    }
    
}
