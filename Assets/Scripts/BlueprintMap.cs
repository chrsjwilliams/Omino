using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintMap
{
    public Blueprint blueprint;
    public HashSet<Coord> missingCoords;
    public Coord targetCoord;
    public int rotations;

    public BlueprintMap()
    {
        blueprint = null;
        missingCoords = null;
        targetCoord = new Coord();
        rotations = 0;
    }

    public BlueprintMap(Blueprint _blueprint, HashSet<Coord> _missingCoords, Coord _coord, int _rotations)
    {
        blueprint = _blueprint;
        missingCoords = _missingCoords;
        targetCoord = _coord;
        rotations = _rotations;
    }
}
