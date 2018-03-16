using System;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;


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
}

public class Graph<TVertex> where TVertex : IVertex
{
    //  Using a Dictionary as an adjacency list.
    //  Each vertex contains a list of vertices it's adjacent to
    public List<Edge<TVertex>> Edges { get; set; }
    public List<TVertex> Vertices;
    public Dictionary<TVertex, List<Edge<TVertex>>> EdgeDict;
    public Graph()
    {
        Edges = new List<Edge<TVertex>>();
        Vertices = new List<TVertex>();
        EdgeDict = new Dictionary<TVertex, List<Edge<TVertex>>>();
    }

    public Graph(Graph<TVertex> graphToCopy)
    {
        Edges = new List<Edge<TVertex>>(graphToCopy.Edges);
        Vertices = new List<TVertex>(graphToCopy.Vertices);
        EdgeDict = new Dictionary<TVertex, List<Edge<TVertex>>>(graphToCopy.EdgeDict);
    }

    public void ApplyKarger()
    {
        var random = new System.Random();
        if (Edges.Count == 0) return;
        while (Vertices.Count > 2)
        {
            //  choose a random edge and extract its two verticies
            var randomIndex = random.Next(0, Edges.Count);
            Edge<TVertex> randomEdge = Edges[randomIndex];
            TVertex firstVertex = randomEdge.curFirstVertex;
            TVertex secondVertex = randomEdge.curSecondVertex;
            //Merge
            foreach (Edge<TVertex> edge in EdgeDict[secondVertex])
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
    }
}
