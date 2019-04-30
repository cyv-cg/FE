using System.Collections;
using System.Collections.Generic;
using QPathfinding;

public class GuardAI : AttackBaseAI
{
    public override IEnumerator DoTurn()
    {
        List<Cell> cells = new List<Cell>();
        foreach (Cell c in MoveArea)
            cells.Add(c);
        foreach (Cell c in AttackArea)
            cells.Add(c);

        FindTarget(cells.ToArray());

        if (Target != null)
        {
            ChooseWeapon();

            yield return Fighter.ApplyPath(FindPath(), GameSettings.DoMoveAnimation);
            TargetData();

            if (Target != null && AttackArea.Contains(Map.UnitTile(Target)))
            {
                yield return new UnityEngine.WaitForSeconds(1);
                Attack();
            }
        }
    }

    Cell[] FindPath()
    {
        Cell[] tilesWhereUnitCanAttackFrom = Map.GetExtendedArea(new Cell[1] { Map.UnitTile(Target) }, AtkData);
        List<Cell> attackArea = new List<Cell>();
        foreach (Cell c in tilesWhereUnitCanAttackFrom)
            attackArea.Add(c);

        Cell[] tilesWhereTargetCanCounterFrom = Map.GetExtendedArea(new Cell[1] { Map.UnitTile(Target) }, Target.Unit.inventory.GetAttackData(Target));
        List<Cell> counterArea = new List<Cell>();
        foreach (Cell c in tilesWhereTargetCanCounterFrom)
            counterArea.Add(c);

        List<Cell> preferenceArea = new List<Cell>();
        foreach (Cell c in attackArea)
            if (!counterArea.Contains(c))
                preferenceArea.Add(c);

        List<Cell> area = preferenceArea.Count == 0 ? attackArea : preferenceArea;

        Cell targetTile = area[UnityEngine.Random.Range(0, tilesWhereUnitCanAttackFrom.Length)];

        foreach (Cell c in tilesWhereUnitCanAttackFrom)
        {
            if (c == Map.UnitTile(Fighter))
                targetTile = Map.UnitTile(Fighter);
        }
        
        List<Cell> range = new List<Cell>();
        Cell[] path = QPath.FindPath(Fighter, Map.UnitTile(Fighter), targetTile, Cell.EstimateDistance);

        foreach (Cell c in path)
            if (MoveArea.Contains(c) && area.Contains(c))
                range.Add(c);

        return range.ToArray();
    }
}