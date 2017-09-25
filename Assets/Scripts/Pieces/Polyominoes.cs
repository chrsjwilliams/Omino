using System.Collections.Generic;
using UnityEngine;

public abstract class Polyominoes
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
}
