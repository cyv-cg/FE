using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Default")]
public class Item : ScriptableObject
{
    public Sprite icon;

    public int uses = 35;
    public bool unbreakable = true;
    public int cost = 480;

    public bool consumable = false;

    public static Item Blank()
    {
        string path = "Assets/Data/Items/" + "__blank" + ".asset";
        return (Item)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Item));
    }

    public virtual Item CreateInstance()
    {
        return this;
    }

    public virtual bool IsUseable(Fighter f)
    {
        return consumable;
    }

    public virtual void Use(Fighter f)
    {
        throw new System.NotImplementedException();
    }
}