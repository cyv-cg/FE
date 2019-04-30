using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TradeMenu : MonoBehaviour
{
    public static TradeMenu instance;

    public static bool IsOpen { get; private set; }

    public GameObject menu, itemButtonPrefab;

    public Transform inv1, inv2;

    private static Fighter Fighter;
    private static Fighter Target;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (IsOpen && InputManager.RightClickUp())
        {
            ActionMenu.Open(PhaseManager.Fighter);
            Close();
        }
    }

    public static void Open(Fighter one, Fighter two)
    {
        if (IsOpen)
            return;

        Fighter = one;
        Target = two;

        ActionMenu.Close();

        instance.menu.SetActive(true);
        instance.ResetLists(one, two);
        IsOpen = true;
    }
    public static void Close()
    {
        instance.ClearLists();
        instance.menu.SetActive(false);
        Fighter = null;
        instance.StartCoroutine(instance._Close());
    }

    System.Collections.IEnumerator _Close()
    {
        yield return new WaitForSeconds(0.1f);
        IsOpen = false;
        
        if (!ActionMenu.instance.TradeThroughItemMenu)
            ActionMenu.Open(PhaseManager.Fighter);
    }

    void SpawnItems(Fighter f, Transform parent)
    {
        foreach (Item i in f.Unit.inventory.GetOrderedArray(f))
        {
            GameObject go = Instantiate(itemButtonPrefab, parent);
            ItemButton btn = go.GetComponent<ItemButton>();

            if (i.IsUseable(f))
            {
                btn.label.color = Color.white;
            }
            else
                btn.label.color = Color.gray;

            btn.SetItem(i);
            SetMethods(btn);
        }

        if (f.Unit.inventory.Items.Count < Inventory.Size)
        {
            GameObject go = Instantiate(itemButtonPrefab, parent);
            ItemButton btn = go.GetComponent<ItemButton>();

            btn.SetItem(Item.Blank());

            SetMethods(btn);
        }
    }
    void SetMethods(ItemButton btn)
    {
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            btn.GetComponent<UIHighlight>().MakeStatic();
        });

        btn.GetComponent<UIHighlight>().OnMadeStaticAction += () => { Trade(); };
    }

    void ClearLists()
    {
        for (int i = inv1.childCount - 1; i >= 0; i--)
            Destroy(inv1.GetChild(i).gameObject);
        for (int i = inv2.childCount - 1; i >= 0; i--)
            Destroy(inv2.GetChild(i).gameObject);
    }
    void ResetLists(Fighter one, Fighter two)
    {
        ClearLists();
        SpawnItems(one, inv1);
        SpawnItems(two, inv2);
    }

    void Trade()
    {
        Item fromUnit = null;
        Item fromTarget = null;

        foreach(ItemButton b in inv1.GetComponentsInChildren<ItemButton>())
            if (b.GetComponent<UIHighlight>().IsStatic)
            {
                fromUnit = b.GetItem();
                break;
            }

        foreach (ItemButton b in inv2.GetComponentsInChildren<ItemButton>())
            if (b.GetComponent<UIHighlight>().IsStatic)
            {
                fromTarget = b.GetItem();
                break;
            }

        if (fromUnit == null || fromTarget == null)
            return;

        Fighter.Unit.inventory.Trade(Fighter, Target, fromUnit, fromTarget);

        ActionMenu.BaseAction();
        ResetLists(Fighter, Target);
    }
}