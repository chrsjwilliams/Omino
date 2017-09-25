﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pentominoes : Polyominoes
{
    public Pentominoes()
    {
        variations = 12;
        width = 5;
        length = 5;

        holder = new GameObject();
        string holderName = "PentominoHolder";
        holder.name = holderName;

        GenerateTemplate();

        Services.GameEventManager.Register<PlacePieceEvent>(OnPlacePiece);

    }

    public override void GenerateTemplate()
    {
        piece = new int[12, 5, 5]
        { 
            //  F Shape
            //   ##
            //  ##
            //   #
            {
                {0,1,1,0,0 },
                {1,1,0,0,0 },
                {0,1,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  I Shape
            //  #
            //  #
            //  #
            //  #
            //  #
            {
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 }
            },
            //  L Shape
            //  #
            //  #
            //  #
            //  ##
            {
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,1,0,0,0 },
                {0,0,0,0,0 }
            },
            //  N Shape
            //  ###
            //    ##
            {
                {1,1,1,0,0 },
                {0,0,1,1,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  P Shape
            //  ##
            //  ##
            //  #
            {
                {1,1,0,0,0 },
                {1,1,0,0,0 },
                {1,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  T Shape
            //  ###
            //   #
            //   #
            {
                {1,1,1,0,0 },
                {0,1,0,0,0 },
                {0,1,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  U Shape
            //  # #
            //  ###
            {
                {1,0,1,0,0 },
                {1,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  V Shape
            //  #
            //  #
            //  ###
            {
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  W Shape
            //  #
            //  ##
            //   ##
            {
                {1,0,0,0,0 },
                {1,1,0,0,0 },
                {0,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  X Shape
            //   #
            //  ###
            //   #
            {
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  Y Shape
            //   #
            //  ##
            //   #
            //   #
            {
                {0,1,0,0,0 },
                {1,1,0,0,0 },
                {0,1,0,0,0 },
                {0,1,0,0,0 },
                {0,0,0,0,0 }
            },
            //  Z Shape
            //  ##
            //   #
            //   ##
            {
                {1,1,0,0,0 },
                {0,1,0,0,0 },
                {0,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            }

        };
    }
}
