using UnityEngine;
using System.Collections.Generic;
using QPathfinding;

public static class Map
{
    public static List<Dictionary<Vector2Int, IQPathTile>> Layers { get; private set; }
    public static List<Dictionary<Vector2Int, IQPathTile>> IconLayers { get; private set; }

    public static void Add(Cell cell, int layer, bool icon = false)
    {
        if (!icon)
        {
            if (Layers == null)
                Layers = new List<Dictionary<Vector2Int, IQPathTile>>();

            layer = Mathf.Clamp(layer, 0, Layers.Count + 1);
        
            if (Layers.Count <= layer)
            {
                for (int i = 0; i <= layer; i++)
                {
                    Layers.Add(new Dictionary<Vector2Int, IQPathTile>());
                }
            }

            if (Layers.Count < layer)
            {
                Debug.LogError("Layers does not contain layer: " + layer);
                return;
            }

            if (Layers[layer].ContainsKey(cell.position))
            {
                Debug.LogError("Layers[" + cell.position + "] already exists.");
                return;
            }

            Layers[layer].Add(cell.position, cell);
        }
        else
        {
            if (IconLayers == null)
                IconLayers = new List<Dictionary<Vector2Int, IQPathTile>>();

            layer = Mathf.Clamp(layer, 0, IconLayers.Count + 1);

            if (IconLayers.Count <= layer)
            {
                for (int i = 0; i <= layer; i++)
                {
                    IconLayers.Add(new Dictionary<Vector2Int, IQPathTile>());
                }
            }

            if (IconLayers.Count < layer)
            {
                Debug.LogError("IconLayers does not contain layer: " + layer);
                return;
            }
            
            if (IconLayers[layer].ContainsKey(cell.position))
            {
                //Debug.LogError("IconLayers[" + cell.position + "] already exists: " + IconLayers[layer][cell.position].ToString() + "\n" + 
                //    "Cannot add: " + cell + ", on layer " + layer);

                IconLayers[layer][cell.position] = cell;

                return;
            }

            IconLayers[layer].Add(cell.position, cell);
        }
    }
    public static Cell Remove(Cell cell, int layer, bool icon = false)
    {
        if (!icon)
        {
            layer = Mathf.Clamp(layer, 0, Layers.Count + 1);

            if (Layers[layer].ContainsValue(cell))
            {
                Cell val = (Cell)Layers[layer][cell.position];
                Layers[layer].Remove(val.position);
                return val;
            }
        }
        else
        {
            layer = Mathf.Clamp(layer, 0, IconLayers.Count + 1);

            if (IconLayers[layer].ContainsKey(cell.position) && IconLayers[layer].ContainsValue(cell))
            {
                Cell val = (Cell)IconLayers[layer][cell.position];
                IconLayers[layer].Remove(val.position);
                return val;
            }
        }

        return null;
    }

    public static void ClearMap(int layer, bool icon = false)
    {
        if (!icon)
        {
            Layers[layer] = new Dictionary<Vector2Int, IQPathTile>();
        }
        else if (icon && IconLayers != null && IconLayers.Count >= layer + 1)
        {
            IconLayers[layer] = new Dictionary<Vector2Int, IQPathTile>();
        }
    }

    public static Cell GetCellData(Vector2 position)
    {
        Vector2Int pos = new Vector2Int(
            Mathf.FloorToInt(position.x),
            Mathf.FloorToInt(position.y)
        );

        return GetCellData(pos);
    }
    public static Cell GetCellData(Vector2Int position)
    {
        IQPathTile tile = null;
        
        for (int i = Layers.Count - 1; i >= 0; i--)
        {
            if (Layers[i].ContainsKey(position))
            {
                tile = Layers[i][position];
                break;
            }
            else
                continue;
        }

        return (Cell)tile;
    }

    public static Cell UnitTile(Fighter f)
    {
        return GetCellData(f.Position());
    }
    public static Cell UnitTile(Unit u)
    {
        foreach (Dictionary<Vector2Int, IQPathTile> d in Layers)
        {
            foreach (Vector2Int i in d.Keys)
            {
                if (GetCellData(i).unitInTile != null && GetCellData(i).unitInTile.Unit.name == u.name)
                    return GetCellData(i);
            }
        }

        return null;
    }

