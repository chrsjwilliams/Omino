using System;
using UnityEngine;

public class Monomino : Polyominoes
{
    public Monomino()
    {
        variations = 1;
        width = 1;
        length = 1;

        //  Holds the tiles in one place to keep the Unity Hierarchy clean
        holder = new GameObject();
        string holderName = "MonominoHolder";
        holder.name = holderName;

        //  Pieces aren't automatically placed when created
        Services.GameEventManager.Register<PlacePieceEvent>(OnPlacePiece);

        //  Sets up the array for piece variations
        GenerateTemplate();
    }

    public override void GenerateTemplate()
    {
        //  The numbers for the array are variations, width, and length respectively
        //  The reason why they aren't the variable is because the array needs const
        //  values to set itself up
        piece = new int[1, 1, 1] 
        {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 1}
            }
        };
    }
}
