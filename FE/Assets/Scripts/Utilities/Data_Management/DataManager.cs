using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataManager
{
    public static SaveData SaveData { get; private set; }

    public static readonly bool WriteToPlainText = true;

    public static Action OnSave;
    public static Action OnLoad;

    public static Dictionary<string, int> Ints { get; private set; }
    public static Dictionary<string, float> Floats { get; private set; }
    public static Dictionary<string, bool> Bools { get; private set; }
    public static Dictionary<string, string> Strings { get; private set; }

    public static Dictionary<string, Unit> Units { get; private set; }
    public static Dictionary<string, Item> Items { get; private set; }

    private static readonly string dataFileType = ".save";
    public static string saveName = "Save00";
    
    public static string Now()
    {
        string s = DateTime.Now.Second.ToString();
        string min = DateTime.Now.Minute.ToString();
        string h = DateTime.Now.Hour.ToString();
        string d = DateTime.Now.Day.ToString();
        string mon = DateTime.Now.Month.ToString();
        string y = DateTime.Now.Year.ToString();

        return s + "-" + min + "-" + h + "_" + d + "-" + mon + "-" + y;
    }

    public static readonly string SaveDirectory = Application.dataPath + "/Saves"; 
    public static readonly string SaveFile = SaveDirectory + "/" + saveName + dataFileType;
    public static readonly string SaveFilePlainText = SaveDirectory + "/" + saveName + dataFileType + ".plain.txt";

    private static readonly string DataPath = Application.dataPath + "/Data";
    private static readonly string UnitPath = DataPath + "/Units";
    private static readonly string UnitExtention = ".txt";

    #region Data Fetching
    public static void SetInt(string key, int value)
    {
        if (Ints == null)
            Ints = new Dictionary<string, int>();

        Ints[key] = value;
    }
    public static void IncrementInt(string key, int incrementation)
    {
        if (Ints == null)
            Ints = new Dictionary<string, int>();

        Ints[key] = GetInt(key) + incrementation;
    }
    public static int GetInt(string key)
    {
        if (Ints == null)
            Ints = new Dictionary<string, int>();

        return (Ints.ContainsKey(key)) ? Ints[key] : 0;
    }

    public static void SetFloat(string key, float value)
    {
        if (Floats == null)
            Floats = new Dictionary<string, float>();

        Floats[key] = value;
    }
    public static void IncrementFloat(string key, float incrementation)
    {
        if (Floats == null)
            Floats = new Dictionary<string, float>();

        Floats[key] = GetFloat(key) + incrementation;
    }
    public static float GetFloat(string key)
    {
        if (Floats == null)
            Floats = new Dictionary<string, float>();

        return (Floats.ContainsKey(key)) ? Floats[key] : 0f;
    }

    public static void SetBool(string key, bool value)
    {
        if (Bools == null)
            Bools = new Dictionary<string, bool>();

        Bools[key] = value;
    }
    public static void ToggleBool(string key)
    {
        if (Bools == null)
            Bools = new Dictionary<string, bool>();

        Bools[key] = !GetBool(key);
    }
    public static bool GetBool(string key)
    {
        if (Bools == null)
            Bools = new Dictionary<string, bool>();

        return (Bools.ContainsKey(key)) ? Bools[key] : false;
    }

    public static void SetString(string key, string value)
    {
        if (Strings == null)
            Strings = new Dictionary<string, string>();

        Strings[key] = value;
    }
    public static string GetString(string key)
    {
        if (Strings == null)
            Strings = new Dictionary<string, string>();

        return (Strings.ContainsKey(key)) ? Strings[key] : "";
    }

    public static void SetUnit(string key, Unit value)
    {
        if (Units == null)
            Units = new Dictionary<string, Unit>();

        Units[key] = value;
    }
    public static Unit GetUnit(string key)
    {
        if (Units == null)
            Units = new Dictionary<string, Unit>();

        return Units.ContainsKey(key) ? Units[key] : null;
    }

    public static void SetItem(string key, Item value)
    {
        if (Items == null)
            Items = new Dictionary<string, Item>();

        Items[key] = value;
    }
    public static Item GetItem(string key)
    {
        if (Items == null)
            Items = new Dictionary<string, Item>();

        return Items.ContainsKey(key) ? Items[key] : null;
    }
    #endregion

    #region Save/Load
    public static void Save(string saveName = "Save00")
    {
        #region Initialize
        DataManager.saveName = saveName;

        if (!Directory.Exists(DataManager.SaveDirectory))
            Directory.CreateDirectory(DataManager.SaveDirectory);

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(DataManager.SaveFile);
        SaveData = new SaveData();
        #endregion

        OnSave?.Invoke();

        SaveData.ints = Ints;
        SaveData.floats = Floats;
        SaveData.bools = Bools;
        SaveData.strings = Strings;

        SaveData.units = Units;
        SaveData.items = Items;

        if (WriteToPlainText)
            ExportAsPlainText();

        #region Finalize
        bf.Serialize(file, SaveData);
        file.Close();

        Debug.Log("Saved Game: " + file.Name);
        #endregion
    }
    public static void Load(string saveName = "Save00")
    {
        #region Initialize
        string saveFile = SaveDirectory + "/" + saveName + dataFileType;

        if (!File.Exists(saveFile))
            return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(saveFile, FileMode.Open);
        SaveData = (SaveData)bf.Deserialize(file);
        #endregion

        OnLoad?.Invoke();

        Ints = SaveData.ints;
        Floats = SaveData.floats;
        Bools = SaveData.bools;
        Strings = SaveData.strings;

        Units = SaveData.units;
        Items = SaveData.items;

        #region Finalize
        file.Close();

        Debug.Log("Loaded Game: " + file.Name);
        #endregion
    }

    public static void ExportAsPlainText()
    {
        string[] contents = new string[0];
        List<string> data = new List<string>();

        if (Ints != null)
            foreach (string s in Ints.Keys)
                data.Add(s + " | " + Ints[s].ToString());
        if (Floats != null)
            foreach (string s in Floats.Keys)
                data.Add(s + " | " + Floats[s].ToString());
        if (Bools != null)
            foreach (string s in Bools.Keys)
                data.Add(s + " | " + Bools[s].ToString());
        if (Strings != null)
            foreach (string s in Strings.Keys)
                data.Add(s + " | " + Strings[s]);

        if (Units != null)
            foreach (string s in Units.Keys)
                data.Add(s + " | " + Units[s]);
        if (Items != null)
            foreach (string s in Items.Keys)
                data.Add(s + " | " + Items[s]);

        contents = data.ToArray();

        File.WriteAllLines(DataManager.SaveFilePlainText, contents, System.Text.Encoding.Unicode);
    }
    #endregion

    #region Unit Read/Write
    public static TextAsset UnitToFile(Unit u, bool overrideFile, bool log = true)
    {
        if (u.name == null || u.name == "")
        {
            return null;
        }
        
        string fileName = "";

        if (overrideFile && u.unitFile != null)
            fileName = u.unitFile.name;
        else fileName = "Unit_" + u.name + UnitExtention;

        if (!Directory.Exists(DataPath))
            Directory.CreateDirectory(DataPath);
        
        string weapons = "", relations = " ", items = "";
        foreach (Weapon.WeaponType w in Enum.GetValues(typeof(Weapon.WeaponType)))
        {
            Weapon weapon = ScriptableObject.CreateInstance<Weapon>();
            weapon.type = w;

            if (weapon.IsUseable(u))
                weapons += w.ToString() + ",";
        }

        if (u.relationships != null)
        {
            Unit[] keys = new Unit[u.relationships.Count];
            u.relationships.Keys.CopyTo(keys, 0);
            for (int i = 0; i < u.relationships.Count; i++)
            {
                if (keys[i].name == u.name || keys[i].name == "" || keys[i].name == null || u.relationships[keys[i]].rank == Unit.Relationship.Rank.None)
                    continue;

                relations += keys[i].name + ":" + u.relationships[keys[i]].rank.ToString() + ",";
            }
        }

        if (u.inventory.Items != null)
        {
            foreach (Item i in u.inventory.Items)
            {
                items += i.name + (i.unbreakable ? "" : ":" + i.uses) + ",";
            }
        }

        weapons = weapons.Remove(weapons.Length - 1);
        relations = relations.Remove(relations.Length - 1);
        relations = relations.Replace(" ", "");
        if (items.Length > 0)
            items = items.Remove(items.Length - 1);

        string[] contents = new string[0];
        List<string> data = new List<string>
        {
            "Name=" + u.name + "\n",
            "Class=" + u.unitClass + "\n",
            "Sex=" + u.unitSex.ToString() + " //Sex is only used to determine which sprites to use, and has no effect on stats \n",
            "Alignment=" + u.alignment + "\n",
            "Useable_Weapon_Types=" + weapons + "\n",
            "Affinity=" + u.affinity + "\n",
            "   Stats{\n",
            "Level=" + u.stats.level + "\n",
            "Hp=" + u.stats.hp + "\n",
            "Strength=" + u.stats.strength + "\n",
            "Magic=" + u.stats.magic + "\n",
            "Skill=" + u.stats.skill + "\n",
            "Speed=" + u.stats.speed + "\n",
            "Luck=" + u.stats.luck + "\n",
            "Defense=" + u.stats.defense + "\n",
            "Resistance=" + u.stats.resistance + "\n",
            "Constitution=" + u.stats.constitution + "\n",
            "Movement=" + u.stats.movement + "\n",
            "Aid=" + u.stats.aid + "\n",
            "EXP=" + u.Exp + "\n",
            "SP=" + u.SP + "\n",
            "PursuitCriticalCoefficient=" + u.stats.pursuitCriticalCoefficient + "\n",
            "   Stat_Growth{\n",
            "HpGrowth=" + u.stats.growthRates.hp + "\n",
            "StrengthGrowth=" + u.stats.growthRates.strength + "\n",
            "MagicGrowth=" + u.stats.growthRates.magic + "\n",
            "SkillGrowth=" + u.stats.growthRates.skill + "\n",
            "SpeedGrowth=" + u.stats.growthRates.speed + "\n",
            "LuckGrowth=" + u.stats.growthRates.luck + "\n",
            "DefenseGrowth=" + u.stats.growthRates.defense + "\n",
            "ResistanceGrowth=" + u.stats.growthRates.resistance + "\n",
            "   }\n",
            "}\n",
            "Relationships=" + relations + "\n",
            "Inventory=" + items + "\n"
        };
        
        contents = data.ToArray();
        File.WriteAllLines(UnitPath + "/" + fileName, contents, System.Text.Encoding.Unicode);

        if (log)
            Debug.Log("Wrote unit to: " + DataPath + "/" + fileName);

        return (TextAsset)UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Data/Units/" + fileName + UnitExtention, typeof(TextAsset));
    }
    public static Unit FileToUnit(TextAsset t, bool loadRelations, bool log = false)
    {
        Unit output = ScriptableObject.CreateInstance<Unit>();

        string[] lines = t.text.Split('\n');

        string name = "";
        Unit.UnitClass unitClass = new Unit.UnitClass();
        Unit.Sex sex = 0;
        Unit.Alignment alignment = Unit.Alignment.Player;
        Weapon.WeaponType useable = Weapon.WeaponType.Sword;
        Element affinity = Element.None;

        Dictionary<Unit, Unit.Relationship> relationships = new Dictionary<Unit, Unit.Relationship>();
        List<Item> inventory = new List<Item>();

        Unit.Stats stats = new Unit.Stats();
        Unit.Stats.GrowthRates growth = new Unit.Stats.GrowthRates();

        int aid = 0, exp = 0, sp = 0;
        float pcc = 0;

        int level = 0, hp = 0, strength = 0, magic = 0, skill = 0, speed = 0, luck = 0, defense = 0, resistance = 0, constitution = 0, movement = 0;
        float hpGrowth = 0, strengthGrowth = 0, magicGrowth = 0, skillGrowth = 0, speedGrowth = 0, luckGrowth = 0, defenseGrowth = 0, resistanceGrowth = 0;
        
        for (int i = 0; i < lines.Length; i++)
        {
            string[] a = lines[i].Split('=');
            string value = a[a.Length - 1];

            if (a[0] == "Name")
                name = value;
            else if (a[0] == "Class")
                Enum.TryParse(value, out unitClass);
            else if (a[0] == "Sex")
                Enum.TryParse(value, out sex);
            else if (a[0] == "Alignment")
                Enum.TryParse(value, out alignment);
            else if (a[0] == "Useable_Weapon_Types")
            {
                string[] weapons = value.Split(',');
                int val = 0;

                foreach (string s in weapons)
                {
                    Enum.TryParse(s, out Weapon.WeaponType w);
                    val += (int)w;
                }

                useable = (Weapon.WeaponType)val;
            }
            else if (a[0] == "Affinity")
                Enum.TryParse(value, out affinity);

            else if (a[0] == "Aid")
                int.TryParse(value, out aid);
            else if (a[0] == "EXP")
                int.TryParse(value, out exp);
            else if (a[0] == "PursuitCriticalCoefficient")
                float.TryParse(value, out pcc);
            #region Stats
            else if (a[0] == "Level")
                int.TryParse(value, out level);
            else if (a[0] == "Hp")
                int.TryParse(value, out hp);
            else if (a[0] == "Strength")
                int.TryParse(value, out strength);
            else if (a[0] == "Magic")
                int.TryParse(value, out magic);
            else if (a[0] == "Skill")
                int.TryParse(value, out skill);
            else if (a[0] == "Speed")
                int.TryParse(value, out speed);
            else if (a[0] == "Luck")
                int.TryParse(value, out luck);
            else if (a[0] == "Defense")
                int.TryParse(value, out defense);
            else if (a[0] == "Resistance")
                int.TryParse(value, out resistance);
            else if (a[0] == "Constitution")
                int.TryParse(value, out constitution);
            else if (a[0] == "Movement")
                int.TryParse(value, out movement);

            else if (a[0] == "HpGrowth")
                float.TryParse(value, out hpGrowth);
            else if (a[0] == "StrengthGrowth")
                float.TryParse(value, out strengthGrowth);
            else if (a[0] == "MagicGrowth")
                float.TryParse(value, out magicGrowth);
            else if (a[0] == "SkillGrowth")
                float.TryParse(value, out skillGrowth);
            else if (a[0] == "SpeedGrowth")
                float.TryParse(value, out speedGrowth);
            else if (a[0] == "LuckGrowth")
                float.TryParse(value, out luckGrowth);
            else if (a[0] == "DefenseGrowth")
                float.TryParse(value, out defenseGrowth);
            else if (a[0] == "ResistanceGrowth")
                float.TryParse(value, out resistanceGrowth);
            #endregion
            else if (a[0] == "Relationships" && loadRelations)
            {
                string[] values = value.Split(',');

                foreach (string s in values)
                {
                    string[] vs = s.Split(':');
                    if (vs.Length < 2)
                        continue;

                    vs[0] = vs[0].Replace(" ", "");

                    string path = "Assets/Data/Units" + "/Unit_" + vs[0] + UnitExtention;
                    Enum.TryParse(vs[1], out Unit.Relationship.Rank rank);
                    TextAsset file = (TextAsset)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
                    Unit unit = FileToUnit(file, false);

                    relationships.Add(unit, new Unit.Relationship(rank));
                }
            }
            else if (a[0] == "Inventory")
            {
                string[] values = value.Split(',');

                foreach (string s in values)
                {
                    string[] vs = s.Split(':');
                    int uses = -1;

                    if (vs.Length > 1)
                        int.TryParse(vs[1], out uses);

                    string path = "Assets/Data/Items/" + vs[0] + ".asset";
                    Item _item = (Item)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Item));

                    if (uses > -1)
                        _item.uses = uses;

                    if (_item != null)
                        inventory.Add(_item.CreateInstance());
                }
            }
        }

        growth = new Unit.Stats.GrowthRates(hpGrowth, strengthGrowth, magicGrowth, skillGrowth, speedGrowth, luckGrowth, defenseGrowth, resistanceGrowth);
        stats = new Unit.Stats(strength, magic, skill, speed, luck, defense, resistance, constitution, movement, aid, pcc, growth);

        output.Exp = exp;
        output.SP = sp;

        output.name = name;
        output.unitClass = unitClass;
        output.unitSex = sex;
        output.alignment = alignment;
        output.useable = useable;
        output.affinity = affinity;
        output.stats = stats;
        output.stats.growthRates = growth;

        output.stats.level = level;
        output.stats.hp = hp;

        output.relationships = relationships;

        foreach (Item i in inventory)
            output.inventory.Add(i);

        output.unitFile = t;

        if (log)
            Debug.Log("Loaded from file " + t.name);
        
        return output;
    }
    #endregion

    public static Texture2D ConvertSpriteToTexture(Sprite sprite)
    {
        try
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] colors = newText.GetPixels();
                Color[] newColors = sprite.texture.GetPixels((int)System.Math.Ceiling(sprite.textureRect.x),
                                                             (int)System.Math.Ceiling(sprite.textureRect.y),
                                                             (int)System.Math.Ceiling(sprite.textureRect.width),
                                                             (int)System.Math.Ceiling(sprite.textureRect.height));
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }
        catch
        {
            return sprite.texture;
        }
    }
    public static Sprite ConvertTexureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
    }
}

[Serializable]
public class SaveData
{
    public Dictionary<string, int> ints;
    public Dictionary<string, float> floats;
    public Dictionary<string, bool> bools;
    public Dictionary<string, string> strings;

    public Dictionary<string, Unit> units;
    public Dictionary<string, Item> items;
}