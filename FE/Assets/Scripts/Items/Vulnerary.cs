using UnityEngine;

[CreateAssetMenu(fileName = "Vulnerary", menuName = "Item/Vulnerary")]
public class Vulnerary : Item
{
    public int healAmount = 10;

    public override void Use(Fighter f)
    {
        f.CurrentHP += healAmount;
        uses--;
    }

    public override Item CreateInstance()
    {
        Vulnerary i = CreateInstance<Vulnerary>();

        i.icon = icon;
        i.name = name;
        i.cost = cost;
        i.uses = uses;
        i.unbreakable = unbreakable;
        i.consumable = consumable;

        i.healAmount = healAmount;
        i.uses = uses;

        return i;
    }

    public override bool IsUseable(Fighter f)
    {
        return f.CurrentHP < f.Unit.stats.hp;
    }
}