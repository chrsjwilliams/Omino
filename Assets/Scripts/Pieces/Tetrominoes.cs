using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetrominoes : Polyomino
{
    public Tetrominoes()
    {
        variations = 5;
        width = 4;
        length = 4;

        holder = new GameObject();
        string holderName = "TetrominoHolder";
        holder.name = holderName;

        GenerateTemplate();

        Services.GameEventManager.Register<PlacePieceEvent>(OnPlacePiece);
    }

    public override void GenerateTemplate()
    {
        piece = new int[5, 4, 4]
        { 
            //  ####
            {   
                {1,1,1,1 },
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }
            },
            //  #
            //  #
            //  ##
            {
                {1,0,0,0 },
                {1,0,0,0 },
                {1,0,0,0 },
                {1,1,0,0 }
            },
            //  #
            //  ##
            //  #
            {
                {1,0,0,0 },
                {1,1,0,0 },
                {1,0,0,0 },
                {0,0,0,0 }
            },
            //  ##
            //   ##
            {
                {1,1,0,0 },
                {0,1,1,0 },
                {0,0,0,0 },
                {0,0,0,0 }
            },
            //  ##
            //  ##
            {
                {1,1,0,0 },
                {1,1,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }
            }

        };
    }
}
