using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitOverviewMenu : MonoBehaviour
{
    private Fighter target;

    public TextMeshProUGUI unitName, label;
    public Slider healthBar;
    public Image fill;

    private void Update()
    {
        target = Map.GetCellData(PhaseManager.Crosshair.transform.position).unitInTile;

        transform.GetChild(0).gameObject.SetActive(target != null);
        if (target == null)
            return;

        UpdateData();
    }

    void UpdateData()
    {
        unitName.text = target.Unit.name;
        label.text = target.CurrentHP + "/" + target.Unit.stats.hp;

        healthBar.maxValue = target.Unit.stats.hp;
        healthBar.value = target.CurrentHP;

        float percent = (float)target.CurrentHP / target.Unit.stats.hp;

        if (percent >= 0.6f)
            fill.color = new Color(0, 1, 0, 1);
        else if (percent < 0.6f && percent > 0.4f)
            fill.color = new Color(1, 1, 0, 1);
        else if (percent <= 0.4f)
            fill.color = new Color(1, 0, 0, 1);
    }
}