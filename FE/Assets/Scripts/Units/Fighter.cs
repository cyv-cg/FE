using UnityEngine;
using System.Collections;
using QPathfinding;

[System.Serializable]
public class Fighter : MonoBehaviour, IQPathUnit
{
    public TextAsset unitFile;
    public Unit Unit { get; private set; }

    private int _currentHP;
    public int CurrentHP { get { return _currentHP; } set {
            _currentHP = Mathf.Clamp(value, 0, Unit.stats.hp);
        }
    }

    public Weapon Weapon { get; private set; }
    public Weapon Staff { get; private set; }

    public Fighter Rescued { get; set; }

    [HideInInspector] public Fighter LastTarget;
    [HideInInspector] public int RepeatedTarget;

    public bool TurnOver { get; private set; }

    private Material mat;
    
    void StartTurn()
    {
        TurnOver = false;
        mat.SetFloat("_EffectAmount", 0);
    }
    public void EndTurn()
    {
        if (Unit.alignment == Unit.Alignment.Player)
            ActionMenu.Close();

        TurnOver = true;
        mat.SetFloat("_EffectAmount", 1);

        UnitManager.OnUnitEnd(this);
    }
    
    private void OnEnable()
    {
        PhaseManager.OnTurnStart += StartTurn;
        PhaseManager.OnPhaseEnd += () => { mat.SetFloat("_EffectAmount", 0); };
    }
    private void OnDisable()
    {
        PhaseManager.OnTurnStart -= StartTurn;
        PhaseManager.OnPhaseEnd -= () => { mat.SetFloat("_EffectAmount", 0); };
    }

    private void OnValidate()
    {
        if (Unit == null)
            Unit = DataManager.FileToUnit(unitFile, true);

        gameObject.name = Unit.name;
    }

    private void Awake()
    {
        LoadFromFile();

        mat = GetComponentInChildren<SpriteRenderer>().material;
    }
    private void Start()
    {
        Cell cell = Map.GetCellData(transform.position);
        cell.unitInTile = this;
        CurrentHP = Unit.stats.hp;

        Equip();
        //if (Unit.inventory.GetWeapons(true, this).Length > 0)
        //    Weapon = Unit.inventory.GetWeapons(true, this)[0];
    }

    void LoadFromFile()
    {
        Unit = DataManager.FileToUnit(unitFile, true);
    }

    public Vector2Int Position()
    {
        if (this == null || transform == null)
            return new Vector2Int();

        return new Vector2Int(Mathf.FloorToInt(
            transform.position.x),
            Mathf.FloorToInt(transform.position.y)
        );
    }

    public IEnumerator ApplyPath(Cell[] path, bool doAnimation)
    {
        PhaseManager.TracingPath = true;
        path[0].unitInTile = null;

        if (!doAnimation)
        {
            transform.position = (Vector2)path[path.Length - 1].position;
            PhaseManager.TracingPath = false;
        }
        else
        {
            float speed = 1f / Unit.stats.movement;

            System.Collections.Generic.Queue<Cell> queue = new System.Collections.Generic.Queue<Cell>();
            foreach (Cell c in path)
                queue.Enqueue(c);

            Cell next = path[0];

            while (next != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, (Vector2)next.position, speed);

                if (Vector2.Distance(transform.position, next.position) <= 0.01f)
                {
                    if (queue.Count > 0)
                        next = queue.Dequeue();
                    else if (queue.Count == 0)
                        next = null;
                }

                yield return null;
            }

            transform.position = (Vector2)path[path.Length - 1].position;
            PhaseManager.TracingPath = false;
        }

        path[path.Length - 1].unitInTile = this;
        yield return null;
    }

    public void Equip(Weapon w)
    {
        if (w.type != Weapon.WeaponType.Staff)
            Weapon = w;
        else
            Staff = w;

        //Debug.Log(gameObject.name + " equipped " + w.name);
    }
    public void Equip()
    {
        Weapon[] weapons = Unit.inventory.GetWeapons(true, this, false);
        if (weapons == null || weapons.Length == 0)
        {
            //Debug.Log(gameObject.name + " has no weapons");
            return;
        }

        Weapon w = weapons[0];

        if (w != null)
            Equip(w);
    }
    public void Unequip()
    {
        Weapon = null;
    }
    
    public void Damage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, Unit.stats.hp);
    }

    public void AddEXP(int value)
    {
        if (this != null)
            StartCoroutine(_AddEXP(value));
    }
    private IEnumerator _AddEXP(int value)
    {
        int total = Unit.TotalEXP + value;

        while (Unit.TotalEXP != total)
        {
            Unit.Exp++;
            Unit.ExpToNextLevel--;

            if (Unit.ExpToNextLevel <= 0)
                yield return _LevelUp();

            yield return null;
        }
    }

    public void LevelUp()
    {
        StartCoroutine(_LevelUp());
    }
    private IEnumerator _LevelUp()
    {
        Unit.Exp = 0;
        Unit.stats.level++;
        Unit.ExpToNextLevel = StatsCalc.ExperienceToNextLevel(Unit.stats.level);

        yield return new WaitForSeconds(1f);
    }

    public void Heal(Unit target)
    {
        throw new System.NotImplementedException();
    }

    #region QPathfinding Implementation
    public float CostToEnterTile(IQPathTile sourceTile, IQPathTile destinationTile)
    {
        return 1;
    }

    public float AggregateTurnsToEnterTile(Cell cell, float turnsToDate)
    {
        float baseTurnsToEnterTile = cell.MoveCost(Unit) / Unit.stats.movement;

        if (baseTurnsToEnterTile <= 0)
        {
            return -99999;
        }

        if (baseTurnsToEnterTile > 1)
        {
            baseTurnsToEnterTile = 1;
        }

        float turnsToDateWhole = Mathf.Floor(turnsToDate);
        float turnsToDateFraction = turnsToDate - turnsToDateWhole;

        if ((turnsToDateFraction > 0 && turnsToDateFraction < 0.01f) || turnsToDateFraction > 0.99f)
        {
            if (turnsToDateFraction < 0.01f)
                turnsToDateFraction = 0;

            if (turnsToDateFraction > 0.99f)
            {
                turnsToDateWhole++;
                turnsToDateFraction = 0;
            }
        }

        float turnsUsedAfterThisMove = turnsToDateFraction + baseTurnsToEnterTile;

        if (turnsUsedAfterThisMove > 1)
        {
            if (turnsToDateFraction == 0)
            {

            }
            else
            {
                turnsToDateWhole++;
                turnsToDateFraction = 0;
            }
        }

        return turnsToDateWhole + turnsUsedAfterThisMove;
    }
    #endregion
}