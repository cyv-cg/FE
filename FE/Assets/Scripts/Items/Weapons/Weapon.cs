using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Weapon", menuName = "Item/Weapon")]
public class Weapon : Item
{
    //public enum WeaponType { Sword = 0, Lance = 1, Axe = 2, Bow = 4, Anima_Tome = 8, Light_Tome = 16, Dark_Tome = 32, Staff = 64, Dragonstone = 128 }
    public enum WeaponType { Sword = 1, Lance = 2, Axe = 4, Bow = 8, Anima_Tome = 16, Light_Tome = 32, Dark_Tome = 64, Staff = 128, Dragonstone = 256 }

    public class AttackData
    {
        public readonly int range;
        public readonly bool closedSet;
        public readonly int closedSetMin;
        public Unit.Alignment targets;

        public AttackData(int range = 1, bool closedSet = true, int closedSetMin = 0, Unit.Alignment targets = Unit.Alignment.Enemy)
        {
            this.range = range;
            this.closedSet = closedSet;
            this.closedSetMin = closedSetMin;
            this.targets = targets;
        }
    }

    public WeaponType type = WeaponType.Sword;
    public Element affinity;
    [EnumFlag(4)] public Unit.Alignment target = Unit.Alignment.Enemy;
    [EnumFlags] public Unit.UnitClass classRestriction;
    
    [Space]
     
    public bool isPhysical = true;
    public int weight = 5;
    public int might = 5;
    [Range(0f, 1f)] public float hit = 0.85f, 
        crit = 0.05f;
    public int range = 1;
    public bool rangedClosedSet = true;
    public int closedSetMin = 1;
    
    public override bool IsUseable(Fighter f)
    {
        return f.Unit.useable.HasFlag(type);
    }
    public bool IsUseable(Unit u)
    {
        return u.useable.HasFlag(type);
    }

    public System.Collections.Generic.List<int> Range()
    {
        System.Collections.Generic.List<int> r = new System.Collections.Generic.List<int>();

        if (!rangedClosedSet)
            for (int i = 1; i <= range; i++)
                r.Add(i);
        else
            for (int i = closedSetMin; i <= range; i++)
                r.Add(i);

        return r;
    }

    public override Item CreateInstance()
    {
        Weapon w = CreateInstance<Weapon>();

        w.icon = icon;
        w.name = name;
        w.cost = cost;
        w.uses = uses;
        w.unbreakable = unbreakable;
        w.consumable = consumable;

        w.target = target;
        w.type = type;
        w.isPhysical = isPhysical;
        w.weight = weight;
        w.might = might;
        w.hit = hit;
        w.crit = crit;
        w.range = range;
        w.rangedClosedSet = rangedClosedSet;
        w.closedSetMin = closedSetMin;
        w.classRestriction = classRestriction;
        w.affinity = affinity;
        
        return w;
    }
}