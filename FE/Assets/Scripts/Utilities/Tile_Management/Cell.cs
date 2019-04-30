using UnityEngine;
using QPathfinding;

public class Cell : MonoBehaviour, IQPathTile
{
    public enum TerrainType { Arena, Armory, Ballista, Bridge, Chest, Cliff, Dep_Forest, Desert, Door, Fence, Floor,
        Forest, Fort, Gate, Glacier, House, Killer_Ballista, Lake, Long_Ballista, Mountain, Peak, Pillar, Plain,
        River, Road, Ruins_Village, Sand, Sea, Stairs, Throne, Valley, Vendor, Village, Village_Closed, Wall,
        Wall_Weak, Wasteland }

    [System.Serializable]
    public class TerrainBonus
    {
        public int defense = 0;
        public int avoid = 0;
        [Range(0f, 1f)] public float heal = 0;

        public TerrainBonus()
        {
            defense = 0;
            avoid = 0;
            heal = 0;
        }
        public TerrainBonus(int defense, int avoid, float heal)
        {
            this.defense = defense;
            this.avoid = avoid;
            this.heal = heal;
        }
    }

    public Vector2Int position;
    public int layer = 0;
    public bool isIcon = false;

    public bool impassible = false;
    
    public TerrainType terrain;
    public TerrainBonus terrainBonus;

    public Fighter unitInTile;

    public int MoveCost(Unit u)
    {
        int cost = 1;

        if (unitInTile != null && unitInTile.Unit.alignment != u.alignment)
            cost++;

        return cost;
    }

    public void Display_Debug(string s)
    {
        TMPro.TextMeshProUGUI t = GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (t == null)
            return;

        t.text = s;
    }
    
    private void OnEnable()
    {
        position = new Vector2Int(Mathf.FloorToInt(transform.localPosition.x), Mathf.FloorToInt(transform.localPosition.y));
        transform.localPosition = (Vector2)position;
        Map.Add(this, layer, isIcon);
    }

    void Update()
    {
        //if (UnitController.Fighter == null)
        //    Display_Debug("");
    }

    public void SetColor(Color color)
    {
        GetComponentInChildren<SpriteRenderer>().color = color;
    }

    #region QPathfinding Implementation
    public IQPathTile[] GetNeighbors()
    {
        return Map.GetNeighbors(this);
    }

    public static float EstimateDistance(IQPathTile a, IQPathTile b)
    {
        Cell s = (Cell)a, e = (Cell)b;
        return Vector2.Distance(s.transform.position, e.transform.position);
    }

    public float AggregateCostToEnter(float costSoFar, IQPathTile sourceTile, IQPathUnit unit)
    {
        Fighter f = (Fighter)unit;
        return (f).AggregateTurnsToEnterTile(this, costSoFar) + MoveCost(f.Unit);
    }
    #endregion
}