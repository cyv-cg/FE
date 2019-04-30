using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using QPathfinding;

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager instance;

    public static Action OnPhaseStart, OnPhaseEnd;
    public static Action OnTurnStart, OnTurnEnd;

    #region Variables

    public static bool TracingPath = false;

    public enum Phase { None, Player, Allied, Enemy, Neutral }

    public Phase currentPhase;
    private Phase lastPhase;

    public static int TurnNumber = 1;
    
    private static Cell _selected;
    public static Cell Selected { get { return _selected; } private set {
            _selected = value;

            if (Fighter != null && pathApplied)
            {
                instance.StartCoroutine(instance.OnSelectedChange());
            }
        }
    }

    private IEnumerator OnSelectedChange()
    {
        ClearDrawnPath();
        ClearHighlightedArea();

        yield return StartCoroutine(Fighter.ApplyPath(QPath.FindPath(Fighter, startCell, Selected, Cell.EstimateDistance), GameSettings.DoMoveAnimation));

        AttackArea = new Cell[0];
        StaffArea = new Cell[0];

        if (Fighter.Unit.inventory.GetWeapons(Fighter, true).Length > 0)
        {
            Weapon.AttackData atkData = Fighter.Unit.inventory.GetAttackData(Fighter);
            if (atkData != null)
                AttackArea = Map.GetExtendedArea(new Cell[] { Map.UnitTile(Fighter) }, atkData.range, atkData.closedSet, atkData.closedSetMin);
        }

        if (Fighter.Unit.inventory.GetStaves(Fighter, true).Length > 0)
        {
            Weapon.AttackData stfData = Fighter.Unit.inventory.GetStaffData(Fighter);
            if (stfData != null)
                StaffArea = Map.GetExtendedArea(new Cell[] { Map.UnitTile(Fighter) }, stfData.range, stfData.closedSet, stfData.closedSetMin);
        }

        CurrentRange = new List<Cell>
        {
            Map.GetCellData(Fighter.Position())
        };
        DoHighlights(Fighter, CurrentRange.ToArray(), false);

        ActionMenu.Open(Fighter);
        ActionMenu.ActionDone = false;
    }

    private static Fighter _fighter;
    public static Fighter Fighter { get { return _fighter; } private set {
            _fighter = value;

            if (value == null)
            {
                ClearDrawnPath();
                return;
            }

            MovementRemaining = value.Unit.stats.movement;
            CurrentRange = GetRange(value);
        }
    }

    private static List<Cell> GetRange(Fighter f)
    {
        List<Cell> _cells = new List<Cell>();
        Cell[] cells = Map.GetMoveArea(f, MovementRemaining, true);
        foreach (Cell c in cells)
        {
            _cells.Add(c);
        }

        return _cells;
    }

    public static Cell MouseToCell()
    {
        RaycastHit2D hit;
        Camera main = Camera.main;
        Cell cell;

        Ray ray = main.ScreenPointToRay(Input.mousePosition);
        hit = Physics2D.Raycast(main.ScreenToWorldPoint(Input.mousePosition), main.transform.forward);

        if (hit.transform != null && hit.transform.GetComponent<Cell>() != null)
        {
            cell = hit.transform.GetComponent<Cell>();
            return Map.GetCellData(cell.position);
        }

        return null;
    }
    
    private static Cell startCell;
    private static bool pathApplied;
    
    public static Cell CellUnderMouse;
    public static Cell LastCellUnderMouse;

    public static List<Cell> CurrentRange { get; private set; }
    public static Cell[] CurrentPath { get; private set; }

    public static Cell[] AttackArea { get; private set; }
    public static Cell[] StaffArea { get; private set; }

    public GameObject crosshairPrefab;
    public static GameObject Crosshair { get; private set; }

    private static List<Cell> arrow;
    public GameObject arrowPrefab;
    private const int arrowLayer = 1;
    
    private static List<Cell> highlight;
    public GameObject blueHighlightPrefab;
    public GameObject redHighlightPrefab;
    public GameObject greenHighlightPrefab;
    private const int highlightLayer = arrowLayer - 1;

    public static int MovementRemaining;
    public static int MovementUsed;

    IEnumerator AIPhase;

    #endregion

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        UnitManager.Init();

        Crosshair = Instantiate(crosshairPrefab);

        currentPhase = Phase.Player;
        NewTurn();
    }

    private void Update()
    {
        if (BattleManager.IsBattling)
            return;

        if (currentPhase == Phase.Player && !CameraController.IsDraggingCamera && !ItemMenu.IsOpen)
        {
            PlayerPhase();
        }
        else if (currentPhase != Phase.Player && AIPhase == null)
        {
            if (currentPhase == Phase.Allied)
            {

            }
            else if (currentPhase == Phase.Enemy)
            {
                AIPhase = EnemyPhase();
                StartCoroutine(AIPhase);
            }
            else if (currentPhase == Phase.Neutral)
            {

            }
        }
    }
    private void LateUpdate()
    {
        LastCellUnderMouse = MouseToCell();
    }

    void NewTurn()
    {
        OnTurnStart?.Invoke();

        Crosshair.SetActive(true);
    }
    void NextPhase()
    {
        StartCoroutine(_NextPhase());
    }
    IEnumerator _NextPhase()
    {
        while (BattleManager.IsBattling)
            yield return null;

        if (lastPhase == Phase.Player)
        {
            if (UnitManager.Allies.Count > 0)
                currentPhase = Phase.Allied;
            else if (UnitManager.Enemies.Count > 0)
                currentPhase = Phase.Enemy;
            else
            {
                currentPhase = Phase.Player;
                NewTurn();
            }
        }
        else if (lastPhase == Phase.Allied)
        {
            if (UnitManager.Enemies.Count > 0)
                currentPhase = Phase.Enemy;
            else
            {
                currentPhase = Phase.Player;
                NewTurn();
            }
        }
        else if (lastPhase == Phase.Enemy)
        {
            if (UnitManager.Neutrals.Count > 0)
                currentPhase = Phase.Neutral;
            else
            {
                currentPhase = Phase.Player;
                NewTurn();
            }
        }
        else if (lastPhase == Phase.Neutral)
        {
            currentPhase = Phase.Player;
            NewTurn();
        }

        AIPhase = null;
    }

    public static void EndPhase()
    {
        Crosshair.SetActive(false);
        instance.StartCoroutine(instance._EndPhase());
    }
    private IEnumerator _EndPhase()
    {
        lastPhase = currentPhase;
        currentPhase = Phase.None;

        yield return new WaitForSeconds(1);
        OnPhaseEnd?.Invoke();

        NextPhase();

        Debug.Log(currentPhase + " phase");

        OnPhaseStart?.Invoke();
    }

    public static void EndFighterTurn()
    {
        ClearHighlightedArea();
        Crosshair.SetActive(true);
        Fighter.EndTurn();
        Fighter = null;
        Selected = null;
        pathApplied = false;
    }

    public void Battle(Fighter attacker, Fighter defender, bool doAnimation)
    {
        BattleManager.Attack(attacker, defender, GameSettings.DoBattleAnimation);
        
        EndFighterTurn();
    }

    #region PlayerPhase
    void PlayerPhase()
    {
        CellUnderMouse = MouseToCell();

        if (CellUnderMouse != null && CellUnderMouse != LastCellUnderMouse && !ActionMenu.IsOpen)
            Crosshair.transform.position = (Vector2)CellUnderMouse.position;

        if (InputManager.LeftClickUp() && !CameraController.IsDraggingCamera)
            OnLeftClick();

        if (InputManager.RightClickUp() && !CameraController.IsDraggingCamera)
            OnRightClick();
         
        if (Fighter != null && CellUnderMouse != LastCellUnderMouse && Selected == Map.UnitTile(Fighter) && !pathApplied)
            DrawPath();

        if (LastCellUnderMouse != null && CellUnderMouse != null)
        {
            if (CellUnderMouse != LastCellUnderMouse && CellUnderMouse.unitInTile != null && !pathApplied && Fighter == null)
            {
                ClearHighlightedArea();

                Fighter tile = CellUnderMouse.unitInTile;
                Cell[] range = new Cell[0];

                if (tile.Unit.alignment == Unit.Alignment.Player)
                    range = Map.GetMoveArea(tile, true);
                else
                    range = tile.GetComponent<BaseAI>().GetMoveArea();

                DoHighlights(tile, tile.TurnOver ? new Cell[1] { CellUnderMouse } : range, true);
            }
            else if ((CellUnderMouse == null || CellUnderMouse.unitInTile == null) && Fighter == null)
                ClearHighlightedArea();
        }
    }

    void OnLeftClick()
    {
        if (ActionMenu.Selection == ActionMenu.SelectionState.closed)
        {
            if (Fighter == null)
            {
                Cell cell = MouseToCell();

                Selected = cell;
                startCell = Selected;

                if (cell != null && cell.unitInTile != null && cell.unitInTile.Unit.alignment == Unit.Alignment.Player && !cell.unitInTile.TurnOver)
                {
                    Fighter = Map.GetCellData(Selected.position).unitInTile;

                    //ClearHighlightedArea();
                    DoHighlights(Fighter, CurrentRange.ToArray(), true, 0.01f);
                }
            }
            else if (Fighter != null && CurrentRange.Contains(MouseToCell()))
            {
                pathApplied = true;
                Selected = MouseToCell();
                MovementUsed = Map.AggregateCost(QPath.FindPath(Fighter, startCell, Selected, Cell.EstimateDistance), Fighter.Unit);
            }
        }
        else if (ActionMenu.Selection == ActionMenu.SelectionState.target)
        {
            if (ActionMenu.CanSelect)
                SelectTarget();
            else
                ActionMenu.CanSelect = true;
        }
    }
    public static bool HighlightIsFaded { get; private set; }
    void OnRightClick()
    {
        if (Fighter == null)
            return;
        
        ClearHighlightedArea();
        Crosshair.transform.position = (Vector2)CellUnderMouse.position;
        
        if (!pathApplied)
        {
            if (MovementRemaining < Fighter.Unit.stats.movement)
                EndFighterTurn();

            Fighter f = CellUnderMouse.unitInTile;
            if (f != null)
                DoHighlights(f, Map.GetMoveArea(f, true), true, 0.01f);
            else
                ClearHighlightedArea();

            Selected = null;
            Fighter = null;
            ClearDrawnPath();
            //ClearHighlightedArea();
        }
        else if (pathApplied)
        {
            if (!TradeMenu.IsOpen)
            {
                ActionMenu.Close();
                Crosshair.SetActive(true);
                pathApplied = false;
            }

            ClearDrawnPath();
            ClearHighlightedArea();
            
            Selected.unitInTile = null;
            startCell.unitInTile = Fighter;
            
            if (!ActionMenu.ActionDone)
            {
                Selected = startCell;

                Fighter.transform.position = (Vector2)startCell.position;
            }
            else
            {
                startCell.unitInTile = null;
                startCell = Map.UnitTile(Fighter);
                startCell.unitInTile = Fighter;
            }
            
            if (!ActionMenu.IsOpen)
                CurrentRange = GetRange(Fighter);

            if (!TradeMenu.IsOpen)
                DrawPath();
            DoHighlights(Fighter, CurrentRange.ToArray());
        }
        
        if (ActionMenu.Selection == ActionMenu.SelectionState.target)
        {
            ActionMenu.Selection = ActionMenu.SelectionState.option;
        }
    }

    void SelectTarget()
    {
        ClearHighlightedArea();

        if (ActionMenu.Action == ActionMenu.ActionState.attack)
        {
            Fighter f = Map.GetCellData(Crosshair.transform.position).unitInTile;
            Battle(Fighter, f, GameSettings.DoBattleAnimation);
        }
        if (ActionMenu.Action == ActionMenu.ActionState.staff)
        {
            Fighter f = Map.GetCellData(Crosshair.transform.position).unitInTile;

            Debug.LogError("NYI");
        }
        if (ActionMenu.Action == ActionMenu.ActionState.trade)
        {
            Fighter f = Map.GetCellData(Crosshair.transform.position).unitInTile;
            TradeMenu.Open(Fighter, f);
        }
    }
    #endregion
    #region EnemyPhase

    IEnumerator EnemyPhase()
    {
        foreach (Fighter f in UnitManager.Enemies)
        {
            if (f == null)
                continue;

            BaseAI ai = f.GetComponent<BaseAI>();

            yield return ai.DoTurn();

            f.EndTurn();
        }
    }

    #endregion

    #region Highlights
    public void DrawPath()
    {
        Cell destination = null;

        if (CurrentRange.Contains(MouseToCell()))
        {
            if (arrow != null)
                ClearDrawnPath();
            arrow = new List<Cell>();

            destination = MouseToCell();
        }
        else
            return;

        CurrentPath = QPath.FindPath(Fighter, Map.UnitTile(Fighter), destination, Cell.EstimateDistance);

        if (CurrentPath.Length == 1)
            return;

        foreach (Cell c in CurrentPath)
        {
            GameObject a = null;

            if (!ObjectPool.ContainsKey("arrow") || ObjectPool.GetFirstInactiveObject("arrow") == null)
                a = Instantiate(arrowPrefab, new Vector3(c.position.x, c.position.y), Quaternion.identity);
            else
                a = ObjectPool.GetFirstInactiveObject("arrow");

            ArrowDisplay display = a.GetComponent<ArrowDisplay>();

            arrow.Add(display);
            display.layer = arrowLayer;
            Map.Add(display, arrowLayer, true);

            a.GetComponent<SpriteRenderer>().enabled = false;

            a.transform.position = new Vector3(c.position.x, c.position.y);
            a.SetActive(true);

            display.AssignSprite();
            a.GetComponent<SpriteRenderer>().enabled = true;

            ObjectPool.AddToPool("arrow", a);
        }
    }
    static void ClearDrawnPath()
    {
        if (CurrentPath == null || CurrentPath.Length == 0)
            return;

        for (int i = arrow.Count - 1; i >= 0; i--)
        {
            if (arrow[i] == null)
                continue;

            arrow[i].gameObject.SetActive(false);
            Map.Remove(arrow[i], arrowLayer, true);
        }

        arrow = new List<Cell>();
        Map.ClearMap(arrowLayer, true);

        CurrentPath = new Cell[0];
    }

    public void DoHighlights(Fighter f, Cell[] range, bool doWalkable = true, float delay = 0)
    {
        StartCoroutine(_DoHighlights(f, range, doWalkable, delay));
    }

    IEnumerator _DoHighlights(Fighter f, Cell[] range, bool doWalkable = true, float delay = 0)
    {
        if (delay > 0)
            HighlightIsFaded = true;

        yield return new WaitForSeconds(delay);
        HighlightIsFaded = false;

        ClearHighlightedArea();

        if (doWalkable)
            HighlightWalkableArea(range);

        if (f.Weapon == null)
            yield return 0;

        HighlightAttackArea(f, range);
        HighlightStaffArea(f, range);
    }

    void HighlightWalkableArea(Cell[] range)
    {
        if (highlight == null)
            highlight = new List<Cell>();

        foreach (Cell c in range)
            Highlight(c, "blue");
    }
    void HighlightAttackArea(Fighter f, Cell[] range)
    {
        Weapon.AttackData data = f.Unit.inventory.GetAttackData(f);
        if (data == null)
        {
            //Debug.LogWarning("Weapon not found");
            return;
        }

        List<Cell> _last = new List<Cell>();
        foreach (Cell c in range)
            _last.Add(c);

        AttackArea = Map.GetExtendedArea(range, data.range, data.closedSet, data.closedSetMin);
        
        if (highlight == null)
            highlight = new List<Cell>();
        
        foreach (Cell c in AttackArea)
        {
            if (highlight.Contains(c) || _last.Contains(c))
                continue;

            Fighter u = c.unitInTile;

            if (u == null)
            {
                Highlight(c, "red");
                continue;
            }

            if (f.Unit.alignment == u.Unit.alignment)
                Highlight(c, "blue");
            else
                Highlight(c, "red");
        }
    }
    void HighlightStaffArea(Fighter f, Cell[] range)
    {
        Weapon.AttackData data = f.Unit.inventory.GetStaffData(f);
        if (data == null)
        {
            //Debug.LogWarning("Staff not found");
            return;
        }

        List<Cell> _last = new List<Cell>();
        foreach (Cell c in AttackArea)
            _last.Add(c);

        StaffArea = Map.GetExtendedArea(range, data.range, data.closedSet, data.closedSetMin);

        if (highlight == null)
            highlight = new List<Cell>();

        foreach (Cell c in StaffArea)
        {
            if (_last.Contains(c))
                continue;
            
            Highlight(c, "green");
        }
    }

    void Highlight(Cell c, string color = "blue")
    {
        GameObject h = null;

        GameObject prefab = blueHighlightPrefab;
        if (color == "red")
            prefab = redHighlightPrefab;
        else if (color == "green")
            prefab = greenHighlightPrefab;

        if (!ObjectPool.ContainsKey("highlights_" + color) || ObjectPool.GetFirstInactiveObject("highlights_" + color) == null)
            h = Instantiate(prefab, new Vector3(c.position.x, c.position.y), Quaternion.identity);
        else
            h = ObjectPool.GetFirstInactiveObject("highlights_" + color);

        Cell cell = h.GetComponent<Cell>();
        cell.layer = highlightLayer;
        highlight.Add(cell);
        Map.Add(h.GetComponent<TileHighlight>(), highlightLayer, true);

        h.transform.position = new Vector3(c.position.x, c.position.y);
        h.SetActive(true);

        ObjectPool.AddToPool("highlights_" + color, h);
    }

    static void ClearHighlightedArea()
    {
        AttackArea = StaffArea = new Cell[0];

        if (highlight == null)
            return;

        for (int i = highlight.Count - 1; i >= 0; i--)
        {
            if (highlight[i] == null)
                continue;
         
            highlight[i].gameObject.SetActive(false);
            Map.Remove(highlight[i], highlightLayer, true);
        }

        highlight = new List<Cell>();
        Map.ClearMap(highlightLayer, true);
    }
    #endregion
}