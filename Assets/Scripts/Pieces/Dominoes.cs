using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dominoes : Polyominoes
{
    protected override void Start()
    {
        variations = 1;
        width = 1;
        length = 2;
    }

    public override void GenerateTemplate()
    {
        variations = 1;
        width = 1;
        length = 2;
        piece = new int[1, 1, 2] 
        { 
            //  ##
            {
                { 1 ,  1}
            }
        };
    }

    public override void InitTemplate()
    {

    }

    public override void Create(int index)
    {
        if (index > 0 || index < 0)
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
