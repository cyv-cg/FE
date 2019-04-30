using UnityEngine;

public class TileHighlight : Cell
{
    private void OnEnable()
    {
        position = new Vector2Int(Mathf.FloorToInt(transform.localPosition.x), Mathf.FloorToInt(transform.localPosition.y));
        transform.localPosition = (Vector2)position;
    }

    private void Update()
    {
        float opacity = 0.8f;
        if (PhaseManager.Fighter == null)
            opacity = 0.65f;

        if (PhaseManager.HighlightIsFaded)
            opacity = 0.2f;

        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, opacity);
    }
}
