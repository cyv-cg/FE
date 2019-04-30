namespace QPathfinding
{ 
    public interface IQPathTile
    {
        IQPathTile[] GetNeighbors();

        float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit unit);
    }
}