using UnityEngine;
using TMPro;

public class TileOverviewMenu : MonoBehaviour
{
    private Cell target;

    public TextMeshProUGUI tile, avoid, defense;

    private void Update()
    {
        target = Map.GetCellData(PhaseManager.Crosshair.transform.position);
        
        transform.GetChild(0).gameObject.SetActive(target != null);
        if (target == null)
            return;

        tile.text = target.terrain.ToString();

        avoid.text = "AVO: " + target.terrainBonus.avoid;
        defense.text = "DEF: " + target.terrainBonus.defense;
    }
}