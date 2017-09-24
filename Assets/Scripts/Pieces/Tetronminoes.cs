using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetronminoes : Polyominoes
{
    protected override void Start()
    {
        variations = 5;
        width = 4;
        length = 4;
    }

    public override void GenerateTemplate()
    {
        //piece = new int[variations, width, length];
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
