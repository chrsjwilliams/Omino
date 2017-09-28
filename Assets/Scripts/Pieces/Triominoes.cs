using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triominoes : Polyomino
{
    public Triominoes()
    {
        variations = 2;
        width = 3;
        length = 3;

        holder = new GameObject();
        string holderName = "TriominoHolder";
        holder.name = holderName;

        GenerateTemplate();

        Services.GameEventManager.Register<PlacePieceEvent>(OnPlacePiece);
    }

    public override void GenerateTemplate()
    {
        piece = new int[2, 3, 3] 
        { 
            //  ###
            {
                {1,1,1 },
                {0,0,0 },
                {0,0,0 }
            },
            //  #
            //  ##
            {

                {1,0,0 },
                {1,1,0 },
                {0,0,0 }
            }
        };
    }
}
