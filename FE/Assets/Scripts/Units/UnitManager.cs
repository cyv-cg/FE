using UnityEngine;
using System.Collections.Generic;

public static class UnitManager
{
    public static List<Fighter> Players { get; private set; }
    public static List<Fighter> Allies { get; private set; }
    public static List<Fighter> Enemies { get; private set; }
    public static List<Fighter> Neutrals { get; private set;}

    public static void Init()
    {
        Fighter[] units = Object.FindObjectsOfType<Fighter>();
        Players = new List<Fighter>();
        Allies = new List<Fighter>();
        Enemies = new List<Fighter>();
        Neutrals = new List<Fighter>();

        foreach (Fighter f in units)
        {
            if (f.Unit.alignment == Unit.Alignment.Player)
                Players.Add(f);
            else if (f.Unit.alignment == Unit.Alignment.Allied)
                Allies.Add(f);
            else if (f.Unit.alignment == Unit.Alignment.Neutral)
                Neutrals.Add(f);
            else if (f.Unit.alignment == Unit.Alignment.Enemy)
                Enemies.Add(f);
        }

        PhaseManager.OnPhaseEnd += OnPhaseEnd;
    }

    private static void OnPhaseEnd()
    {
        for (int i = 0; i < Players.Count; i++)
            if (Players[i] == null)
                Players.RemoveAt(i);

        for (int i = 0; i < Allies.Count; i++)
            if (Allies[i] == null)
                Allies.RemoveAt(i);

        for (int i = 0; i < Enemies.Count; i++)
            if (Enemies[i] == null)
                Enemies.RemoveAt(i);

        for (int i = 0; i < Neutrals.Count; i++)
            if (Neutrals[i] == null)
                Neutrals.RemoveAt(i);
    }

    public static void Kill(Fighter f)
    {
        if (f.Unit.alignment == Unit.Alignment.Player && PhaseManager.instance.currentPhase != PhaseManager.Phase.Player)
            Players.Remove(f);
        if (f.Unit.alignment == Unit.Alignment.Allied && PhaseManager.instance.currentPhase != PhaseManager.Phase.Allied)
            Allies.Remove(f);
        if (f.Unit.alignment == Unit.Alignment.Enemy && PhaseManager.instance.currentPhase != PhaseManager.Phase.Enemy)
            Enemies.Remove(f);
        if (f.Unit.alignment == Unit.Alignment.Neutral && PhaseManager.instance.currentPhase != PhaseManager.Phase.Neutral)
            Neutrals.Remove(f);

        Map.UnitTile(f).unitInTile = null;
        if (f != null)
        {
            if (GameSettings.DoBattleAnimation)
                PhaseManager.instance.StartCoroutine(_Kill(f));
            else
                Object.Destroy(f.gameObject);
        }
    }
    private static System.Collections.IEnumerator _Kill(Fighter f)
    {
        SpriteRenderer sr = f.GetComponentInChildren<SpriteRenderer>();
        sr.material.shader = Shader.Find("GUI/Text Shader");
        sr.color = Color.white;

        float a = 1;

        while (a > 0)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
            a -= Time.deltaTime;
            yield return null;
        }

        Object.Destroy(f.gameObject);
    }

    public static void EndAll()
    {
        foreach (Fighter f in Players)
            f.EndTurn();
    }

    public static void OnUnitEnd(Fighter fighter)
    {
        bool end = true;

        List<Fighter> list = null;

        if (fighter.Unit.alignment == Unit.Alignment.Player)
            list = Players;
        else if (fighter.Unit.alignment == Unit.Alignment.Allied)
            list = Allies;
        else if (fighter.Unit.alignment == Unit.Alignment.Enemy)
            list = Enemies;
        else if (fighter.Unit.alignment == Unit.Alignment.Neutral)
            list = Neutrals;

        if (list.Count > 0)
            foreach (Fighter f in list)
            {
                if (!f.TurnOver)
                {
                    end = false;
                    return;
                }
            }
        
        if (end)
            PhaseManager.EndPhase();
    }
}