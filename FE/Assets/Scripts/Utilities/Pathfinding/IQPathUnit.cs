namespace QPathfinding
{
    public interface IQPathUnit
    {
        float CostToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile);
    }
}