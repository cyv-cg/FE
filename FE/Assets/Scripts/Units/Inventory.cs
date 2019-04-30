using System.Collections.Generic;

public class Inventory
{
    public static readonly int Size = 5;

    public List<Item> Items;

    public Item[] GetOrderedArray(Fighter f)
    {
        //if (f.Weapon == null)
            return Items.ToArray();

        //List<Item> _i = new List<Item>();
        //foreach (Item i in Items)
        //    _i.Add(i);

        //List<Item> items = new List<Item>();
        //items.Add(f.Weapon);
        //_i.Remove(f.Weapon);

        //foreach (Item i in _i)
        //    items.Add(i);

        //return items.ToArray();
    }

    public void Add(Item item)
    {
        if (Items == null)
            Items = new List<Item>();

        if (Items.Count >= Size)
            return;

        Items.Add(item);
    }
    public void Remove(Item item)
    {
        if (Items == null || Items.Count == 0)
            return;
        
        for (int i = 0; i < Items.Count; i++)
            if (Items[i] == item)
            {
                Items.RemoveAt(i);
                break;
            }
    }

    public Weapon[] GetWeapons(Fighter f, bool includeStaves = false)
    {
        return GetWeapons(false, f, includeStaves);
    }
    public Weapon[] GetWeapons(bool useableOnly, Fighter f, bool includeStaves = false)
    {
        List<Item> _items = new List<Item>();
        foreach (Item i in Items)
            if (i.GetType() == typeof(Weapon) && (!useableOnly || i.IsUseable(f)))
                _items.Add(i);

        List<Weapon> weapons = new List<Weapon>();
        foreach (Weapon w in _items)
            if (includeStaves || w.type != Weapon.WeaponType.Staff)
            weapons.Add(w);

        if (weapons == null)
            weapons = new List<Weapon>();

        return weapons.ToArray();
    }

    public Weapon[] GetStaves(Fighter f, bool useableOnly = false)
    {
        List<Item> _items = new List<Item>();
        foreach (Item i in Items)
        {
            Weapon w = i.GetType() == typeof(Weapon) ? (Weapon)i : null;
            if (i.GetType() == typeof(Weapon) && w.type == Weapon.WeaponType.Staff && (!useableOnly || i.IsUseable(f)))
                _items.Add(i);
        }

        List<Weapon> staves = new List<Weapon>();
        foreach (Weapon s in _items)
            if (s.type == Weapon.WeaponType.Staff)
            staves.Add(s);

        if (staves == null)
            staves = new List<Weapon>();

        return staves.ToArray();
    }

    public Weapon.AttackData GetAttackData(Fighter f)
    {
        Weapon weapon = null;

        if (GetWeapons(true, f, false).Length == 0)
            return null;

        List<Unit.Alignment> targets = new List<Unit.Alignment>();
        Unit.Alignment _t = 0;

        foreach (Weapon w in GetWeapons(true, f, false))
        {
            foreach (Unit.Alignment a in System.Enum.GetValues(typeof(Unit.Alignment)))
            {
                if (!targets.Contains(a) && w.target.HasFlag(a))
                    targets.Add(a);
            }

            if (weapon == null)
            {
                weapon = w;
                continue;
            }

            if (w.range > weapon.range)
                weapon = w;
        }

        foreach (Unit.Alignment a in targets)
            _t += (int)a;
        
        return new Weapon.AttackData(weapon.range, weapon.rangedClosedSet, weapon.closedSetMin, _t);
    }
    public Weapon.AttackData GetStaffData(Fighter f)
    {
        Weapon staff = null;

        if (GetStaves(f, true).Length == 0)
            return null;

        List<Unit.Alignment> targets = new List<Unit.Alignment>();
        Unit.Alignment _t = 0;

        foreach (Weapon s in GetStaves(f, true))
        {
            foreach (Unit.Alignment a in System.Enum.GetValues(typeof(Unit.Alignment)))
            {
                if (!targets.Contains(a) && s.target.HasFlag(a))
                    targets.Add(a);
            }
            if (staff == null)
            {
                staff = s;
                continue;
            }

            if (s.range > staff.range)
                staff = s;
        }

        foreach (Unit.Alignment a in targets)
            _t += (int)a;

        return new Weapon.AttackData(staff.range, staff.rangedClosedSet, staff.closedSetMin, _t);
    }

    public void Trade(Fighter _unit, Fighter _target, Item fromUnit, Item fromTarget)
    {
        Inventory target = _target.Unit.inventory;

        if (target.Items == null)
            target.Items = new List<Item>();
        if (Items == null)
            Items = new List<Item>();
        
        if (fromUnit != Item.Blank())
        {
            Remove(fromUnit);
            target.Add(fromUnit);
        }

        if (fromTarget != Item.Blank())
        {
            target.Remove(fromTarget);
            Add(fromTarget);
        }

        if (fromUnit == _unit.Weapon)
            _unit.Equip(GetWeapons(true, _unit).Length > 0 ? GetWeapons(true, _unit)[0] : null);
        if (fromTarget == _target.Weapon)
            _target.Equip(target.GetWeapons(true, _target).Length > 0 ? target.GetWeapons(true, _target)[0] : null);
    }
}