    public static Cell[] GetExtendedArea(Cell[] moveArea, Weapon.AttackData data)
    {
        return GetExtendedArea(moveArea, data.range, data.closedSet, data.closedSetMin);
    }
    public static Cell[] GetExtendedArea(Cell[] moveArea, int range, bool closedSet = true, int closedSetMin = 1)
    {
        List<Cell> cells = new List<Cell>();
        Dictionary<int, List<Cell>> layers = new Dictionary<int, List<Cell>>
        {
            [0] = new List<Cell>()
        };

        List<Cell> _moveArea = new List<Cell>();
        foreach (Cell c in moveArea)
            _moveArea.Add(c);

        foreach (Cell c in _moveArea)
            foreach (Cell a in GetNeighbors(c))
                if (!cells.Contains(a) && !_moveArea.Contains(a))
                {
                    cells.Add(a);
                    layers[0].Add(a);
                }

        for (int i = 1; i < range; i++)
        {
            layers[i] = new List<Cell>();

            foreach (Cell c in layers[i - 1])
                foreach (Cell a in GetNeighbors(c))
                {
                    if (!cells.Contains(a) && !_moveArea.Contains(a))
                    {
                        cells.Add(a);
                        layers[i].Add(a);
                    }
                }
        }

        if (closedSet)
        {
            List<Cell> set = new List<Cell>();

            for (int i = closedSetMin - 1; i < layers.Count; i++)
            {
                foreach (Cell c in layers[i])
                    set.Add(c);
            }

            return set.ToArray();
        }

        return layers[layers.Count - 1].ToArray();
    }

    public static int DistBtwn(Cell start, Cell end, Fighter f)
    {
        Cell[] path = QPath.FindPath(f, start, end, (IQPathTile a, IQPathTile b) => { return 1; });
        return path.Length;
    }

    #region QPathfinding
    public static Cell[] GetNeighbors(Cell c)
    {
        Vector2Int cellPosition = c.position;
        List<Cell> cells = new List<Cell>();

        IQPathTile[] tiles = new IQPathTile[4]
        {
                GetCellData(cellPosition + Vector2Int.up),
                GetCellData(cellPosition + Vector2Int.right),
                GetCellData(cellPosition + Vector2Int.down),
                GetCellData(cellPosition + Vector2Int.left)
        };

        foreach (Cell t in tiles)
        {
            if (t != null)
                cells.Add(t);
        }

        return cells.ToArray();
    }

    public static Cell[] GetMoveArea(Fighter f, int movement, bool weighted = true)
    {
        Dictionary<Cell, int> depths = GetTileDepths(f, movement + 1);
        Cell[] tiles = new Cell[depths.Count];
        depths.Keys.CopyTo(tiles, 0);

        if (weighted)
        {
            IQPathTile startCell = UnitTile(f);
            List<Cell> range = new List<Cell>
            {
                (Cell)startCell
            };

            foreach (Cell c in tiles)
            {
                Fighter unitInTile = c.unitInTile;

                IQPathTile[] path = QPath.FindPath(f, startCell, c, Cell.EstimateDistance);
                if (!c.impassible && AggregateCost(path, f.Unit) <= movement && c.MoveCost(f.Unit) >= 0 && (unitInTile == null || unitInTile.Unit.alignment == f.Unit.alignment))
                {
                    range.Add(c);
                    c.Display_Debug(AggregateCost(path, f.Unit).ToString());
                }
            }

            List<Cell> all = range;
            range = new List<Cell>();
            foreach (Cell c in all)
                if (c.unitInTile == null || c.unitInTile == f)
                    range.Add(c);

            return range.ToArray();
        }

        return tiles;
    }
    public static Cell[] GetMoveArea(Fighter f, bool weighted = true)
    {
        return GetMoveArea(f, f.Unit.stats.movement, weighted);
    }
    public static Dictionary<Cell, int> GetTileDepths(Fighter f, int range)
    {
        int movement = range;
        Vector3 worldPosition = f.transform.position;

        List<Cell> cells = new List<Cell> { (Cell)UnitTile(f) };

        int depth = 0;
        List<List<IQPathTile>> layers = new List<List<IQPathTile>> { new List<IQPathTile>() };
        List<int> thresholds = new List<int>();
        for (int i = 0; i <= movement + 1; i++)
        {
            thresholds.Add(n(i));
        }

        Dictionary<Cell, int> depths = new Dictionary<Cell, int>();
        
        for (int i = 0; i < n(movement - 1); i++)
        {
            if (cells[i] == null)
                continue;

            Vector2Int cellPosition = cells[i].position;

            if (thresholds.Contains(i))
            {
                depth++;
                layers.Add(new List<IQPathTile>());
            }

            IQPathTile[] _cells = GetNeighbors(cells[i]);

            layers[depth].Add(GetCellData(cellPosition));

            foreach (Cell c in _cells)
            {
                if (c != null && !cells.Contains(c))
                {
                    cells.Add(c);
                    depths.Add(c, depth);
                }
            }
        }
        return depths;
    }

    private static int n(int x)
    {
        return 4 * x + 2 * (x - 1) * ((x - 1) + 1) + 1;
    }
    public static int AggregateCost(IQPathTile[] path, Unit unit)
    {
        if (path == null || path.Length == 0)
            return 0;

        int cost = 0;
        foreach (IQPathTile i in path)
        {
            if (i == path[0])
                continue;

            Cell c = (Cell)i;
            cost += c.MoveCost(unit);
        }
        return cost;
    }
    #endregion
}