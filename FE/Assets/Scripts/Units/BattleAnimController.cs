using UnityEngine;
using System.Collections;

public class BattleAnimController : MonoBehaviour
{
    public Unit.Alignment alignment = Unit.Alignment.Neutral;
    public Fighter linked;
    public Animator anim;
    
    private static bool wait = true;
    public static bool IsAnimating { get; private set; }
    
    private void Start()
    {
        OnValidate();
        anim = GetComponent<Animator>();
    }
    private void OnValidate()
    {
        ColorManager.SetColor(alignment, transform);
    }
    
    public Animator Anim()
    {
        try
        {
            return GetComponent<Animator>();
        }
        catch
        {
            Debug.LogError("The unit being referenced is null");
            return null;
        }
    }

    public static IEnumerator WaitForBattleAnimation(BattleManager.Offensive atk)
    {
        IsAnimating = true;

        Animator animator1 = !atk.IsCounter ? BattleScreen.Attacker.Anim() : BattleScreen.Defender.Anim();
        Animator animator2 = !atk.IsCounter ? BattleScreen.Defender.Anim() : BattleScreen.Attacker.Anim();

        animator1.SetBool("critical", atk.Crit);
        animator1.SetBool("ranged", !atk.IsPhysical);

        animator1.SetTrigger("attack");

        while (wait)
            yield return null;

        if (!atk.Hit)
            animator2.SetTrigger("evade");

        if (atk.Hit && atk.Defender.CurrentHP - atk.Damage <= 0)
            yield return BattleManager.instance.StartCoroutine(FadeOut(animator2.transform.parent.gameObject));

        wait = true;
    }

    private static IEnumerator FadeOut(GameObject go)
    {
        int i = 0;

        while (i < 3)
        {
            SetWhite();
            yield return new WaitForSeconds(0.1f);
            SetNormal();
            yield return new WaitForSeconds(0.1f);

            i++;
        }

        SetWhite();

        SpriteRenderer[] srs = BattleScreen.Defender.GetComponentsInChildren<SpriteRenderer>();
        Color[] colors = new Color[srs.Length];

        for (int c = 0; c < colors.Length; c++)
            colors[c] = srs[c].color;

        float a = 1;
        while (a > 0)
        {
            for (int c = 0; c < srs.Length; c++)
            {
                srs[c].color = new Color(colors[c].r, colors[c].g, colors[c].b, a);
                a -= Time.deltaTime;
            }

            yield return null;
        }

        Destroy(go);
    }

    static void SetWhite()
    {
        SpriteRenderer[] srs = BattleScreen.Defender.GetComponentsInChildren<SpriteRenderer>();
        Color[] colors = new Color[srs.Length];

        for (int c = 0; c < colors.Length; c++)
            colors[c] = srs[c].color;

        foreach (SpriteRenderer sr in srs)
        {
            sr.material.shader = Shader.Find("GUI/Text Shader");
            sr.color = Color.white;
        }
    }
    static void SetNormal()
    {
        SpriteRenderer[] srs = BattleScreen.Defender.GetComponentsInChildren<SpriteRenderer>();
        Color[] colors = new Color[srs.Length];

        foreach (SpriteRenderer sr in srs)
            sr.material.shader = Shader.Find("Sprites/Default");

        for (int c = 0; c < colors.Length; c++)
            srs[c].color = colors[c];
    }

    public void DoDamage()
    {
        wait = false;
    }

    public void End()
    {
        IsAnimating = false;
    }
}