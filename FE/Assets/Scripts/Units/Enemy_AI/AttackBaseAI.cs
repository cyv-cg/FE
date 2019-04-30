using System.Collections.Generic;

public class AttackBaseAI : BaseAI
{
    private List<Fighter> atkTargets = new List<Fighter>();

    protected Fighter Target { get; private set; }

    protected void FindTarget(Cell[] area)
    {
        foreach (Cell c in area)
        {
            Unit u = null;
            if (c.unitInTile != null)
                u = c.unitInTile.Unit;

            if (u == null)
                continue;
            
            if (AtkData != null && targets.HasFlag(u.alignment))
                atkTargets.Add(c.unitInTile);
        }

        if (atkTargets.Count == 0)
            return;

        int min = 999;
        foreach (Fighter f in atkTargets)
        {
            if (f.CurrentHP < min)
            {
                Target = f;
                min = f.CurrentHP;
            }
        }
    }

    private List<Weapon> GetBonus(List<Weapon> weapons, Fighter t)
    {
        Weapon e = t.Weapon;

        if (e == null)
            return weapons;

        List<Weapon> bonus = new List<Weapon>();

        foreach (Weapon w in weapons)
        {
            if (StatsCalc.TriangleBonus(w, e) > 1 || StatsCalc.WeaponEffectiveness(w, Target) > 1)
                bonus.Add(w);
        }

        if (bonus.Count == 0)
            return weapons;

        return bonus;
    }
    protected void ChooseWeapon()
    {
        if (Target == null || Fighter.Unit.inventory.GetWeapons(true, Fighter, false).Length == 0)
            return;

        Weapon chosen = Fighter.Unit.inventory.GetWeapons(true, Fighter, false)[0];

        int dist = Map.DistBtwn(Map.UnitTile(Fighter), Map.UnitTile(Target), Fighter);

        List<Weapon> useable = new List<Weapon>();

        foreach (Weapon w in Fighter.Unit.inventory.GetWeapons(true, Fighter, false))
        {
            if (w.Range().Contains(dist))
                useable.Add(w);
        }

        int might = 0;
        foreach (Weapon w in GetBonus(useable, Target))
        {
            if (w.might > might)
            {
                chosen = w;
                might = w.might;
            }
        }

        Fighter.Equip(chosen);
    }

    protected void Attack()
    {
        if (Fighter.Weapon == null || Target == null)
            return;

        BattleManager.Attack(Fighter, Target, GameSettings.DoBattleAnimation);
    }
}