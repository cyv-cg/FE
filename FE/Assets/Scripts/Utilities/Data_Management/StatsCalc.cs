using UnityEngine;
using System.Collections.Generic;

public static class StatsCalc
{
    #region Constants

    public static readonly int RepeatedAttackThreshold = 5;

    public static readonly float WeaponTriangleBonus = 0.15f;

    public static readonly int SupportRange = 2;
    public static readonly int SupportBonusA = 8,
        SupportBonusB = 5,
        SupportBonusC = 3,
        SupportBonusS = 12;

    public static Dictionary<int, int> ExpGainTable = new Dictionary<int, int>
    {
        { -1, 1 },
        { 0, 3 },
        { 1, 6 },
        { 2, 8 },
        { 3, 10 },
        { 4, 10 },
        { 5, 10 },
        { 6, 10 },
        { 7, 13 },
        { 8, 16 },
        { 9, 19 },
        { 10, 22 },
        { 11, 25 },
        { 12, 28 },
        { 13, 30 }
    };

    public static Dictionary<int, int> MinLevelGainTable = new Dictionary<int, int>
    {
        { 1, 1 },
        { 2, 1 },
        { 3, 1 },
        { 4, 1 },
        { 5, 1 },
        { 6, 1 },
        { 7, 1 },
        { 8, 1 },
        { 9, 1 },
        { 10, 1 },
        { 11, 1 },
        { 12, 1 },
        { 13, 1 },
        { 14, 1 },
        { 15, 1 },
        { 16, 1 },
        { 17, 1 },
        { 18, 1 },
        { 19, 1 },
        { 20, 1 },
        { 21, 1 },
        { 22, 1 },
        { 23, 1 },
        { 24, 1 },
        { 25, 1 },
        { 26, 12 },
        { 27, 12 },
        { 28, 12 },
        { 29, 12 },
        { 30, 12 },
        { 31, 12 },
        { 32, 12 },
        { 33, 12 },
        { 34, 12 },
        { 35, 12 },
        { 36, 12 },
        { 37, 12 },
        { 38, 12 },
        { 39, 12 },
        { 40, 20 }
    };

    public static Dictionary<Unit.UnitClass, float> ClassCriticalModifier = new Dictionary<Unit.UnitClass, float>
    {
        { Unit.UnitClass.Berserker, 0.3f },
        { Unit.UnitClass.Swordmaster, 0.3f },
        { Unit.UnitClass.Sniper, 0.15f },
        { Unit.UnitClass.Pegasus_Knight, 0.05f },
    };

    #endregion

    #region Bonus Calculations

    public static int Seed()
    {
        return Random.Range(-int.MaxValue, int.MaxValue);
    }
    public static int Seed(Vector2Int[] positions)
    {
        int sum = 0;
        foreach (Vector2Int p in positions)
            sum += p.x + p.y;

        return sum;
    }

    public static float ClassCrit(Unit.UnitClass unitClass)
    {
        if (ClassCriticalModifier.ContainsKey(unitClass))
            return ClassCriticalModifier[unitClass];

        return 1;
    }

