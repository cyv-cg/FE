using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public class UnitEditorWindow : EditorWindow
{
#if UNITY_EDITOR
    private bool displayStats = true, displayGrowth = true, displayRelations = true, displayInventory = true;

    #region Serialized Properties
    private SerializedProperty nameProperty;

    private SerializedProperty unitFileProperty;

    private SerializedProperty expProperty, spProperty;

    private SerializedProperty classProperty;
    private SerializedProperty sexProperty;
    private SerializedProperty alignmentProperty;

    private SerializedProperty useableWeaponsProperty;

    private SerializedProperty affinityProperty;

    private SerializedProperty statsProperty;
    private SerializedProperty growthRatesProperty;

    private SerializedProperty weaponProperty;

    private SerializedProperty levelProperty;

    private SerializedProperty
        hpProperty,
        strengthProperty,
        magicProperty,
        skillProperty,
        speedProperty,
        luckProperty,
        defenseProperty,
        resistanceProperty,
        constitutionProperty,
        aidProperty,
        movementProperty,
        pccProperty;

    private SerializedProperty
        hpGrowthProperty,
        strengthGrowthProperty,
        magicGrowthProperty,
        skillGrowthProperty,
        speedGrowthProperty,
        luckGrowthProperty,
        defenseGrowthProperty,
        resistanceGrowthProperty;

    private SerializedProperty relationshipsProperty;
    private SerializedProperty inventoryProperty;

    #endregion

    public TextAsset unitFile;
    
    public int TotalEXP { get; private set; }
    public int ExpToNextLevel { get; private set; }

    public int _exp = 0;
    public int Exp { get { return _exp; } set { TotalEXP = _exp + (stats.level == 1 ? 0 : StatsCalc.TotalExperience(stats.level)); _exp = value; } }

    public int SP;

    public new string name;

    public Unit.UnitClass unitClass;
    public Unit.Sex unitSex;
    public Unit.Alignment alignment;
    [EnumFlag(2)] public Weapon.WeaponType useable;
    public Element affinity;
    public Unit.Stats stats;

    public Dictionary<Unit, Unit.Relationship> relationships;
    
    private string[] _items = new string[Inventory.Size];
    private int[] _uses = new int[Inventory.Size];
    public List<Item> inventory = new List<Item>();

    private bool overrideFile = false;

    public ScriptableObject target;
    public SerializedObject serializedObject;

    private Vector2 scrollPos;

    [MenuItem("Editor/Unit")]
    public static void GetWindow()
    {
        GetWindow<UnitEditorWindow>(false, "Unit Editor", true);
    }
    
    public void OnGUI()
    {
        target = this;
        serializedObject = new SerializedObject(target);
        
        Init();

        float width = EditorGUIUtility.currentViewWidth;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        
        serializedObject.Update();

        GUIStyle label = new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        };

        EditorGUI.HelpBox(new Rect(10, 5, width - 25, 40), "", MessageType.None);
        EditorGUI.LabelField(new Rect(0, 5, width, 30), name, label);

        GUILayout.Space(88);
        
        if (unitFile != null)
        {
            overrideFile = EditorGUI.Toggle(new Rect(width - 17, 70, 16, 16), new GUIContent(""), overrideFile);
            EditorGUI.LabelField(new Rect(width - 103, 70, 100, 16), new GUIContent("Override File?"));
        }
        
        EditorGUI.PropertyField(new Rect(14, 50, width * 2 / 3, 16), nameProperty, new GUIContent("Name"));
        EditorGUI.PropertyField(new Rect(14 + width * 3.7f / 5, 50, width / 4.5f, 16), unitFileProperty, new GUIContent(""));
        EditorGUI.LabelField(new Rect(18 + width * 2 / 3, 50, width * 2 / 3, 16), "File");

        EditorGUILayout.PropertyField(classProperty, new GUIContent("Class"));
        EditorGUILayout.PropertyField(sexProperty, new GUIContent("Sex"));
        EditorGUILayout.PropertyField(alignmentProperty, new GUIContent("Alignment"));

        EditorGUILayout.PropertyField(useableWeaponsProperty, new GUIContent("Useable Weapon Types"));

        EditorGUILayout.PropertyField(affinityProperty, new GUIContent("Affinity"));
        
        displayStats = EditorGUILayout.Foldout(displayStats, "Stats");
        if (displayStats)
        {
            int boxHeight = 172;

            {
                EditorGUI.indentLevel++;

                Rect position = GUILayoutUtility.GetLastRect();
                position.x += 4;
                position.y += 5;

                EditorGUI.HelpBox(new Rect(position.x + 8, position.y + 11, width - 31, boxHeight), "", MessageType.None);

                float i = 1;

                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();

                Exp = EditorGUI.IntSlider(new Rect(position.x, position.y + 16 * i, width - 30, 16), new GUIContent("EXP"), Exp, 0, StatsCalc.ExperienceToNextLevel(stats.level + 1));

                EditorGUILayout.EndHorizontal();

                i++;


                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(position.x, position.y + 16 * i, width / 2f - 30, 16), levelProperty);
                EditorGUI.PropertyField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), hpProperty);

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(position.x, position.y + 16 * i, width / 2f - 30, 16), strengthProperty);
                EditorGUI.PropertyField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), magicProperty);

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(position.x, position.y + 16 * i, width / 2f - 30, 16), skillProperty);
                EditorGUI.PropertyField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), speedProperty);

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(position.x, position.y + 16 * i, width / 2f - 30, 16), luckProperty);
                EditorGUI.PropertyField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), defenseProperty);

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(position.x, position.y + 16 * i, width / 2f - 30, 16), resistanceProperty);
                EditorGUI.PropertyField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), constitutionProperty);

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(position.x, position.y + 16 * i, width / 2f - 30, 16), movementProperty);
                EditorGUI.PropertyField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), pccProperty, new GUIContent("Critical Coefficient"));

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(position.x, position.y + 16 * i, width / 2f - 30, 16), aidProperty);
                //EditorGUI.PropertyField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), pccProperty, new GUIContent("Critical Coefficient"));

                EditorGUILayout.EndHorizontal();

                i += 2;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.LabelField(new Rect(position.x, position.y + 16 * i, width / 3.34f - 30, 16), new GUIContent("SP"));
                EditorGUI.PropertyField(new Rect(position.x + width / 5, position.y + 16 * i, width / 3.34f - 30, 16), spProperty, new GUIContent(""));

                EditorGUI.LabelField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), new GUIContent(TotalEXP.ToString()), new GUIStyle { alignment = TextAnchor.MiddleRight });
                EditorGUI.LabelField(new Rect(position.x + width / 2f, position.y + 16 * i, width / 2f - 30, 16), new GUIContent("Total experience: "), new GUIStyle { alignment = TextAnchor.MiddleLeft });

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(boxHeight);
            displayGrowth = EditorGUILayout.Foldout(displayGrowth, "Stat Growth");
            if (displayGrowth)
            {
                Rect pos = GUILayoutUtility.GetLastRect();
                EditorGUI.indentLevel++;
                pos.x += 4;
                pos.y += 5;

                EditorGUI.HelpBox(new Rect(pos.x + 24, pos.y + 11, width - 40, 74), "", MessageType.None);

                float i = 1;

                EditorGUILayout.BeginVertical();


                EditorGUILayout.BeginHorizontal();
                EditorGUI.PropertyField(new Rect(pos.x, pos.y + 16 * i, width / 1.95f - 30, 16), hpGrowthProperty, new GUIContent("HP Growth"));
                EditorGUI.PropertyField(new Rect(pos.x + width / 2f, pos.y + 16 * i, width / 1.95f - 30, 16), strengthGrowthProperty, new GUIContent("Strength Growth"));

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(pos.x, pos.y + 16 * i, width / 1.95f - 30, 16), magicGrowthProperty, new GUIContent("Magic Growth"));
                EditorGUI.PropertyField(new Rect(pos.x + width / 2f, pos.y + 16 * i, width / 1.95f - 30, 16), skillGrowthProperty, new GUIContent("Skill Growth"));

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(pos.x, pos.y + 16 * i, width / 1.95f - 30, 16), speedGrowthProperty, new GUIContent("Speed Growth"));
                EditorGUI.PropertyField(new Rect(pos.x + width / 2f, pos.y + 16 * i, width / 1.95f - 30, 16), luckGrowthProperty, new GUIContent("Luck Growth"));

                EditorGUILayout.EndHorizontal();

                i++;

                EditorGUILayout.BeginHorizontal();

                EditorGUI.PropertyField(new Rect(pos.x, pos.y + 16 * i, width / 1.95f - 30, 16), defenseGrowthProperty, new GUIContent("Defense Growth"));
                EditorGUI.PropertyField(new Rect(pos.x + width / 2f, pos.y + 16 * i, width / 1.95f - 30, 16), resistanceGrowthProperty, new GUIContent("Resistance Growth"));

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
                GUILayout.Space(74);
            }

            EditorGUI.indentLevel--;
        }

        displayRelations = EditorGUILayout.Foldout(displayRelations, "Relationships");
        if (displayRelations && relationships != null)
        {
            int h = 0;

            EditorGUI.indentLevel++;

            Rect position = GUILayoutUtility.GetLastRect();
            position.x += 4;
            position.y += 5;

            int height = relationships.Count * 16 + 23;
            GUILayout.Space(height);
            EditorGUI.HelpBox(new Rect(position.x + 8, position.y + 11, width - 31, height), "", MessageType.None);

            if (relationships != null)
            {
                Unit[] keys = new Unit[relationships.Count];
                Unit.Relationship[] values = new Unit.Relationship[relationships.Count];

                relationships.Keys.CopyTo(keys, 0);
                relationships.Values.CopyTo(values, 0);

                for (int i = 0; i < keys.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    
                    keys[i].name = EditorGUI.TextField(new Rect(position.x, position.y + 16 + 16 * h, width / 2, 16), new GUIContent((i + 1).ToString()), keys[i].name);
                    values[i].rank = (Unit.Relationship.Rank)EditorGUI.EnumPopup(new Rect(position.x + width / 1.75f, position.y + 16 + 16 * h, width / 8f, 16), new GUIContent(""), values[i].rank);
                    EditorGUI.LabelField(new Rect(position.x + width / 2, position.y + 16 + 16 * h, width / 3.25f, 16), new GUIContent("Rank"));

                    bool remove = false;
                    remove = EditorGUI.Foldout(new Rect(width - width / 4, position.y + 16 + 16 * h, width / 2, 16), remove, new GUIContent("Remove"));
                    if (remove)
                        relationships.Remove(keys[i]);

                    GUILayout.EndHorizontal();

                    h++;
                }
            }

            //position = GUILayoutUtility.GetLastRect();

            bool add = false;
            add = EditorGUI.Foldout(new Rect(position.x, position.y + 16 + 16 * h, width / 2, 16), add, new GUIContent("Add"));
            if (add)
                relationships.Add(CreateInstance<Unit>(), new Unit.Relationship(Unit.Relationship.Rank.None));

            EditorGUI.indentLevel--;
        }

        displayInventory = EditorGUILayout.Foldout(displayInventory, "Inventory");
        if (displayInventory)
        {
            EditorGUI.indentLevel++;

            Rect position = GUILayoutUtility.GetLastRect();
            position.x += 4;
            position.y += 5;

            int height = Inventory.Size * 16 + 18;
            GUILayout.Space(height);
            EditorGUI.HelpBox(new Rect(position.x + 8, position.y + 11, width / 2, height), "", MessageType.None);

            position.y += 20;

            int w = 295 - 36;
            for (int i = 0; i < Inventory.Size; i++)
            {
                Item item = (Item)AssetDatabase.LoadAssetAtPath("Assets/Data/Items/" + _items[i] + ".asset", typeof(Item));
                if (item != null)
                    EditorGUI.DrawPreviewTexture(new Rect(position.x + 16, position.y + 16 * i, 16, 16), DataManager.ConvertSpriteToTexture(item.icon));

                int _width = item != null && item.unbreakable ? w : w - 35;

                _items[i] = EditorGUI.TextField(new Rect(width / 2 - w, position.y + 16 * i, _width, 16), _items[i]);
                if (item != null && !item.unbreakable)
                {
                    _uses[i] = EditorGUI.IntField(new Rect(width / 2 - 48, position.y + 16 * i, 48, 16), _uses[i]);
                }
            }
        }

        GUILayout.BeginHorizontal();

        Unit control = CreateInstance<Unit>(), unit = CreateInstance<Unit>();
        unit.name = name;
        unit.unitClass = unitClass;
        unit.unitSex = unitSex;
        unit.alignment = alignment;
        unit.useable = useable;
        unit.affinity = affinity;
        unit.stats = stats;
        unit.stats.growthRates = stats.growthRates;
        unit.relationships = relationships;
        
        for (int i = 0; i < _items.Length; i++)
        {
            Item item = (Item)AssetDatabase.LoadAssetAtPath("Assets/Data/Items/" + _items[i] + ".asset", typeof(Item));

            if (item != null && !item.unbreakable)
                item.uses = _uses[i];
            if (item != null)
                unit.inventory.Add(item);
        }

        if (name != null && name != "" && GUILayout.Button("Export to file"))
            DataManager.UnitToFile(unit, overrideFile);
        if (unitFile != null && GUILayout.Button("Read from file"))
        {
            control = DataManager.FileToUnit(unitFile, true, true);

            name = control.name;
            unitClass = control.unitClass;
            unitSex = control.unitSex;
            alignment = control.alignment;
            useable = control.useable;
            affinity = control.affinity;
            relationships = control.relationships;
            stats = new Unit.Stats(control.stats, control.stats.growthRates);
            inventory = control.inventory.Items;

            if (control.inventory.Items == null)
                control.inventory.Items = new List<Item>();

            for (int i = 0; i < _items.Length; i++)
            {
                _items[i] = "";
                _uses[i] = 0;
            }

            for (int i = 0; i < control.inventory.Items.Count; i++)
                if (control.inventory.Items[i] != null)
                {
                    _items[i] = control.inventory.Items[i].name;
                    if (!control.inventory.Items[i].unbreakable)
                        _uses[i] = control.inventory.Items[i].uses;
                }
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    void Init()
    {
        nameProperty = serializedObject.FindProperty("name");

        unitFileProperty = serializedObject.FindProperty("unitFile");

        expProperty = serializedObject.FindProperty("EXP");
        spProperty = serializedObject.FindProperty("SP");

        classProperty = serializedObject.FindProperty("unitClass");
        sexProperty = serializedObject.FindProperty("unitSex");
        alignmentProperty = serializedObject.FindProperty("alignment");

        useableWeaponsProperty = serializedObject.FindProperty("useable");

        affinityProperty = serializedObject.FindProperty("affinity");

        weaponProperty = serializedObject.FindProperty("weapon");

        statsProperty = serializedObject.FindProperty("stats");

        relationshipsProperty = serializedObject.FindProperty("relationships");
        inventoryProperty = serializedObject.FindProperty("inventory");

        growthRatesProperty = statsProperty.FindPropertyRelative("growthRates");

        levelProperty = statsProperty.FindPropertyRelative("level");

        hpProperty = statsProperty.FindPropertyRelative("hp");
        strengthProperty = statsProperty.FindPropertyRelative("strength");
        magicProperty = statsProperty.FindPropertyRelative("magic");
        skillProperty = statsProperty.FindPropertyRelative("skill");
        speedProperty = statsProperty.FindPropertyRelative("speed");
        luckProperty = statsProperty.FindPropertyRelative("luck");
        defenseProperty = statsProperty.FindPropertyRelative("defense");
        resistanceProperty = statsProperty.FindPropertyRelative("resistance");
        constitutionProperty = statsProperty.FindPropertyRelative("constitution");
        movementProperty = statsProperty.FindPropertyRelative("movement");
        aidProperty = statsProperty.FindPropertyRelative("aid");
        pccProperty = statsProperty.FindPropertyRelative("pursuitCriticalCoefficient");

        hpGrowthProperty = growthRatesProperty.FindPropertyRelative("hp");
        strengthGrowthProperty = growthRatesProperty.FindPropertyRelative("strength");
        magicGrowthProperty = growthRatesProperty.FindPropertyRelative("magic");
        skillGrowthProperty = growthRatesProperty.FindPropertyRelative("skill");
        speedGrowthProperty = growthRatesProperty.FindPropertyRelative("speed");
        luckGrowthProperty = growthRatesProperty.FindPropertyRelative("luck");
        defenseGrowthProperty = growthRatesProperty.FindPropertyRelative("defense");
        resistanceGrowthProperty = growthRatesProperty.FindPropertyRelative("resistance");
    }
#endif
}