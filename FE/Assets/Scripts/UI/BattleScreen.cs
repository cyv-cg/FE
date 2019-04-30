using UnityEngine;
using System.Collections;

public class BattleScreen : MonoBehaviour
{
    public static BattleScreen instance;

    public static BattleAnimController Attacker { get; private set; }
    public static BattleAnimController Defender { get; private set; }

    public Transform background;
    public Transform left, right;

    public Transform mask;

    private Camera cam;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        cam = Camera.main;
    }

    public static IEnumerator Open(Fighter attacker, Fighter defender)
    {
        instance.SetBackground();
        yield return instance.StartCoroutine(instance.ZoomIn(Map.UnitTile(defender)));
        GameObject[] fighters = instance.SetUnits(attacker, defender);

        GameObject left = fighters[0].GetComponentInChildren<BattleAnimController>().linked == attacker ? attacker.gameObject : defender.gameObject;
        GameObject right = left == attacker.gameObject ? defender.gameObject : attacker.gameObject;

        instance.StartCoroutine(instance.ZoomUnits(
            new GameObject[4] { fighters[0], fighters[1], left, right } 
        ));

        yield return new WaitForSeconds(1);
    }
    public static IEnumerator Close()
    {
        if (Attacker != null)
            instance.StartCoroutine(instance.FadeOut(Attacker.transform.parent.gameObject));
        if (Defender != null)
            instance.StartCoroutine(instance.FadeOut(Defender.transform.parent.gameObject));

        yield return instance.StartCoroutine(instance.ZoomOut());
    }

    private void SetBackground()
    {
        Vector2 pos = cam.ScreenToWorldPoint(Vector2.zero);
        Vector2 scale = new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize) * 2;

        background.position = pos;
        background.localScale = scale;
    }

    IEnumerator ZoomIn(Cell tile)
    {
        float t = 1;

        while (t > 0)
        {
            Vector2 scale = new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize) * 2 * t;
            Vector2 pos = tile.position + Vector2.one / 2f - scale / 2f;

            mask.position = pos;
            mask.localScale = scale;

            t -= Time.deltaTime * 2f;
            yield return null;
        }

        mask.transform.localScale = Vector2.zero;
    }
    IEnumerator ZoomOut()
    {
        float s = 0;
        while (s < 1.2)
        {
            Vector3 scale = new Vector3(cam.aspect * cam.orthographicSize, cam.orthographicSize) * 2f * s;
            Vector3 pos = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height) / 2f + Vector3.forward * 10) - scale / 2f;

            mask.position = pos;
            mask.localScale = scale;
            s += Time.deltaTime * 2f;
            yield return null;
        }
    }

    GameObject[] SetUnits(Fighter attacker, Fighter defender)
    {
        Vector2Int atkPos = attacker.Position(), defPos = defender.Position();

        Unit _1 = atkPos.x < defPos.x ? attacker.Unit : defender.Unit;
        string path1 = "Assets/Data/Units/BattlePrefabs/" + _1.unitClass + (_1.unitSex != Unit.Sex.NA ? "_" + _1.unitSex : "") + ".prefab";

        Unit _2 = _1 == attacker.Unit ? defender.Unit : attacker.Unit;
        string path2 = "Assets/Data/Units/BattlePrefabs/" + _2.unitClass + (_2.unitSex != Unit.Sex.NA ? "_" + _2.unitSex : "") + ".prefab";
        
        GameObject one = Instantiate((GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(path1, typeof(GameObject)), left);
        GameObject two = Instantiate((GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(path2, typeof(GameObject)), right);

        one.SetActive(false);
        two.SetActive(false);

        one.transform.localScale = new Vector3(1, 1, 1);
        two.transform.localScale = new Vector3(-1, 1, 1);

        if (one != null)
        {
            BattleAnimController b = one.GetComponentInChildren<BattleAnimController>();
            b.alignment = _1.alignment;
            if (_1 == attacker.Unit)
            {
                b.linked = attacker;
                Attacker = b;
            }
            else
            {
                b.linked = defender;
                Defender = b;
            }
        }
        else
            Debug.LogError(path1 + " not found");
        if (two != null)
        {
            BattleAnimController b = two.GetComponentInChildren<BattleAnimController>();
            b.alignment = _2.alignment;
            if (_2 == attacker.Unit)
            {
                b.linked = attacker;
                Attacker = b;
            }
            else
            {
                b.linked = defender;
                Defender = b;
            }
        }
        else
            Debug.LogError(path2 + " not found");

        return new GameObject[2] { one, two };
    }
    IEnumerator ZoomUnits(GameObject[] prefabs)
    {
        GameObject[] gos = new GameObject[2];
        
        for (int i = 0; i < 2; i++)
        {
            int i2 = i + 2;

            GameObject root = new GameObject("root"),
                    battlegfx = new GameObject("battlegfx"),
                    mapgfx = new GameObject("mapgfx");
            battlegfx.transform.parent = root.transform;
            mapgfx.transform.parent = root.transform;

            SpriteRenderer[] battle = prefabs[i].GetComponentsInChildren<SpriteRenderer>(),
                map = prefabs[i2].GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sr in battle)
            {
                GameObject g = new GameObject("gfx");
                g.transform.parent = battlegfx.transform;
                SpriteRenderer s = g.AddComponent<SpriteRenderer>();
                s.sprite = sr.sprite;
                s.sortingLayerName = sr.sortingLayerName;
                s.sortingOrder = 9999;
                s.material.shader = Shader.Find("GUI/Text Shader");
            }
            foreach (SpriteRenderer sr in map)
            {
                GameObject g = new GameObject("gfx");
                g.transform.parent = mapgfx.transform;
                SpriteRenderer s = g.AddComponent<SpriteRenderer>();
                s.sprite = sr.sprite;
                s.sortingLayerName = sr.sortingLayerName;
                s.sortingOrder = 9999;
                s.material.shader = Shader.Find("GUI/Text Shader");
            }

            if (i == 1)
                root.transform.localScale = new Vector3(-1, 1, 1);

            gos[i] = root;
        }

        float t = 0;
        while (t < 1)
        {
            for (int i = 0; i < gos.Length; i++)
            {
                Transform b = gos[i].transform.GetChild(0), m = gos[i].transform.GetChild(1);
                b.transform.localScale = Vector3.one * t;
                m.transform.localScale = Vector3.one * (1 - t);

                gos[i].transform.position = Vector2.Lerp(gos[i].transform.position, prefabs[i].transform.position, t);
            }

            t += Time.deltaTime * 2;
            yield return null;
        }

        Enable(new GameObject[2] { prefabs[0], prefabs[1] } );

        float a = 1;
        while (a > 0)
        {
            for (int i = 0; i < gos.Length; i++)
            {
                Transform b = gos[i].transform.GetChild(0), m = gos[i].transform.GetChild(1);
                SpriteRenderer[] _b = b.GetComponentsInChildren<SpriteRenderer>(),
                    _m = m.GetComponentsInChildren<SpriteRenderer>();

                foreach (SpriteRenderer sr in _b)
                    sr.color = new Color(1, 1, 1, a);
                foreach (SpriteRenderer sr in _m)
                    sr.color = new Color(1, 1, 1, a);
            }

            a -= Time.deltaTime;
            yield return null;
        }

        Destroy(gos[1]);
        Destroy(gos[0]);
    }

    void Enable(GameObject[] gos)
    {
        foreach (GameObject go in gos)
            go.SetActive(true);
    }

    IEnumerator FadeOut(GameObject go)
    {
        SpriteRenderer[] srs = go.GetComponentsInChildren<SpriteRenderer>();
        Color[] colors = new Color[srs.Length];

        for (int c = 0; c < colors.Length; c++)
            colors[c] = srs[c].color;

        float a = 1;
        while (a > 0)
        {
            for (int c = 0; c < srs.Length; c++)
            {
                srs[c].color = new Color(colors[c].r, colors[c].g, colors[c].b, a);
                a -= Time.deltaTime * 2;
            }

            yield return null;
        }

        Destroy(go);
    }
}