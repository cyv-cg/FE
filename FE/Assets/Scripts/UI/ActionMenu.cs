using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ActionMenu : MonoBehaviour
{
    public static ActionMenu instance;

    public GameObject menu, optionsMenu, buttonPrefab;

    public bool TradeThroughItemMenu { get; set; }

    public static bool IsOpen { get; private set; }
    public static bool CanSelect { get; set; }

    public enum SelectionState { closed, option, target }
    private static SelectionState _state;
    public static SelectionState Selection { get { return _state; } set {
            _state = value;

            if (value == SelectionState.option)
            {
                PhaseManager.Crosshair.SetActive(false);
            }
            else if (value == SelectionState.target)
            {
                Target = Targets[0];
                PhaseManager.Crosshair.SetActive(true);
            }
        }
    }

    public enum ActionState { none, attack, staff, support, rescue, release, recruit, trade, storage, item}
    public static ActionState Action = ActionState.none;

    public static int SupportPointThreshold = 6;

    public static List<Fighter> Targets { get; set; }
    public static Fighter Target { get; set; }
    public static Fighter Source { get; private set; }

    public static bool ActionDone = false;

    #region Checks
    private bool CanAttack(Fighter f)
    {
        if (f.Unit.inventory.GetWeapons(f, true).Length == 0 || PhaseManager.AttackArea == null || PhaseManager.AttackArea.Length == 0)
            return false;

        foreach (Cell c in PhaseManager.AttackArea)
        {
            Unit u = null;
            if (c.unitInTile != null)
                 u = c.unitInTile.Unit;

            if (u == null)
                continue;

            if (f.Unit.inventory.GetAttackData(f) != null && f.Unit.inventory.GetAttackData(f).targets.HasFlag(u.alignment))
                return true;
        }

        return false;
    }
    private bool CanStaff(Fighter f)
    {
        if (f.Unit.inventory.GetStaves(f, true).Length == 0 || PhaseManager.StaffArea == null || PhaseManager.StaffArea.Length == 0)
            return false;
        
        foreach (Cell c in PhaseManager.StaffArea)
        {
            Unit u = null;
            if (c.unitInTile != null)
                u = c.unitInTile.Unit;

            if (u == null)
                continue;
            
            if (f.Unit.inventory.GetStaffData(f) != null && f.Unit.inventory.GetStaffData(f).targets.HasFlag(u.alignment))
                return true;
        }

        return false;
    }

    private bool CanItem(Fighter f)
    {
        return f.Unit.inventory.Items != null && f.Unit.inventory.Items.Count > 0;
    }

    private bool CanSupport(Fighter f)
    {
        foreach (Cell c in Map.GetNeighbors(Map.UnitTile(f)))
        {
            Unit u = null;
            if (c.unitInTile != null)
                u = c.unitInTile.Unit;

            if (u == null || u.alignment != Unit.Alignment.Player)
                continue;

            if (f.Unit.SupportPoints != null && f.Unit.SupportPoints.Count > 0 && f.Unit.SupportPoints[u] >= SupportPointThreshold)
                return true;
        }

        return false;
    }

    private bool CanRescue(Fighter f)
    {
        if (f.Rescued != null)
            return false;

        foreach (Cell c in Map.GetNeighbors(Map.UnitTile(f)))
        {
            Unit u = null;
            if (c.unitInTile != null)
                u = c.unitInTile.Unit;

            if (u == null || u.alignment != Unit.Alignment.Player)
                continue;

            if (u.stats.constitution <= f.Unit.stats.aid)
                return true;
        }

        return false;
    }

    private bool CanRelease(Fighter f)
    {
        return f.Rescued != null;
    }

    private bool CanStorage(Fighter f)
    {
        foreach (Cell c in Map.GetNeighbors(Map.UnitTile(f)))
        {
            Unit u = null;
            if (c.unitInTile != null)
                u = c.unitInTile.Unit;

            if (u == null)
                continue;

            if (u.unitClass == Unit.UnitClass.Transporter)
                return true;
        }

        return false;
    }

    private bool CanTrade(Fighter f)
    {
        foreach (Cell c in Map.GetNeighbors(Map.UnitTile(f)))
        {
            Unit u = null;
            if (c.unitInTile != null)
                u = c.unitInTile.Unit;

            if (u == null)
                continue;

            if (u.alignment == Unit.Alignment.Player)
                return true;
        }

        return false;
    }

    private bool CanRecruit(Fighter f)
    {
        foreach (Cell c in Map.GetNeighbors(Map.UnitTile(f)))
        {
            Unit u = null;
            if (c.unitInTile != null)
                u = c.unitInTile.Unit;

            if (u == null)
                continue;

            if (u.alignment != Unit.Alignment.Player && f.Unit.relationships.ContainsKey(u))
                return true;
        }

        return false;
    }
    #endregion

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Selection != SelectionState.target)
            return;

        Cell cell = PhaseManager.MouseToCell();
        if (cell == null)
            return;

        Fighter t = cell.unitInTile;
        if (t != null && Targets.Contains(t))
            Target = t;

        PhaseManager.Crosshair.transform.position = (Vector2)Target.Position();
    }

    public static void Open(Fighter f)
    {
        if (instance.TradeThroughItemMenu)
            return;

        Selection = SelectionState.option;

        instance.menu.SetActive(true);
        
        instance.ClearButtons();
        instance.SpawnButtons(f);
        instance.optionsMenu.GetComponentInChildren<UIHighlight>().Selectable.Select();

        Source = f;
        instance.Init(f);
        IsOpen = true;
    }
    public static void Close()
    {
        instance.menu.SetActive(false);
        Source = Target = null;
        Targets = new List<Fighter>();

        instance.ClearButtons();

        Selection = SelectionState.closed;
        Action = ActionState.none;

        IsOpen = false;
    }

    void SpawnButtons(Fighter f)
    {
        int height = 75;
        
        if (instance.CanAttack(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Attack";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Attack(Source); });
        }
        if (instance.CanStaff(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Staff";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Staff(Source); });
        }
        if (instance.CanSupport(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Support";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Support(); });
        }
        if (instance.CanRescue(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Rescue";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Rescue(); });
        }
        if (CanRelease(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Release";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Release(); });
        }
        if (CanItem(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Item";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Item(f); });
        }
        if (instance.CanStorage(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Storage";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Storage(); });
        }
        if (instance.CanTrade(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Trade";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Trade(f); });
        }
        if (instance.CanRecruit(f))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Recruit";
            g.GetComponent<Button>().onClick.AddListener(() => { CanSelect = false; Recruit(); });
        }
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, instance.optionsMenu.transform);
            g.GetComponent<TextMeshProUGUI>().text = "End";
            g.GetComponent<Button>().onClick.AddListener(() => { End(); });
        }

        instance.GetComponent<RectTransform>().sizeDelta = new Vector2(180, height);
    }
    void ClearButtons()
    {
        for (int i = optionsMenu.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(optionsMenu.transform.GetChild(i).gameObject);
        }
    }

    void Init(Fighter f)
    {

    }

    #region Actions
    public static void BaseAction(bool applyAction = true)
    {
        ActionDone = applyAction;
        if (applyAction)
            PhaseManager.MovementRemaining -= PhaseManager.MovementUsed;
    }

    public void Attack(Fighter fighter)
    {
        BaseAction(false);

        Targets = new List<Fighter>();

        foreach (Cell c in PhaseManager.AttackArea)
        {
            Fighter f = c.unitInTile;
            if (f != null && fighter.Unit.inventory.GetAttackData(fighter).targets.HasFlag(f.Unit.alignment))
                Targets.Add(f);
        }

        Selection = SelectionState.target;
        Action = ActionState.attack;
    }
    public void Staff(Fighter fighter)
    {
        BaseAction(false);

        Targets = new List<Fighter>();

        foreach (Cell c in PhaseManager.StaffArea)
        {
            Fighter f = c.unitInTile;
            if (f != null && fighter.Unit.inventory.GetStaffData(fighter).targets.HasFlag(f.Unit.alignment))
                Targets.Add(f);
        }

        Selection = SelectionState.target;
        Action = ActionState.staff;
    }

    public void Support()
    {
        BaseAction();

        // subtract support points from unit
        Action = ActionState.support;
    }

    public void Rescue()
    {
        BaseAction();

        Action = ActionState.rescue;
    }

    public void Release()
    {
        BaseAction();

        Action = ActionState.release;
    }

    public void Item(Fighter f)
    {
        BaseAction();

        Action = ActionState.item;
        ItemMenu.Open(f);
    }

    public void Storage()
    {
        BaseAction();

        Action = ActionState.storage;
    }

    public void Trade(Fighter fighter, bool throughItemMenu = false)
    {
        TradeThroughItemMenu = throughItemMenu;
        BaseAction(false);

        Targets = new List<Fighter>();

        foreach (Cell c in Map.GetNeighbors(Map.UnitTile(fighter)))
        {
            Fighter f = c.unitInTile;
            if (f != null && f.Unit.alignment == Unit.Alignment.Player)
                Targets.Add(f);
        }

        Selection = SelectionState.target;

        Action = ActionState.trade;
    }

    public void Recruit()
    {
        BaseAction();

        Action = ActionState.recruit;
    }

    public static void End()
    {
        PhaseManager.EndFighterTurn();
        Close();
    }
    #endregion
}