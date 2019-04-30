using UnityEngine;
using QPathfinding;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public static bool IsBattling { get; private set; }

    public static Offensive Current { get; private set; }

    public class Offensive
    {
        private System.Random Rand;

        public readonly Fighter Attacker;
        public readonly Fighter Defender;

        public readonly int Seed;

        public readonly bool Crit;
        public readonly bool Hit;

        public readonly bool IsPhysical;

        public bool IsCounter;

        #region Stats

        private readonly int AtkStat;
        private readonly int DefStat;

        private readonly IQPathTile Cell;

        private readonly int Support1;
        private readonly int Support2;

        private readonly float TriangleBonus;
        private readonly Cell.TerrainBonus TerrainBonus1;
        private readonly Cell.TerrainBonus TerrainBonus2;

        private readonly int Speed1;
        private readonly int Speed2;

        private readonly int Wgt1;
        private readonly int Wgt2;

        private readonly int Con1;
        private readonly int Con2;

        private readonly int Attack;

        private readonly int Defense;

        private readonly int Spd1;
        private readonly int Spd2;
        
        private readonly int Atk;
        private readonly int Def;

        private readonly float HitRate;
        private readonly float Avoid;
        private readonly float Acc;

        private readonly float CritRate;
        private readonly float CritEvade;
        private readonly float CritChance;

        #endregion

        public readonly int Damage;

        public Offensive(Fighter attacker, Fighter defender, bool repeated = false)
        {
            Attacker = attacker;
            Defender = defender;

            Seed = StatsCalc.Seed();
            Rand = new System.Random(Seed);

            IsPhysical = Attacker.Weapon.isPhysical;

            AtkStat = IsPhysical ? Attacker.Unit.stats.strength : Attacker.Unit.stats.magic;
            DefStat = IsPhysical ? Defender.Unit.stats.defense : Defender.Unit.stats.resistance;

            Cell = Map.UnitTile(Attacker);

            Support1 = StatsCalc.SupportBonus(Attacker);
            Support2 = StatsCalc.SupportBonus(Defender);

            TriangleBonus = StatsCalc.TriangleBonus(Attacker, Defender);
            TerrainBonus1 = StatsCalc.TerrainBonus(Map.UnitTile(Attacker));
            TerrainBonus2 = StatsCalc.TerrainBonus(Map.UnitTile(Defender));

            Speed1 = Attacker.Unit.stats.speed;
            Speed2 = Defender.Unit.stats.speed;

            Wgt1 = Attacker.Weapon.weight;
            Wgt2 = Defender.Weapon.weight;

            Con1 = Attacker.Unit.stats.constitution;
            Con2 = Defender.Unit.stats.constitution;

            Attack = AtkStat;
            Defense = DefStat;

            Spd1 = StatsCalc.AttackSpeed(Speed1, Wgt1, Con1);
            Spd2 = StatsCalc.AttackSpeed(Speed2, Wgt2, Con2);
            
            Atk = StatsCalc.PhysicalAttack(Attack, Attacker.Weapon.might, TriangleBonus, StatsCalc.WeaponEffectiveness(Attacker.Weapon, Defender), Support1, false);
            Def = StatsCalc.PhysicalDefense(TerrainBonus2, Defense, Support2);

            HitRate = StatsCalc.HitRate(Attacker.Weapon.hit, Attacker.Unit.stats.skill, Attacker.Unit.stats.luck, Support1);
            Avoid = StatsCalc.Avoid(Speed2, Defender.Unit.stats.luck, TerrainBonus2, Support2);
            Acc = StatsCalc.Accuracy(HitRate, Avoid, TriangleBonus);

            CritRate = StatsCalc.CriticalRate(Attacker.Weapon.crit, Attacker.Unit.stats.skill, Support1, StatsCalc.ClassCrit(Attacker.Unit.unitClass));
            CritEvade = StatsCalc.CriticalEvade(Defender.Unit.stats.luck, Support2);
            CritChance = StatsCalc.CriticalChance(CritRate, CritEvade);

            if (StatsCalc.ClassCriticalModifier.ContainsKey(Attacker.Unit.unitClass))
                CritChance += StatsCalc.ClassCriticalModifier[Attacker.Unit.unitClass];

            if (repeated)
                CritChance += Attacker.Unit.stats.pursuitCriticalCoefficient;

            Hit = Rand.Next(0, 100) <= Acc;
            Crit = Rand.Next(0, 100) <= CritChance;

            Damage = Crit ? StatsCalc.CriticalDamage(StatsCalc.Damage(Atk, Def)) : StatsCalc.Damage(Atk, Def);

            if (Attacker.LastTarget == Defender)
                Attacker.RepeatedTarget++;
            else
            {
                Attacker.LastTarget = Defender;
                Attacker.RepeatedTarget = 0;
            }
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public static void Attack(Fighter attacker, Fighter target, bool doAnimations)
    {
        IsBattling = true;

        int spd1 = StatsCalc.AttackSpeed(attacker.Unit.stats.speed, attacker.Weapon.weight, attacker.Unit.stats.constitution);
        int spd2 = StatsCalc.AttackSpeed(target.Unit.stats.speed, target.Weapon.weight, target.Unit.stats.constitution);

        bool repeated = StatsCalc.RepeatedAttack(spd1, spd2);
        bool repeatedCounter = StatsCalc.RepeatedAttack(spd2, spd1);
        bool counter = false;

        Cell[] counterArea = Map.GetExtendedArea(new Cell[1] { Map.UnitTile(target) }, target.Weapon.range, target.Weapon.rangedClosedSet, target.Weapon.closedSetMin);

        foreach (Cell c in counterArea)
            if (c == Map.UnitTile(attacker))
            {
                counter = true;
                break;
            }

        Offensive atk1 = new Offensive(attacker, target);
        Offensive cnt1 = null;

        Offensive atk2 = null;
        Offensive cnt2 = null;

        if (counter)
        {
            cnt1 = new Offensive(target, attacker);
            cnt1.IsCounter = true;
        }
        if (repeated)
            atk2 = new Offensive(attacker, target, true);
        if (repeatedCounter)
        {
            cnt2 = new Offensive(target, attacker, true);
            cnt1.IsCounter = true;
        }

        instance.StartCoroutine(DoAttacks(
            new Offensive[4]
            {
                atk1,
                cnt1,
                atk2,
                cnt2
            }
        ));
    }

    private static IEnumerator DoAttacks(Offensive[] offensives)
    {
        if (GameSettings.DoBattleAnimation)
            yield return instance.StartCoroutine(BattleScreen.Open(offensives[0].Attacker, offensives[0].Defender));

        foreach (Offensive o in offensives)
        {
            if (o != null && o.Attacker != null && o.Defender != null)
            {
                Current = o;
                yield return _Attack(o);
            }
        }

        if (GameSettings.DoBattleAnimation)
            yield return instance.StartCoroutine(BattleScreen.Close());

        IsBattling = false;
    }
    private static IEnumerator _Attack(Offensive offensive)
    {
        if (offensive.Attacker.Weapon.uses <= 0)
            yield break;

        if (GameSettings.DoBattleAnimation)
            yield return BattleAnimController.WaitForBattleAnimation(offensive);

        if (offensive.Hit)
        {
            offensive.Defender.Damage(offensive.Damage);

            Debug.Log(offensive.Attacker.Unit.name + " attacked " + offensive.Defender.Unit.name + " for " + offensive.Damage + " damage \n" +
            offensive.Defender.Unit.name + " hp: " + offensive.Defender.CurrentHP + "/" + offensive.Defender.Unit.stats.hp);
        }
        else
            Debug.Log(offensive.Attacker.Unit.name + " missed " + offensive.Defender.Unit.name);

        if (!offensive.Attacker.Weapon.unbreakable)
            offensive.Attacker.Weapon.uses--;

        if (offensive.Attacker.CurrentHP <= 0)
            UnitManager.Kill(offensive.Attacker);
        if (offensive.Defender.CurrentHP <= 0)
            UnitManager.Kill(offensive.Defender);

        offensive.Attacker.AddEXP(
            StatsCalc.EXPGain_Damage(
                offensive.Attacker.Unit.stats.level,
                offensive.Defender.Unit.stats.level,
                offensive.Attacker.RepeatedTarget,
                offensive.Defender == null || offensive.Defender.CurrentHP <= 0,
                false
            )
        );

        while (GameSettings.DoBattleAnimation && BattleAnimController.IsAnimating)
            yield return null;

        yield return new WaitForSeconds(0.5f);
    }
}