    /// <summary>
    /// The Weapon Triange behaves such that certain weapon types have combative dis/advantages when used against certain other weapon types.
    /// (one = the attacking unit, two = the defending unit)
    /// </summary>
    public static float TriangleBonus(Fighter one, Fighter two)
    {
        return TriangleBonus(one.Weapon, two.Weapon);

        //float bonus = 1f;

        //if (one.Weapon.type == Weapon.WeaponType.Sword)
        //{
        //    if (two.Weapon.type == Weapon.WeaponType.Sword)
        //        bonus = 1f;
        //    else if (two.Weapon.type == Weapon.WeaponType.Axe)
        //        bonus = 1f + WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Lance)
        //        bonus = 1f - WeaponTriangleBonus;
        //}
        //else if (one.Weapon.type == Weapon.WeaponType.Axe)
        //{
        //    if (two.Weapon.type == Weapon.WeaponType.Sword)
        //        bonus = 1f - WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Axe)
        //        bonus = 1f;
        //    else if (two.Weapon.type == Weapon.WeaponType.Lance)
        //        bonus = 1f + WeaponTriangleBonus;
        //}
        //else if (one.Weapon.type == Weapon.WeaponType.Lance)
        //{
        //    if (two.Weapon.type == Weapon.WeaponType.Sword)
        //        bonus = 1f + WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Axe)
        //        bonus = 1f - WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Lance)
        //        bonus = 1f;
        //}

        //else if (one.Weapon.type == Weapon.WeaponType.Anima_Tome)
        //{
        //    if (two.Weapon.type == Weapon.WeaponType.Anima_Tome)
        //        bonus = 1f;
        //    else if (two.Weapon.type == Weapon.WeaponType.Light_Tome)
        //        bonus = 1f + WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Dark_Tome)
        //        bonus = 1f - WeaponTriangleBonus;
        //}
        //else if (one.Weapon.type == Weapon.WeaponType.Light_Tome)
        //{
        //    if (two.Weapon.type == Weapon.WeaponType.Anima_Tome)
        //        bonus = 1f - WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Light_Tome)
        //        bonus = 1f;
        //    else if (two.Weapon.type == Weapon.WeaponType.Dark_Tome)
        //        bonus = 1f + WeaponTriangleBonus;
        //}
        //else if (one.Weapon.type == Weapon.WeaponType.Dark_Tome)
        //{
        //    if (two.Weapon.type == Weapon.WeaponType.Anima_Tome)
        //        bonus = 1f + WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Light_Tome)
        //        bonus = 1f - WeaponTriangleBonus;
        //    else if (two.Weapon.type == Weapon.WeaponType.Dark_Tome)
        //        bonus = 1f;
        //}

        //return bonus;
    }
    public static float TriangleBonus(Weapon one, Weapon two)
    {
        float bonus = 1f;

        if (one.type == Weapon.WeaponType.Sword)
        {
            if (two.type == Weapon.WeaponType.Sword)
                bonus = 1f;
            else if (two.type == Weapon.WeaponType.Axe)
                bonus = 1f + WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Lance)
                bonus = 1f - WeaponTriangleBonus;
        }
        else if (one.type == Weapon.WeaponType.Axe)
        {
            if (two.type == Weapon.WeaponType.Sword)
                bonus = 1f - WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Axe)
                bonus = 1f;
            else if (two.type == Weapon.WeaponType.Lance)
                bonus = 1f + WeaponTriangleBonus;
        }
        else if (one.type == Weapon.WeaponType.Lance)
        {
            if (two.type == Weapon.WeaponType.Sword)
                bonus = 1f + WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Axe)
                bonus = 1f - WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Lance)
                bonus = 1f;
        }

        else if (one.type == Weapon.WeaponType.Anima_Tome)
        {
            if (two.type == Weapon.WeaponType.Anima_Tome)
                bonus = 1f;
            else if (two.type == Weapon.WeaponType.Light_Tome)
                bonus = 1f + WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Dark_Tome)
                bonus = 1f - WeaponTriangleBonus;
        }
        else if (one.type == Weapon.WeaponType.Light_Tome)
        {
            if (two.type == Weapon.WeaponType.Anima_Tome)
                bonus = 1f - WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Light_Tome)
                bonus = 1f;
            else if (two.type == Weapon.WeaponType.Dark_Tome)
                bonus = 1f + WeaponTriangleBonus;
        }
        else if (one.type == Weapon.WeaponType.Dark_Tome)
        {
            if (two.type == Weapon.WeaponType.Anima_Tome)
                bonus = 1f + WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Light_Tome)
                bonus = 1f - WeaponTriangleBonus;
            else if (two.type == Weapon.WeaponType.Dark_Tome)
                bonus = 1f;
        }

        return bonus;
    }

    /// <summary>
    /// NYI
    /// </summary>
    public static float WeaponEffectiveness(Weapon attacking, Fighter defending)
    {
        return 1;
        //throw new System.NotImplementedException();
    }

    /// <summary>
    /// The Support Bonus adds to other stats depending on the rank of [f]'s relationship with nearby units.
    /// (f = the target unit)
    /// </summary>
    public static int SupportBonus(Fighter f)
    {
        if (f.Unit.relationships == null || f.Unit.relationships.Count == 0)
            return 0;

        Unit one = f.Unit;
        Unit two = null;

        Dictionary<Cell, int> range = Map.GetTileDepths(f, SupportRange);
        Dictionary<int, Unit> r = new Dictionary<int, Unit>();

        foreach (Unit u in f.Unit.relationships.Keys)
        {
            if (QPathfinding.QPath.FindPath(f, Map.UnitTile(f), Map.UnitTile(u), Cell.EstimateDistance).Length <= SupportRange + 1)
                    r.Add((int)f.Unit.relationships[u].rank, u);
        }

        if (r.Count == 0)
            return 0;
        
        int[] keys = new int[r.Count];
        r.Keys.CopyTo(keys, 0);
        two = r[Mathf.Max(keys)];

        if (one.relationships[two].rank == Unit.Relationship.Rank.S)
            return SupportBonusS;
        else if (one.relationships[two].rank == Unit.Relationship.Rank.A)
            return SupportBonusA;
        else if (one.relationships[two].rank == Unit.Relationship.Rank.B)
            return SupportBonusB;
        else if (one.relationships[two].rank == Unit.Relationship.Rank.C)
            return SupportBonusC;

        return 0;
    }

    /// <summary>
    /// Some terrains give stat bonuses to Defense and Avoid. Some terrains will heal a certain percent 
    /// of health per turn.
    /// </summary>
    public static Cell.TerrainBonus TerrainBonus(QPathfinding.IQPathTile cell)
    {
        Cell c = (Cell)cell;
        return c.terrainBonus;
    }

    #endregion
    #region Attacks
    /// <summary>
    /// Attack Speed is equal to the character's Speed, 
    /// unless the character's Constitution is less than the weight of the weapon they have equipped, 
    /// in which case its attack speed is reduced by the amount by which the weight exceeds its constitution.
    /// (speed = Unit's speed, wwt = weight of equipped weapon, con = unit's constitution)
    /// </summary>
    public static int AttackSpeed(int speed, int wwt, int con)
    {
        int AS = 0;

        if (wwt <= con)
            AS = speed;
        else if (wwt > con)
            AS = speed - (wwt - con);

        return AS;
    }

    /// <summary>
    /// A unit will attack twice in a battle if its attack speed is at least a certain amount higher than the opposing unit's attack speed.
    /// (speed1 = Attacking unit's attack speed, speed2 = Defending unit's attack speed)
    /// </summary>
    public static bool RepeatedAttack(int speed1, int speed2)
    {
        return speed1 - speed2 > RepeatedAttackThreshold;
    }

    /// <summary>
    /// Attack for physical itsms. Attack is used to calculate how much someone's attack could deal under optimum conditions. 
    /// This does not take into account anything about the enemy, so in reality after taking into account their defensive properties the actual damage will be a lot lower.
    /// (str = unit's strength, wmt = weapon might, triangle = triangle bonus, eff = WeaponEffectiveness(), supp = support bonus, specialCase = a case where a melee weapon is used at range, etc.)
    /// </summary>
    public static int PhysicalAttack(int str, int wmt, float triange, float eff, int supp, bool specialCase)
    {
        int atk = 0;

        if (!specialCase)
            atk = (int)(str + (wmt * triange) * eff + supp);
        else
            atk = (int)(str / 2f + (wmt * triange) + eff + supp);

        return atk;
    }

    /// <summary>
    /// Attack strength for magical items. Attack is used to calculate how much someone's attack could deal under optimum conditions. 
    /// This does not take into account anything about the enemy, so in reality after taking into account their defensive properties the actual damage will be a lot lower.
    /// (magic = unit's magic stat, mmt = magic might, triangle = triangle bonus, eff = weapon effectiveness, supp = support bonus)
    /// </summary>
    public static int MagicalAttack(int magic, int mmt, float triangle, float eff, int supp)
    {
        float atk = magic + (mmt * triangle) * eff + supp;

        return (int)atk;
    }

    /// <summary>
    /// Hit Rate is the chance a character has of hitting a stationary object.
    /// (acc = weapon accuracy, skill = unit's skill, luck = unit's luck, supp = support bonus)
    /// </summary>
    public static float HitRate(float acc, int skill, int luck, int supp)
    {
        return 100 * acc + skill * 2 + luck / 2f + supp;
    }

    /// <summary>
    /// Accuracy is used in battle to determine how likely a character is to hit another character, using the difference between the attacker's hit rate and the defender's evade.
    /// (hitRate = attackers HitRate(), avoid = defender's Avoid(), triangleBonus = TriangleBonus())
    /// </summary>
    public static float Accuracy(float hitRate, float avoid, float triangleBonus)
    {
        float acc = (hitRate - avoid) * triangleBonus;

        return Mathf.Clamp((int)acc, 0, 100);
    }

    #endregion
    #region Defends
    /// <summary>
    /// Avoid is a measure of how well a unit can avoid being hit by an enemy's attack.
    /// (speed = unit's attack speed, luck = unit's luck, terrain = local terrain bonus, supp = support bonus)
    /// </summary>
    public static float Avoid(int speed, int luck, Cell.TerrainBonus terrain, int supp)
    {
        return speed * 2 + luck + terrain.avoid + supp;
    }
    
    /// <summary>
    /// Defense from physical items. Defense power is a measure of the total damage a character can negate from enemy attacks.
    /// (terrain = local terrain bonus, def = unit's defense stat, supp = support bonus)
    /// </summary>
    public static int PhysicalDefense(Cell.TerrainBonus terrain, int def, int supp)
    {
        return terrain.defense + def + supp;
    }

    /// <summary>
    /// Defense from magical items. Defense power is a measure of the total damage a character can negate from enemy attacks.
    /// (terrain = local terrain bonus, res = unit's resistance stat, supp = support bonus)
    /// </summary>
    public static int MagicalDefense(Cell.TerrainBonus terrain, int res, int supp)
    {
        return terrain.defense + res + supp;
    }

    /// <summary>
    /// Damage is the amount of health an attack takes away from a defending unit if the attack hits. 
    /// It depends on the attacker's attack power and the defender's Resistance or Defense (for magical or physical attacks respectively.).
    /// (atk = attacker's XAttack(), def = defender's XDefense())
    /// </summary>
    public static int Damage(int atk, int def)
    {
        return Mathf.Clamp(atk - def, 0, 60);
    }

    #endregion
    #region Criticals

    /// <summary>
    /// A character's critical rate is the likelihood of their managing to perform a critical hit against a stationary target.
    /// (wcrt = weapon's critical chance, skill = attackers skill stat, supp = support bonus, classCrit = unit class's critical modifier)
    /// </summary>
    public static float CriticalRate(float wcrt, int skill, int supp, float classCrit)
    {
        float rate = ((100 * wcrt) + skill / 2f + supp) * classCrit;
        return rate;
    }

    /// <summary>
    /// When attacking, characters often have the chance to strike a critical hit. This will in general do a vastly higher amount of damage than a regular hit.
    /// (damage = Damage())
    /// </summary>
    public static int CriticalDamage(int damage)
    {
        return damage * 3;
    }

    /// <summary>
    /// Critical Evade is to Critical Rate as Evade is to Accuracy. It is a measurement of the chance a character has to 
    /// detect an approaching critical hit and react accordingly; the amount by which an attacker's critical rate is reduced.
    /// (luck = unit's luck stat, supp = support bonus)
    /// </summary>
    public static float CriticalEvade(int luck, int supp)
    {
        return luck + supp;
    }

    /// <summary>
    /// Critical chance is the percentage used in battle to determine the chance of a character landing a critical hit. 
    /// It is determined from the attacker's critical rate minus the defender's critical evade.
    /// (critRate = attacker's CriticalRate(), critEvade = defender's CriticalEvade())
    /// </summary>
    public static float CriticalChance(float critRate, float critEvade)
    {
        return critRate - critEvade;
    }

    #endregion
    #region Experience

    /// <summary>
    /// NYI
    /// </summary>
    public static float EXPModifier()
    {
        return 1;
    }

    /// <summary>
    /// Amount of experience required to get to the next level
    /// (level = the unit's level)
    /// </summary>
    public static int ExperienceToNextLevel(int level)
    {
        return (int)(Mathf.Pow(1.1f, level - 2) * 100);
    }

    public static int TotalExperience(int level)
    {
        int t = 0;

        for (int i = 2; i <= level; i++)
        {
            t += ExperienceToNextLevel(i);
        }

        return t;
    }

    /// <summary>
    /// Experience gained from attacking an enemy.
    /// (unitLevel = the level of the attacking unit, enemyLevel = level of the defending unit, consecutiveAttacks = the number of times unit has attacked the same enemy in a row,
    /// enemyDefeated = whether or not the enemy died, specialTriggered = whether or not the unit's special attack was used in combat)
    /// </summary>
    public static int EXPGain_Damage(int unitLevel, int enemyLevel, int consecutiveAttacks, bool enemyDefeated, bool specialTriggered)
    {
        float _base = ExpGainTable[Mathf.Clamp(enemyLevel - MinLevelGainTable[unitLevel], -1, 13)];

        if (consecutiveAttacks == 3)
            _base *= 0.75f;
        else if (consecutiveAttacks == 4)
            _base *= 0.5f;
        else if (consecutiveAttacks == 5)
            _base *= 0.25f;
        else if (consecutiveAttacks >= 6)
            _base *= 0f;

        if (enemyDefeated)
            _base *= 10f;

        if (specialTriggered)
            _base *= 1.5f;

        return (int)(_base * EXPModifier());
    }

    /// <summary>
    /// Experience gained from healing an ally.
    /// (heals = number of times this unit healed someone in battle, specialTriggered = whether or not the unit's special attack was used in combat)
    /// </summary>
    public static int EXPGain_Heal(int heals, bool specialTriggered)
    {
        float _base = 50;

        if (heals > 5)
            _base -= 5 * (heals - 5);

        if (specialTriggered)
            _base *= 1.5f;

        return (int)_base;
    }

    #endregion
}