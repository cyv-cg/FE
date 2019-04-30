using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemMenu : MonoBehaviour
{
    public static ItemMenu instance;

    public static Fighter Fighter;

    public static bool IsOpen { get; private set; }
    private bool ActionMenuOpen { get; set; }
    private bool ThrowDialogueOpen { get; set; }

    private bool isTrading;

    public GameObject menu, itemList, itemButtonPrefab, itemActionMenu, itemActionList, buttonPrefab, throwDialogue;
    public TextMeshProUGUI type, atk, acc, crt, eva;

    public GameObject _atkAr, _accAr, _crtAr, _evaAr;
    private Image atkAr, accAr, crtAr, evaAr;

    private Color inc = new Color(0.5f, 1f, 0.6f), dec = new Color(1f, 0.5f, 0.25f);

    int baseAtk;
    float baseAcc, baseCrt, baseEva;

    private Item hovered;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        atkAr = _atkAr.GetComponentInChildren<Image>();
        accAr = _accAr.GetComponentInChildren<Image>();
        crtAr = _crtAr.GetComponentInChildren<Image>();
        evaAr = _evaAr.GetComponentInChildren<Image>();
    }

    private void Update()
    {
        if (!IsOpen && !isTrading)
            return;

        if (Fighter != null)
            StatusMenu(Fighter);

        if (InputManager.RightClickUp())
        {
            if (!isTrading)
            {
                if (IsOpen && !ActionMenuOpen)
                    Close();
                else if (ActionMenuOpen && !ThrowDialogueOpen)
                    CloseActionMenu();
            }
            else
            {
                ActionMenu.Selection = ActionMenu.SelectionState.closed;
                ActionMenu.Action = ActionMenu.ActionState.none;
                ActionMenu.instance.TradeThroughItemMenu = false;
                isTrading = false;
                Open(Fighter);
            }
        }
    }

    public static void Open(Fighter f)
    {
        if (IsOpen)
            return;

        PhaseManager.Crosshair.SetActive(false);

        instance.SetBaseStats(f);

        IsOpen = true;

        ActionMenu.Close();
        instance.menu.SetActive(true);

        foreach (Item i in f.Unit.inventory.Items)
        {
            instance.SpawnButton(i, f);
        }

        instance.gameObject.GetComponentInChildren<UIHighlight>().Selectable.Select();

        Fighter = f;
    }
    public static void Close()
    {
        instance.ClearList();
        instance.menu.SetActive(false);

        IsOpen = false;

        ActionMenu.Open(PhaseManager.Fighter);

        Fighter = null;
    }

    void ResetMenu()
    {
        Fighter f = Fighter;
        Close();
        Open(f);
    }

    void SpawnButton(Item i, Fighter f)
    {
        GameObject go = Instantiate(itemButtonPrefab, itemList.transform);
        ItemButton btn = go.GetComponent<ItemButton>();
        
        go.GetComponent<UIHighlight>().OnSelectAction += () => { hovered = btn.GetItem(); };
        go.GetComponent<Button>().onClick.AddListener( () => { OpenActionMenu(); });

        if (i.IsUseable(f))
        {
            btn.label.color = Color.white;
        }
        else
            btn.label.color = Color.gray;

        btn.SetItem(i);
    }

    void ClearList()
    {
        for (int i = itemList.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(itemList.transform.GetChild(i).gameObject);
        }
    }

    void SetBaseStats(Fighter f)
    {
        Weapon w = f.Weapon;

        bool magic = false;

        if (w != null)
            magic = w.type == Weapon.WeaponType.Anima_Tome || w.type == Weapon.WeaponType.Dark_Tome || w.type == Weapon.WeaponType.Light_Tome;

        int str = 0;
        float evade = 0;
        float crit = 0;
        float acc = 0;

        if (w != null)
        {
            if (!magic)
                str = StatsCalc.PhysicalAttack(f.Unit.stats.strength, w.might, 1, 1, StatsCalc.SupportBonus(f), false);
            else
                str = StatsCalc.MagicalAttack(f.Unit.stats.magic, w.might, 1, 1, StatsCalc.SupportBonus(f));

            evade = StatsCalc.Avoid(f.Unit.stats.speed, f.Unit.stats.luck, new Cell.TerrainBonus(), StatsCalc.SupportBonus(f));
            crit = StatsCalc.CriticalChance(StatsCalc.CriticalRate(w.crit, f.Unit.stats.skill, StatsCalc.SupportBonus(f), StatsCalc.ClassCrit(f.Unit.unitClass)), 0);
            acc = StatsCalc.Accuracy(StatsCalc.HitRate(w.hit, f.Unit.stats.skill, f.Unit.stats.luck, StatsCalc.SupportBonus(f)), StatsCalc.Avoid(f.Unit.stats.speed, f.Unit.stats.luck, new Cell.TerrainBonus(), StatsCalc.SupportBonus(f)), 1);
        }

        baseAtk = str;
        baseAcc = acc;
        baseCrt = crit;
        baseEva = evade;
    }

    void StatusMenu(Fighter f)
    {
        Weapon w = null;
        if (hovered.GetType() != typeof(Weapon))
        {
            if (f.Weapon != null)
                w = f.Weapon;
        }
        else
        {
            w = (Weapon)hovered;
        }

        if (w == null)
            w = ScriptableObject.CreateInstance<Weapon>();

        type.text = w.type.ToString().Replace('_', ' ');

        bool magic = w.type == Weapon.WeaponType.Anima_Tome || w.type == Weapon.WeaponType.Dark_Tome || w.type == Weapon.WeaponType.Light_Tome;

        int str = 0;
        float acc = StatsCalc.Accuracy(StatsCalc.HitRate(w.hit, f.Unit.stats.skill, f.Unit.stats.luck, StatsCalc.SupportBonus(f)), StatsCalc.Avoid(f.Unit.stats.speed, f.Unit.stats.luck, new Cell.TerrainBonus(), StatsCalc.SupportBonus(f)), 1);
        float crit = StatsCalc.CriticalChance(StatsCalc.CriticalRate(w.crit, f.Unit.stats.skill, StatsCalc.SupportBonus(f), StatsCalc.ClassCrit(f.Unit.unitClass)), 0);
        float evade = StatsCalc.Avoid(f.Unit.stats.speed, f.Unit.stats.luck, new Cell.TerrainBonus(), StatsCalc.SupportBonus(f));

        if (!magic)
            str = StatsCalc.PhysicalAttack(f.Unit.stats.strength, w.might, 1, 1, StatsCalc.SupportBonus(f), false);
        else
            str = StatsCalc.MagicalAttack(f.Unit.stats.magic, w.might, 1, 1, StatsCalc.SupportBonus(f));

        atk.text = "ATK: " + str;
        this.acc.text = "ACC: " + (int)acc;
        crt.text = "CRT: " + (int)crit;
        eva.text = "EVA: " + (int)evade;

        #region atk
        if (str > baseAtk)
        {
            atkAr.color = inc;
            atkAr.transform.localScale = Vector3.one;
            _atkAr.gameObject.SetActive(true);
        }
        else if (str < baseAtk)
        {
            atkAr.color = dec;
            atkAr.transform.localScale = new Vector3(1, -1, 1);
            _atkAr.gameObject.SetActive(true);
        }
        else
            _atkAr.gameObject.SetActive(false);
        #endregion
        #region acc
        if (acc > baseAcc)
        {
            accAr.color = inc;
            accAr.transform.localScale = Vector3.one;
            _accAr.gameObject.SetActive(true);
        }
        else if (acc < baseAcc)
        {
            accAr.color = dec;
            accAr.transform.localScale = new Vector3(1, -1, 1);
            _accAr.gameObject.SetActive(true);
        }
        else
            _accAr.gameObject.SetActive(false);
        #endregion
        #region crt
        if (crit > baseCrt)
        {
            crtAr.color = inc;
            crtAr.transform.localScale = Vector3.one;
            _crtAr.gameObject.SetActive(true);
        }
        else if (crit < baseCrt)
        {
            crtAr.color = dec;
            crtAr.transform.localScale = new Vector3(1, -1, 1);
            _crtAr.gameObject.SetActive(true);
        }
        else
            _crtAr.gameObject.SetActive(false);
        #endregion
        #region eva
        if (evade > baseEva)
        {
            evaAr.color = inc;
            evaAr.transform.localScale = Vector3.one;
            _evaAr.gameObject.SetActive(true);
        }
        else if (evade < baseEva)
        {
            evaAr.color = dec;
            evaAr.transform.localScale = new Vector3(1, -1, 1);
            _evaAr.gameObject.SetActive(true);
        }
        else
            _evaAr.gameObject.SetActive(false);
        #endregion
    }

    #region Item Action Menu
    void OpenActionMenu()
    {
        ActionMenuOpen = true;
        SpawnActionButtons();

        for (int i = 0; i < itemList.transform.childCount; i++)
        {
            itemList.transform.GetChild(i).GetComponent<Selectable>().interactable = false;
        }

        itemActionMenu.SetActive(true);
        itemActionList.GetComponentInChildren<UIHighlight>().Selectable.Select();
    }
    void CloseActionMenu()
    {
        itemActionMenu.SetActive(false);
        ClearItemActions();

        ActionMenuOpen = false;

        for (int i = 0; i < itemList.transform.childCount; i++)
        {
            itemList.transform.GetChild(i).GetComponent<Selectable>().interactable = true;
            itemList.transform.GetChild(i).GetComponent<UIHighlight>().Deselect();
        }

        GetComponentInChildren<UIHighlight>().Selectable.Select();
    }

    void SpawnActionButtons()
    {
        int height = 50;

        if (CanConsume())
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, itemActionList.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Use";
            g.GetComponent<Button>().onClick.AddListener(() => { Consume(Fighter); });

            g.GetComponent<TextMeshProUGUI>().color = hovered.IsUseable(Fighter) ? Color.white : Color.gray;
        }
        if (CanEquip())
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, itemActionList.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Equip";
            g.GetComponent<Button>().onClick.AddListener(() => { Equip(Fighter, (Weapon)hovered); });

            g.GetComponent<TextMeshProUGUI>().color = hovered != Fighter.Weapon ? Color.white : Color.gray;
        }
        if (CanTrade(Fighter))
        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, itemActionList.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Trade";
            g.GetComponent<Button>().onClick.AddListener(() => { Trade(Fighter); });
        }

        {
            height += 50;
            GameObject g = Instantiate(instance.buttonPrefab, itemActionList.transform);
            g.GetComponent<TextMeshProUGUI>().text = "Throw";
            g.GetComponent<Button>().onClick.AddListener(() => { Throw(Fighter); });
        }

        itemActionMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(175, height);
    }
    void ClearItemActions()
    {
        for (int i = itemActionList.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(itemActionList.transform.GetChild(i).gameObject);
        }
    }
    #region Checks
    bool CanConsume()
    {
        return hovered.consumable;
    }

    bool CanEquip()
    {
        return hovered.GetType() == typeof(Weapon) && hovered.IsUseable(Fighter);
    }

    bool CanTrade(Fighter f)
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
    #endregion

    #region Actions
    void Consume(Fighter f)
    {
        if (!hovered.IsUseable(f))
            return;

        hovered.Use(f);
        CloseActionMenu();
        Close();
    }

    void Equip(Fighter f, Weapon w)
    {
        f.Equip(w);
        SetBaseStats(f);

        CloseActionMenu();
    }

    void Trade(Fighter f)
    {
        instance.ClearList();
        CloseActionMenu();
        instance.menu.SetActive(false);

        ActionMenu.instance.Trade(f, true);

        ActionMenu.CanSelect = true;
        StartCoroutine(_Trade());
    }
    System.Collections.IEnumerator _Trade()
    {
        yield return new WaitForSeconds(0.1f);

        isTrading = true;
        IsOpen = false;
    }

    void Throw(Fighter f)
    {
        ThrowDialogueOpen = true;

        Selectable[] selectables = itemActionList.GetComponentsInChildren<Selectable>();
        for (int i = 0; i < selectables.Length; i++)
            selectables[i].interactable = false;

        throwDialogue.SetActive(true);
        throwDialogue.transform.GetChild(throwDialogue.transform.childCount - 1).GetComponent<UIHighlight>().Selectable.Select();

        throwDialogue.transform.GetChild(throwDialogue.transform.childCount - 2).GetComponent<Button>().onClick.AddListener(() => { _Throw(Fighter); });
        throwDialogue.transform.GetChild(throwDialogue.transform.childCount - 1).GetComponent<Button>().onClick.AddListener(() => { CloseThrowDialogue(); } );
    }

    void _Throw(Fighter f)
    {
        f.Unit.inventory.Remove(hovered);
        CloseThrowDialogue();
        CloseActionMenu();

        ResetMenu();
    }
    void CloseThrowDialogue()
    {
        UIHighlight[] s = throwDialogue.GetComponentsInChildren<UIHighlight>();
        foreach (UIHighlight u in s)
            u.Deselect();

        throwDialogue.transform.GetChild(throwDialogue.transform.childCount - 2).GetComponent<Button>().onClick.RemoveAllListeners();
        throwDialogue.transform.GetChild(throwDialogue.transform.childCount - 1).GetComponent<Button>().onClick.RemoveAllListeners();
        throwDialogue.SetActive(false);
        
        Selectable[] selectables = itemActionList.GetComponentsInChildren<Selectable>();
        for (int i = 0; i < selectables.Length; i++)
        {
            selectables[i].interactable = true;
            selectables[i].GetComponent<UIHighlight>().Deselect();
        }
        selectables[0].GetComponent<UIHighlight>().Selectable.Select();

        ThrowDialogueOpen = false;
    }
    #endregion
    #endregion
}