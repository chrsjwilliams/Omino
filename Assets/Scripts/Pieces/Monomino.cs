using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monomino : Polyominoes
{
    protected override void Start()
    {
        variations = 1;
        width = 1;
        length = 1;
    }

    public override void GenerateTemplate()
    {
        piece = new int[1, 1, 1] 
        {   
            //  #
            {
                { 1}
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

        //  Instantiate Tile
    }
}
