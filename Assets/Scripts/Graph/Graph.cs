using System;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;


public interface IVertex
{
}
/*
public struct Edge<TVertex>
{
    public readonly List<List<TVertex>> vertices;

    public Edge(TVertex u, TVertex v)
    {
        vertices = new List<List<TVertex>> ();
        List<TVertex> first = new List<TVertex>();
        first.Add(u);

        List<TVertex> second = new List<TVertex>();
        second.Add(v);

        vertices.Add(first);
        vertices.Add(second);
    }

    public bool HasVertex(TVertex v)
    {
        foreach(List<TVertex> vertex in vertices)
        {
            if (vertex[0].Equals(v))
                return true;
        }
        return false;
    }
}
*/

public class Graph<TVertex> where TVertex : IVertex
{
    //  Using a Dictionary as an adjacency list.
    //  Each vertex contains a list of verticies its adjacent to
    public Dictionary<TVertex, List<TVertex>> Vertices { get; set; }
    public Graph()
    {
        Vertices = new Dictionary<TVertex, List<TVertex>>();
    }

    public void ApplyKrager()
    {
        var random = new System.Random();
        while (Vertices.Count > 2)
        {
            //  choose a random edge and extract its two verticies
            var randomIndex = random.Next(0, Vertices.Keys.Count);
            TVertex firstVertex = Vertices.Keys.ElementAt(randomIndex);
            TVertex secondVertex = Vertices[firstVertex].ElementAt(random.Next(0, Vertices[firstVertex].Count));

            if (Vertices.ContainsKey(secondVertex))
            {
                //Merge
                foreach (var edge in Vertices[secondVertex])
                {
                    //  For each edge connected to the second vertex,
                    //  connect the other end of the edge to the first vertex
                    if (!Vertices[firstVertex].Contains(edge))
                        Vertices[firstVertex].Add(edge);
                }

                //change all the occurences of the secondVertex to the first
                foreach (var vertex in Vertices)
                {
                    if (vertex.Value.Contains(secondVertex))
                    {
                        vertex.Value.Remove(secondVertex);
                        vertex.Value.Add(firstVertex);
                    }
                }
                //Remove Self Loops
                Vertices[firstVertex].RemoveAll(_ => _.Equals(firstVertex));
                Vertices.Remove(secondVertex);
            }
        }

    }

