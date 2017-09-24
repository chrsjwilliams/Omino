using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triominoes : Polyominoes
{
    protected override void Start()
    {
        variations = 2;
        width = 3;
        length = 3;
    }

    public override void GenerateTemplate()
    {
        variations = 2;
        width = 3;
        length = 3;
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

    public override void InitTemplate()
    {

    }

    public override void Create(int index)
    {
        if (index < 0)
            index = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    //  Instantiate Tile
                }
            }
        }
    }
}
