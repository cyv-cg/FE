using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseAI : MonoBehaviour
{
    [EnumFlag(4)] public Unit.Alignment targets = (Unit.Alignment)3;

    protected Fighter Fighter { get; private set; }

    protected List<Cell> MoveArea { get; private set; }
    protected List<Cell> AttackArea { get; private set; }
    protected List<Cell> StaffArea { get; private set; }

    protected Weapon.AttackData AtkData, StfData;

    public virtual IEnumerator DoTurn()
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        Fighter = GetComponent<Fighter>();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void OnEnable()
    {
        PhaseManager.OnPhaseStart += OnPhaseStart;
    }
    protected virtual void OnDisable()
    {
        PhaseManager.OnPhaseStart -= OnPhaseStart;
    }

    protected virtual void OnPhaseStart()
    {
        SetMoveArea(Map.GetMoveArea(Fighter, true));
        TargetData();
    }

    public virtual Cell[] GetMoveArea()
    {
        return Map.GetMoveArea(Fighter, true);
    }
    protected void SetMoveArea(Cell[] area)
    {
        MoveArea = new List<Cell>();

        foreach (Cell c in area)
            MoveArea.Add(c);
    }

    protected void TargetData()
    {
        AttackArea = new List<Cell>();
        StaffArea = new List<Cell>();

        GetAtkData();
        GetStfData();
    }

    void GetAtkData()
    {
        AtkData = Fighter.Unit.inventory.GetAttackData(Fighter);

        if (AtkData != null)
            foreach (Cell c in Map.GetExtendedArea(MoveArea.ToArray(), AtkData))
                AttackArea.Add(c);
    }
    void GetStfData()
    {
        StfData = Fighter.Unit.inventory.GetStaffData(Fighter);

        if (StfData != null)
            foreach (Cell c in Map.GetExtendedArea(MoveArea.ToArray(), StfData))
                StaffArea.Add(c);
    }
}