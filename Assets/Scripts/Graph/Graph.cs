using System;
using System.Collections.Generic;


public interface IVertex
{
}

public class Edge<TVertex> where TVertex : IVertex
{
    public TVertex originalFirstVertex;
    public TVertex originalSecondVertex;
    public TVertex curFirstVertex;
    public TVertex curSecondVertex;

    public Edge(TVertex first, TVertex second)
    {
        curFirstVertex = first;
        originalFirstVertex = first;
        curSecondVertex = second;
        originalSecondVertex = second;
    }

    public void AssignVertex(TVertex vertex, int num)
    {
        if (num == 1) curFirstVertex = vertex;
        else curSecondVertex = vertex;
    }

    public bool OriginalsEquivalent(Edge<TVertex> otherEdge)
    {
        return (originalFirstVertex.Equals(otherEdge.originalFirstVertex) ||
            originalFirstVertex.Equals(otherEdge.originalSecondVertex)) &&
            (originalSecondVertex.Equals(otherEdge.originalFirstVertex) ||
            originalSecondVertex.Equals(otherEdge.originalSecondVertex));
    }
}

public class Cut
{
    public HashSet<Edge<Polyomino>> edges;
    public int size;

    public Cut (HashSet<Edge<Polyomino>> edges_, int size_)
    {
        edges = edges_;
        size = size_;
    }

    public bool IsEquivalentTo(Cut otherCut)
    {
        foreach(Edge<Polyomino> edge in edges)
        {
            bool hasEquivalent = false;
            foreach (Edge<Polyomino> otherEdge in otherCut.edges)
            {
                if (otherEdge.OriginalsEquivalent(edge))
                {
                    hasEquivalent = true;
                    break;
                }
            }
            if (!hasEquivalent) return false;
        }
        foreach (Edge<Polyomino> otherEdge in otherCut.edges)
        {
            bool hasEquivalent = false;
            foreach (Edge<Polyomino> edge in edges)
            {
                if (edge.OriginalsEquivalent(otherEdge))
                {
                    hasEquivalent = true;
                    break;
                }
            }
            if (!hasEquivalent) return false;
        }
        return true;
    }
}

public class CutCoordSet
{
    public HashSet<Coord> coords;
    public int size;

    public CutCoordSet(HashSet<Coord> coords_, int size_)
    {
        coords = coords_;
        size = size_;
    }
    
    public CutCoordSet()
    {
        coords = new HashSet<Coord>();
        size = 0;
    }
}

public class Graph
{
    //  Using a Dictionary as an adjacency list.
    //  Each vertex contains a list of vertices it's adjacent to
    public List<Edge<Polyomino>> Edges { get; set; }
    public List<Polyomino> Vertices;
    public Dictionary<Polyomino, List<Edge<Polyomino>>> EdgeDict;
    public Graph()
    {
        Edges = new List<Edge<Polyomino>>();
        Vertices = new List<Polyomino>();
        EdgeDict = new Dictionary<Polyomino, List<Edge<Polyomino>>>();
    }

    // Adding edges error
    public Graph(Graph graphToCopy)
    {
        Edges = new List<Edge<Polyomino>>(graphToCopy.Edges);
        Vertices = new List<Polyomino>(graphToCopy.Vertices);
        EdgeDict = new Dictionary<Polyomino, List<Edge<Polyomino>>>();
        foreach(Edge<Polyomino> edge in Edges)
        {
            if (!EdgeDict.ContainsKey(edge.curFirstVertex))
            {
                EdgeDict[edge.curFirstVertex] = new List<Edge<Polyomino>>();
            }
            EdgeDict[edge.curFirstVertex].Add(edge);
            if (!EdgeDict.ContainsKey(edge.curSecondVertex))
            {
                EdgeDict[edge.curSecondVertex] = new List<Edge<Polyomino>>();
            }
            EdgeDict[edge.curSecondVertex].Add(edge);

        }
    }

    public virtual int ApplyKarger()
    {
        return ApplyKarger(null);
    }

    public virtual int ApplyKarger(MapTile tile)
    {
        var random = new System.Random();
        if (Edges.Count == 0)
        {
            return 0;
        }
        Dictionary<Polyomino, int> collapsedSizes = new Dictionary<Polyomino, int>();
        Dictionary<Polyomino, bool> containsMainBase = new Dictionary<Polyomino, bool>();
        Dictionary<Polyomino, bool> containsTargetTile = new Dictionary<Polyomino, bool>();
        foreach(Polyomino vertex in Vertices)
        {
            collapsedSizes[vertex] = 1;
            containsMainBase[vertex] = false;
            containsTargetTile[vertex] = false;

            if(tile != null && vertex.centerCoord.Equals(tile.coord))
            {
                containsTargetTile[vertex] = true;
            }

            if (vertex is Base)
            {
                Base baseVertex = vertex as Base;
                if (baseVertex.mainBase) containsMainBase[vertex] = true;
            }
        }
        while (Vertices.Count > 2)
        {
            //  choose a random edge and extract its two verticies
            var randomIndex = random.Next(0, Edges.Count);
            Edge<Polyomino> randomEdge;

            try
            {
                randomEdge = Edges[randomIndex];
            }catch (ArgumentOutOfRangeException outOfRange)
            {
                UnityEngine.Debug.Log("Exception caught");
                UnityEngine.Debug.Log("Edge Count: " + Edges.Count);
                UnityEngine.Debug.Log("Rand Index: " + randomIndex);
                return 0;
            }
            Polyomino firstVertex = randomEdge.curFirstVertex;
            Polyomino secondVertex = randomEdge.curSecondVertex;
            collapsedSizes[firstVertex] += collapsedSizes[secondVertex];
            if (containsMainBase[secondVertex])
                containsMainBase[firstVertex] = true;

            if (containsTargetTile[secondVertex])
                containsTargetTile[firstVertex] = true;
            //Merge
            foreach (Edge<Polyomino> edge in EdgeDict[secondVertex])
            {
                //change all the occurences of the secondVertex to the first
                if (edge.curFirstVertex.Equals(secondVertex))
                {
                    edge.AssignVertex(firstVertex, 1);
                }
                else if (edge.curSecondVertex.Equals(secondVertex))
                {
                    edge.AssignVertex(firstVertex, 2);
                }
                if (edge.curFirstVertex.Equals(edge.curSecondVertex)) //check for self loop
                {
                    Edges.Remove(edge);
                    EdgeDict[firstVertex].Remove(edge);
                }
                else
                //  For each edge connected to the second vertex,
                //  connect the other end of the edge to the first vertex
                {
                    EdgeDict[firstVertex].Add(edge);
                }
            }

            Vertices.Remove(secondVertex);
            EdgeDict.Remove(secondVertex);
        }

        foreach(Polyomino vertex in Vertices)
        {
            if (tile != null)
            {
                if (!containsTargetTile[vertex])
                    return collapsedSizes[vertex];
            }
            else
            {
                if (!containsMainBase[vertex]) return collapsedSizes[vertex];
            }
        }
        return 0;
    }
}
