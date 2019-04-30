using System.Collections;

public class SentryAI : AttackBaseAI
{
    public override IEnumerator DoTurn()
    {
        FindTarget(AttackArea.ToArray());
        ChooseWeapon();
        Attack();

        yield return null;
    }

    protected override void OnEnable()
    {
        PhaseManager.OnPhaseStart += OnPhaseStart;
    }
    protected override void OnDisable()
    {
        PhaseManager.OnPhaseStart -= OnPhaseStart;
    }

    protected override void OnPhaseStart()
    {
        SetMoveArea(new Cell[1] { Map.UnitTile(Fighter) });
        TargetData();
    }

    public override Cell[] GetMoveArea()
    {
        return new Cell[1] { Map.UnitTile(Fighter) };
    }
}