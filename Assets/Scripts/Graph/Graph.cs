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
    //  Each vertex contains a list of verticies its adjacent to
    public List<Edge<TVertex>> Edges { get; set; }
    public List<TVertex> Vertices;
    public Dictionary<TVertex, List<Edge<TVertex>>> EdgeDict;
    public Graph()
    {
        Edges = new List<Edge<TVertex>>();
        Vertices = new List<TVertex>();
        EdgeDict = new Dictionary<TVertex, List<Edge<TVertex>>>();
    }

    public void ApplyKarger()
    {
        var random = new System.Random();
        while (Vertices.Count > 2)
        {
            //  choose a random edge and extract its two verticies
            var randomIndex = random.Next(0, Edges.Count);
            Edge<TVertex> randomEdge = Edges[randomIndex];
            TVertex firstVertex = randomEdge.curFirstVertex;
            TVertex secondVertex = randomEdge.curSecondVertex;
            //Merge
            Polyomino firstNode = firstVertex as Polyomino;
            Polyomino secondNode = secondVertex as Polyomino;
            Debug.Log("collapsing " + secondNode.centerCoord.x + "," + secondNode.centerCoord.y + " into " +
                firstNode.centerCoord.x + "," + firstNode.centerCoord.y);
            foreach (Edge<TVertex> edge in EdgeDict[secondVertex])
            {

                //change all the occurences of the secondVertex to the first
                TVertex otherVertex = default(TVertex);
                if (edge.curFirstVertex.Equals(secondVertex))
                {
                    edge.AssignVertex(firstVertex, 1);
                    otherVertex = edge.curSecondVertex;
                }
                else if (edge.curSecondVertex.Equals(secondVertex))
                {
                    edge.AssignVertex(firstVertex, 2);
                    otherVertex = edge.curFirstVertex;
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
                    bool addNewEdge = true;
                    foreach (Edge<TVertex> firstVertEdge in EdgeDict[firstVertex])
                    {
                        if (firstVertEdge.curFirstVertex.Equals(otherVertex) ||
                            firstVertEdge.curSecondVertex.Equals(otherVertex))
                        {
                            addNewEdge = false;
                            break;
                        }
                    }
                    if (addNewEdge) EdgeDict[firstVertex].Add(edge);
                }
            }

            Vertices.Remove(secondVertex);
            EdgeDict.Remove(secondVertex);
            Debug.Log(Vertices.Count + " vertices remaining:");
            foreach (TVertex vertex in EdgeDict.Keys)
            {
                Polyomino piece = vertex as Polyomino;
                Debug.Log("vertex at " + piece.centerCoord.x + "," + piece.centerCoord.y + " has edges:");
                foreach (Edge<TVertex> edge in EdgeDict[vertex])
                {
                    Polyomino origFirstEdgePiece = edge.originalFirstVertex as Polyomino;
                    Polyomino origSecondEdgePiece = edge.originalSecondVertex as Polyomino;
                    Polyomino firstEdgePiece = edge.curFirstVertex as Polyomino;
                    Polyomino secondEdgePiece = edge.curSecondVertex as Polyomino;
                    Debug.Log(firstEdgePiece.centerCoord.x + "," + firstEdgePiece.centerCoord.y + " to " +
                        secondEdgePiece.centerCoord.x + "," + secondEdgePiece.centerCoord.y);
                    Debug.Log("originally: " + origFirstEdgePiece.centerCoord.x + "," + origFirstEdgePiece.centerCoord.y + " to " +
                        origSecondEdgePiece.centerCoord.x + "," + origSecondEdgePiece.centerCoord.y);
                }
            }

        }
    }
}
