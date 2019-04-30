using UnityEngine;

namespace QPathfinding
{
    public static class QPath
    {
        public static T[] FindPath<T>(IQPathUnit unit, T startTile, T endTile, CostEstimateDelegate costEstimateFunc) where T : IQPathTile
        {
            if (unit == null || startTile == null || endTile == null)
            {
                Debug.LogError("null values passed to QPath::FindPath");
                return null;
            }

            QPath_AStar<T> resolver = new QPath_AStar<T>(unit, startTile, endTile, costEstimateFunc);
            resolver.DoWork();

            return resolver.GetList();
        }
    }

    public delegate float CostEstimateDelegate(IQPathTile a, IQPathTile b);
}