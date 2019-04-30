using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Unit : ScriptableObject
{
    [Serializable]
    public class Relationship
    {
        public enum Rank { None = 0, S = 8, A = 4, B = 2, C = 1 }
        
        public Rank rank = Rank.None;

        public Relationship(Rank rank)
        {
            this.rank = rank;
        }
    }

    [Serializable]
    public class Stats
    {
        public int levelCap = 40;

        [Serializable]
        public class GrowthRates
        {
            [Range(0f, 2f)] public float 
                hp,
                strength,
                magic,
                skill,
                speed,
                luck,
                defense,
                resistance;

            public GrowthRates() { }
            public GrowthRates(float hp, float strength, float magic, float skill, float speed, float luck,
                float defense, float resistance)
            {
                this.hp = hp;
                this.strength = strength;
                this.magic = magic;
                this.skill = skill;
                this.speed = speed;
                this.luck = luck;
                this.defense = defense;
                this.resistance = resistance;
            }
        }
        
        [Range(1, 40)] public int level = 1;
        [Range(1, 99)] public int hp = 15;

        [Range(1, 99)] public int strength;
        [Range(1, 99)] public int magic;
        [Range(1, 99)] public int skill;
        [Range(1, 99)] public int speed;
        [Range(1, 99)] public int luck;
        [Range(1, 99)] public int defense;
        [Range(1, 99)] public int resistance;
        [Range(1, 99)] public int constitution;
        [Range(1, 99)] public int movement;

        [Tooltip("Essentially, a unit's Aid stat is the maximum constitution or build that another unit can have for the first one to be able to Rescue them. The highest Aid stats are usually attributed to mounted units, such as cavaliers and wyvern riders.")]
        [Range(1, 99)] public int aid;
        [Tooltip("On the second attack of a double attack, the character's Critical rate is multiplied by their PCC. Characters with a PCC of 0 cannot perform a critical attack on their second hit.")]
        [Range(0f, 1f)] public float pursuitCriticalCoefficient;

        public GrowthRates growthRates;

        public Stats Add(Stats a, Stats b)
        {
            return new Stats(a.strength + b.strength, a.magic + b.magic, a.skill + b.skill, a.speed + b.speed, a.luck + b.luck,
                a.defense + b.defense, a.resistance + b.resistance, a.constitution + b.constitution, a.movement + b.movement,
                a.aid + b.aid, a.pursuitCriticalCoefficient + b.pursuitCriticalCoefficient);
        }

        public Stats() { }
        public Stats(int strength, int magic, int skill, int speed, int luck, 
            int defense, int resistance, int constitution, int movement, int aid, float pcc)
        {
            this.strength = strength;
            this.magic = magic;
            this.skill = skill;
            this.speed = speed;
            this.luck = luck;
            this.defense = defense;
            this.resistance = resistance;
            this.constitution = constitution;
            this.movement = movement;
            this.aid = aid;
            pursuitCriticalCoefficient = pcc;
        }
        public Stats(int strength, int magic, int skill, int speed, int luck, int defense, int resistance, int constitution, 
            int movement, int aid, float pcc, GrowthRates growthRates)
        {
            this.strength = strength;
            this.magic = magic;
            this.skill = skill;
            this.speed = speed;
            this.luck = luck;
            this.defense = defense;
            this.resistance = resistance;
            this.constitution = constitution;
            this.movement = movement;
            this.aid = aid;
            pursuitCriticalCoefficient = pcc;
            this.growthRates = growthRates;
        }
        public Stats(Stats stats)
        {
            strength = stats.strength;
            magic = stats.magic;
            skill = stats.skill;
            speed = stats.speed;
            luck = stats.luck;
            defense = stats.defense;
            resistance = stats.resistance;
            constitution = stats.constitution;
            movement = stats.movement;
            aid = stats.aid;
            pursuitCriticalCoefficient = stats.pursuitCriticalCoefficient;
            growthRates = stats.growthRates;
        }
        public Stats(Stats stats, GrowthRates growthRates)
        {
            strength = stats.strength;
            magic = stats.magic;
            skill = stats.skill;
            speed = stats.speed;
            luck = stats.luck;
            defense = stats.defense;
            resistance = stats.resistance;
            constitution = stats.constitution;
            movement = stats.movement;
            aid = stats.aid;
            pursuitCriticalCoefficient = stats.pursuitCriticalCoefficient;
            this.growthRates = growthRates;
        }
    }

    public Unit CreateInstance() {
        return CreateInstance<Unit>();
    }
    public Unit CreateInstance(string name, UnitClass unitClass, Alignment alignment, Weapon.WeaponType useable, Element affinity, Stats stats, Stats.GrowthRates growthRates)
    {
        Unit instance = CreateInstance<Unit>();

        instance.name = name;
        instance.unitClass = unitClass;
        instance.alignment = alignment;
        instance.useable = useable;
        instance.affinity = affinity;
        instance.stats = new Stats(stats, growthRates);

        return instance;
    }
    
    public TextAsset unitFile;

    public enum Sex { NA = 0, M = 1, F = 2}

    public enum Alignment { Player = 1, Allied = 2, Neutral = 4, Enemy = 8 }
    public enum UnitClass {
        Mercenary, Hero, Myrmidon, Swordmaster, Thief, Knight, General, Soldier,
        Fighter, Warrior, Bandit, Pirate, Berserker, Archer, Sniper, Nomad, Nomad_Trooper, Cavalier, Paladin,
        Pegasus_Knight, Falcon_Knight, Wyvern_Rider, Wyvern_Lord, Priest, Bishop, Troubadour, Valkyrie, Mage, Sage,
        Shaman, Druid, Bard, Dancer, Civilian, Transporter, Manakete, King, Demon_Dragon
    }

    public int TotalEXP { get; private set; }
    public int ExpToNextLevel; // { get; private set; }

    private int _exp = 0;
    public int Exp { get { return _exp; } set { TotalEXP = _exp + (stats.level == 1 ? 0 : StatsCalc.TotalExperience(stats.level)); _exp = value; } }

    [Tooltip("SP are required to learn skills and can be earned by performing certain actions.")]
    public int SP;

    public new string name;
    
    public UnitClass unitClass;
    public Sex unitSex;
    public Alignment alignment;
    [EnumFlag(2)] public Weapon.WeaponType useable;
    public Element affinity;
    public Stats stats = new Stats();

    public Inventory inventory = new Inventory();

    public Dictionary<Unit, Relationship> relationships = new Dictionary<Unit, Relationship>();
    public Dictionary<Unit, int> SupportPoints { get; private set; }

    public void AddSupportPoints(Unit u, int value)
    {
        if (SupportPoints == null)
            SupportPoints = new Dictionary<Unit, int>();

        if (!SupportPoints.ContainsKey(u))
        {
            SupportPoints.Add(u, value);
            return;
        }

        SupportPoints[u] += value;
    }
}

public enum Element
{
    None,
    Anima,
    Light,
    Dark
}