    /*
    public List<List<TVertex>> vertices { get; private set; }
    public List<Edge<TVertex>> edges { get; private set; }

    
    public Graph()
    {
        vertices = new List<List<TVertex>>();
        edges = new List<Edge<TVertex>>();
    }
    */
    /*
    public int GetSize() { return vertices.Count; }

    private void VertexNotInGraph(TVertex v)
    {
        Debug.Log("ERROR: Graph does not contain vertex: " + v);
    }

    public void AddVertex(TVertex v)
    {
        if (!HasVertex(v))
        {
            List<TVertex> vertex = new List<TVertex>();
            vertex.Add(v);
            vertices.Add(vertex);
        }
        else Debug.Log("Vertex " + v + " already in graph");
    }

    /// <summary>
    /// Use this function when connecting verticies
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    public void AddEgde(TVertex u, TVertex v)
    {
        Edge<TVertex> newEdge = new Edge<TVertex>(u, v);
        if(HasEdge(newEdge))
        {
            Debug.Log("Edge already in graph");
            return;
        }
        else
        {
            if (!HasVertex(u))
            {
                List<TVertex> vertex = new List<TVertex>();
                vertex.Add(u);
                vertices.Add(vertex);
            }
            if (!HasVertex(v))
            {
                List<TVertex> vertex = new List<TVertex>();
                vertex.Add(v);
                vertices.Add(vertex);
            }
            edges.Add(newEdge);
        }
    }

    public void AddEdge(Edge<TVertex> edge)
    {
        if (!HasEdge(edge))
        {
            edges.Add(edge);
        }
        else
        {
            Debug.Log("Edge " + edge + " is already in the graph");
        }
    }

    public void RemoveEdge(Edge<TVertex> edge)
    {
        if (!HasEdge(edge))
        {
            Debug.Log("Removing Edge that doesn't exist: " + edge);
            return;
        }
        else
        {
            edges.Remove(edge);
        }
    }

    public void RemoveVertex(TVertex v)
    {
        if(!HasVertex(v))
        {
            VertexNotInGraph(v);
        }
        else
        {
            foreach (Edge<TVertex> edge in edges)
            {
                if( edge.HasVertex(v))
                {
                    edges.Remove(edge);
                }
            }

            foreach (List<TVertex> vertex in vertices)
            {
                if(vertex[0].Equals(v))
                {
                    vertices.Remove(vertex);
                }
            }
        }
    }

    public List<TVertex> GetAdjacentVertcies(TVertex v)
    {
        if (!HasVertex(v))
        {
            VertexNotInGraph(v);
            return null;
        }
        else
        {
            List<TVertex> adjacentVerticies = new List<TVertex>();
            foreach(Edge<TVertex> edge in edges)
            {
                if (edge.vertices[0][0].Equals(v))
                {
                    adjacentVerticies.Add(edge.vertices[0][0]);

                }

                if (edge.vertices[1][0].Equals(v))
                {
                    adjacentVerticies.Add(edge.vertices[1][0]);

                }
            }
            return adjacentVerticies;
        }   
    }

    public List<Edge<TVertex>> GetEdges(TVertex v)
    {
        if (!HasVertex(v))
        {
            VertexNotInGraph(v);
            return null;
        }
        else
        {
            List<Edge<TVertex>> edgeList = new List<Edge<TVertex>>();
            foreach (Edge<TVertex> edge in edges)
            {
               if(  edge.HasVertex(v) &&
                    !edgeList.Contains(edge))
                {
                    edgeList.Add(edge);
                }
            }
            return edgeList;
        }
    }

    public IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        System.Random rand = new System.Random();
        List<TValue> values = Enumerable.ToList(dict.Values);
        int size = dict.Count;
        while (true)
        {
            yield return values[rand.Next(size)];
        }
    }

    public bool HasVertex(TVertex v)
    {
        foreach (List<TVertex> vertex in vertices)
        {
            if (vertex[0].Equals(v))
                return true;
        }
        return false;
    }

    public bool HasEdge(Edge<TVertex> edge)
    {
        return edges.Contains(edge);
    }

    public HashSet<TVertex> BreadthFirstSearch(Graph<TVertex> graph, TVertex start)
    {
        var visited = new HashSet<TVertex>();

        if (!graph.HasVertex(start))
            return visited;

        var queue = new Queue<TVertex>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();

            if (visited.Contains(vertex))
                continue;

            visited.Add(vertex);

            foreach (var neighbor in GetAdjacentVertcies(vertex))
            {
                if (!visited.Contains(neighbor))
                    queue.Enqueue(neighbor);
            }
        }

        return visited;
    }

    public static TVertex KragerAlgorithm(Graph<TVertex> g)
    {  
        System.Random rnd = new System.Random();
        int attempts = (int)(g.vertices.Count() * g.vertices.Count() * Mathf.Log(g.vertices.Count()));

        Graph<TVertex> bestCutGraph = g;
        int bestCutCount = g.edges.Count();

        Debug.Log("Making " + attempts + " attempts");

        for(int i = 0; i < attempts; i++)
        {
            //  make a copy of g for contracting
            Graph<TVertex> g2 = g;

            //  seed the RNG
            System.Random rng = new System.Random();
            Debug.Log("/r" + i);

            //use random contraction to reduce the graph to a cut
            while(g2.vertices.Count() > 2)
            {
                RandomContraction(g2);
            }

            if (g2.edges.Count < bestCutCount)
            {
                //  this is our best so far
                bestCutGraph = g2;
                bestCutCount = g2.edges.Count();
                Debug.Log("New best..." + bestCutCount);
            }
        }

        g = bestCutGraph;
        //  or remove the best cut count
        #region Method1
        while (g.vertices.Count > 2)
        {
            //  removes item at the index
            //Edge<TVertex> e = g.edges.remove(rnd.nextInt(gr.edges.size()));
            int randomIndex = rnd.Next(g.edges.Count);
            TVertex fistVertex = g.edges[randomIndex].vertices[0][0];
            TVertex secondVertex = g.edges[randomIndex].vertices[1][0];

            if (g.HasVertex(secondVertex))
            {
                foreach(Edge<TVertex> edge in g.GetEdges(secondVertex))
                {
                    if(!g.GetEdges(fistVertex).Contains(edge))
                    {
                        foreach(List<TVertex> entry in edge.vertices)
                        {
                            if(!entry[0].Equals(secondVertex))
                            {
                                Edge<TVertex> newEdge = new Edge<TVertex>(fistVertex, entry[0]);
                                g.AddEdge(newEdge);

                            }
                        }
                    }
                }

                foreach (KeyValuePair<TVertex, List<TVertex>> entry in g.vertices)
                {
                    if (entry.Key.Equals(secondVertex))
                    {
                        g.RemoveVertex(secondVertex);
                    }
                }
            }
        }
        #endregion         
    }

    private void RandomContraction(Graph<TVertex> graphCopy)
    {
        //  pick a random edge
        System.Random rnd = new System.Random();
        int randomIndex = rnd.Next(graphCopy.edges.Count);
        Edge<TVertex> selectedEdge = graphCopy.edges[randomIndex];

        //  now contract the vertices joined by this edge.  we'll merge in the edges
        //  from v2 into v1 and remove v2 from the graph.
        List<List<TVertex>> v1 = new List<List<TVertex>>();
        v1.Add(selectedEdge.vertices[0]);

        List<List<TVertex>> v2 = new List<List<TVertex>>();
        v2.Add(selectedEdge.vertices[1]);

        //  build a new list of edges based on the contracted graph
        List<Edge<TVertex>> newEdges = new List<Edge<TVertex>>();
        foreach (Edge<TVertex> edge in graphCopy.edges)
        {
            Edge<TVertex> newEdge = edge;

            if(newEdge.vertices[0].Equals(v2))
            {
                newEdge.vertices[0] = v1[0];
            }
            if(newEdge.vertices[1].Equals(v2))
            {
                newEdge.vertices[1] = v1[0];
            }

            //  only add it to the new list if it's not a self-loop
            if(!newEdge.vertices[0].Equals(newEdge.vertices[1]) && !newEdges.Contains(newEdge))
            {
                newEdges.Add(newEdge);
            }
            else
            {
                //  Self Loop
            }
        }

        graphCopy.edges = newEdges;

        // null out the vertex to signify that it's been squashed

        graphCopy.vertices.RemoveAt(graphCopy.vertices.Count() - 1);

    }
    */
}
