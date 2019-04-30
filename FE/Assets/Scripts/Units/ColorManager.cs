using UnityEngine;

public static class ColorManager
{
    public static readonly Color[] Player = new Color[5]
    {
        new Color(1, 1, 1),
        new Color(56f / 225f, 56f / 255f, 144f / 255f),
        new Color(56f / 255f, 80f / 255f, 224f / 255f),
        new Color(40f / 255f, 160f / 255f, 248f / 255f),
        new Color(24f / 255f, 240f / 255f, 248f / 255f)
    };
    public static readonly Color[] Ally = new Color[5]
    {
        new Color(1, 1, 1),
        new Color(32f / 255f, 80f / 255f, 16f / 255f),
        new Color(8f / 255f, 144f / 255f, 0),
        new Color(24f / 255f, 208f / 255f, 16f / 255f),
        new Color(80f / 255f, 248f / 255f, 56f / 255f)
    };
    public static readonly Color[] Enemy = new Color[5]
    {
        new Color(1, 1, 1),
        new Color(0.3529412f, 0, 0.03137255f),
        new Color(0.7098039f, 0.1921569f, 0.1294118f),
        new Color(0.9686275f, 0.1921569f, 0.1607843f),
        new Color(1, 0.6784314f, 0.4509804f)
    };
    public static readonly Color[] Neutral = new Color[5]
    {
        new Color(1, 1, 1),
        new Color(88f / 255f, 64f / 255f, 96f / 255f),
        new Color(128f / 255f, 96f / 255f, 144f / 255f),
        new Color(184f / 255f, 152f / 255f, 224f / 255f),
        new Color(208f / 255f, 192f / 255f, 248f / 255f)
    };

    public static void SetColor (Unit.Alignment alignment, Transform spriteParent)
    {
        for (int i = 0; i < 5; i++)
        {
            if (spriteParent.childCount <= i)
                break;

            Color c = new Color();
            if (alignment == Unit.Alignment.Player)
                c = Player[i];
            else if (alignment == Unit.Alignment.Allied)
                c = Ally[i];
            else if (alignment == Unit.Alignment.Enemy)
                c = Enemy[i];
            else if (alignment == Unit.Alignment.Neutral)
                c = Neutral[i];

            spriteParent.GetChild(i).GetComponent<SpriteRenderer>().color = c;
        }
    }
}