using UnityEngine;
using System.Collections.Generic;

public class AStarSearch
{
    public static List<Polyomino> ShortestPath(Polyomino start, Polyomino target)
    {
        List<Polyomino> path = new List<Polyomino>();
        Dictionary<Polyomino, Polyomino> cameFrom = 
            new Dictionary<Polyomino, Polyomino>();
        Dictionary<Polyomino, float> costSoFar = 
            new Dictionary<Polyomino, float>();

        PriorityQueue<Polyomino> frontier = new PriorityQueue<Polyomino>();
        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Polyomino current = frontier.Dequeue();
            if (current == target) break;
            foreach (Polyomino next in current.adjacentPieces)
            {
                float newCost;
                newCost = costSoFar[current] + 1;

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + Heuristic(next, target);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        Polyomino pathNode = target;
        while (pathNode != start)
        {
            path.Add(pathNode);
            pathNode = cameFrom[pathNode];
        }
        path.Add(start);

        return path;
    }

    private static float Heuristic(Polyomino a, Polyomino b)
    {
        float shortestDist = Mathf.Infinity;
        foreach (Tile tile in a.tiles)
        {
            foreach(Tile otherTile in b.tiles)
            {
                float dist = Vector3.Distance(tile.transform.position,
                    otherTile.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                }
            }
        }
        return shortestDist;
    }
}

public class PriorityQueue<T>
{
    public List<PrioritizedItem<T>> elements = new List<PrioritizedItem<T>>();

    public int Count { get { return elements.Count; } }

    public void Enqueue(T item, float priority)
    {
        elements.Add(new PrioritizedItem<T>(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority) bestIndex = i;
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}

public class PrioritizedItem<T>
{
    public T item;
    public float priority;
    public PrioritizedItem(T item_, float priority_)
    {
        item = item_;
        priority = priority_;
    }
}
