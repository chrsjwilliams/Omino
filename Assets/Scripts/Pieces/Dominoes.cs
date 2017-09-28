using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dominoes : Polyomino
{
    public Dominoes()
    {
        variations = 1;
        width = 1;
        length = 2;

        holder = new GameObject();
        string holderName = "DominoHolder";
        holder.name = holderName;

        GenerateTemplate();

        Services.GameEventManager.Register<PlacePieceEvent>(OnPlacePiece);
    }

    public override void GenerateTemplate()
    {
        piece = new int[1, 1, 2] 
        { 
            //  ##
            {
                { 1 ,  1}
            }
        };
    }
}
