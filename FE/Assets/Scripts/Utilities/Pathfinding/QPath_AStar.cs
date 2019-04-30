using System.Collections.Generic;
using System.Linq;

namespace QPathfinding
{
    public class QPath_AStar <T> where T : IQPathTile
    {
        public QPath_AStar (IQPathUnit unit, T startTile, T endTile, CostEstimateDelegate costEstimateFunc) 
        {
            this.unit = unit;
            this.startTile = startTile;
            this.endTile = endTile;

            this.costEstimateFunc = costEstimateFunc;
        }
        
        private readonly IQPathUnit unit;
        private readonly T startTile;
        private readonly T endTile;
        private readonly CostEstimateDelegate costEstimateFunc;

        private Queue<T> path;

        public void DoWork()
        {
            path = new Queue<T>();

            HashSet<T> closedSet = new HashSet<T>();

            PathfindingPriorityQueue<T> openSet = new PathfindingPriorityQueue<T>();
            openSet.Enqueue(startTile, 0);

            Dictionary<T, T> came_from = new Dictionary<T, T>();

            Dictionary<T, float> g_score = new Dictionary<T, float>
            {
                { startTile, 0}
            };

            Dictionary<T, float> f_score = new Dictionary<T, float>
            {
                {startTile, costEstimateFunc(startTile, endTile) }
            };

            while (openSet.Count > 0)
            {
                T current = openSet.Dequeue();

                if (ReferenceEquals(current, endTile))
                {
                    Reconstruct_path(came_from, current);
                    return;
                }

                closedSet.Add(current);

                foreach (T edge_neighbor in current.GetNeighbors())
                {
                    T neighbor = edge_neighbor;

                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    
                    float total_pathfinding_cost_to_neighbor = neighbor.AggregateCostToEnter(g_score[current], current, unit);

                    float tentative_g_score = total_pathfinding_cost_to_neighbor;

                    if (openSet.Contains(neighbor) && tentative_g_score >= g_score[neighbor])
                    {
                        continue;
                    }

                    came_from[neighbor] = current;
                    g_score[neighbor] = tentative_g_score;
                    f_score[neighbor] = g_score[neighbor] + costEstimateFunc(neighbor, endTile);

                    openSet.EnqueueOrUpdate(neighbor, f_score[neighbor]);
                }
            }
        }

        public void Reconstruct_path(Dictionary<T, T> came_from, T current)
        {
            Queue<T> total_path = new Queue<T>();
            total_path.Enqueue(current);

            while (came_from.ContainsKey(current))
            {
                current = came_from[current];
                total_path.Enqueue(current);
            }

            path = new Queue<T>(total_path.Reverse());
        }
        
        public T[] GetList()
        {
            return path.ToArray();
        }
    }
}