using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class ArrowDisplay : Cell
{
    public Sprite headUp, headDown, headLeft, headRight;
    public Sprite horizontal, vertical;
    public Sprite tailUp, tailDown, tailLeft, tailRight;
    public Sprite cornerNE, cornerSE, cornerNW, cornerSW;

    private Cell[] neighbors;
    private List<Cell> path = new List<Cell>();

    private SpriteRenderer sr;

    private void OnEnable()
    {
        position = new Vector2Int(Mathf.FloorToInt(transform.localPosition.x), Mathf.FloorToInt(transform.localPosition.y));
        transform.localPosition = (Vector2)position;
        //Map.Add(this, layer, isIcon);

        sr = GetComponent<SpriteRenderer>();

        //neighbors = Map.GetNeighbors(this);
        //foreach (Cell c in UnitController.CurrentPath)
        //    path.Add(c);

        //AssignSprite();
    }

    public void AssignSprite()
    {
        path.Clear();
        neighbors = Map.GetNeighbors(this);
        foreach (Cell c in PhaseManager.CurrentPath)
            path.Add(c);

        bool up, right, down, left;

        up = path.Contains(neighbors[0]);
        right = path.Contains(neighbors[1]);
        down = path.Contains(neighbors[2]);
        left = path.Contains(neighbors[3]);

        if (Map.GetCellData(position) == path[0])
        {
            if (up)
                sr.sprite = tailUp;
            else if (right)
                sr.sprite = tailRight;
            else if (down)
                sr.sprite = tailDown;
            else if (left)
                sr.sprite = tailLeft;
        }
        else if (Map.GetCellData(position) == path[path.Count - 1])
        {
            if (up)
                sr.sprite = headDown;
            else if (right)
                sr.sprite = headLeft;
            else if (down)
                sr.sprite = headUp;
            else if (left)
                sr.sprite = headRight;
        }
        else
        {
            if (left && right && !up && !down)
                sr.sprite = horizontal;
            else if (up && down && !left && !right)
                sr.sprite = vertical;

            else if (down && right && !up && !left)
                sr.sprite = cornerSE;
            else if (down && left && !up && !right)
                sr.sprite = cornerSW;

            else if (up && right && !down && !left)
                sr.sprite = cornerNE;
            else if (up && left && !down && !right)
                sr.sprite = cornerNW;
        }
    }
}