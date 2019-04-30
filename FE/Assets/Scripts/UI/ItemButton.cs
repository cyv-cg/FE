using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemButton : MonoBehaviour
{
    private Item item;

    public TextMeshProUGUI label, uses;
    public Image icon;

    public void SetItem(Item item)
    {
        this.item = item;
        UpdateIcon();
    }
    public Item GetItem()
    {
        return item;
    }

    void UpdateIcon()
    {
        if (item == Item.Blank())
        {
            label.text = "";
            uses.text = "";
            icon.color = Color.clear;

            return;
        }

        label.text = item.name;
        uses.text = !item.unbreakable ? item.uses.ToString() : "";
        icon.sprite = item.icon;
    }
}