using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Selectable))]
public class UIHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public Action OnSelectAction, OnDeselectAction;
    public Action OnMadeStaticAction, OnMadeUnstaticAction;

    public GameObject highlightPrefab;
    private GameObject highlight;
    private RectTransform highlightImage;

    public bool IsStatic { get; private set; }
    private GameObject staticHighlight;

    public Vector2 sizeDelta = new Vector2(130, 50);
    private const float delta = 10;
    
    public Selectable Selectable { get; private set; }

    void Awake()
    {
        Selectable = GetComponent<Selectable>();
    }

    void Update()
    {
        if (highlight == null)
            return;

        float x = 0, y = 0;

        x = sizeDelta.x + Mathf.Abs(delta * Mathf.Sin(Mathf.PI * Time.time));
        y = sizeDelta.y + Mathf.Abs(delta * Mathf.Sin(Mathf.PI * Time.time));
        y = Mathf.Clamp(y, 60f, float.MaxValue);

        highlightImage.sizeDelta = new Vector2(x, y);
    }

    public void MakeStatic()
    {
        Transform p = transform.parent;
        foreach (UIHighlight u in p.GetComponentsInChildren<UIHighlight>())
            u.ClearStatic();

        IsStatic = true;
        Deselect();
        Selectable.interactable = false;

        staticHighlight = Instantiate(highlightPrefab, transform);
        staticHighlight.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        Image i = staticHighlight.GetComponentInChildren<Image>();
        highlightImage = i.GetComponent<RectTransform>();

        float x = 0, y = 0;

        x = sizeDelta.x;
        y = sizeDelta.y;
        y = Mathf.Clamp(y, 60f, float.MaxValue);

        highlightImage.sizeDelta = new Vector2(x, y);

        i.color = new Color(0.75f, 0.75f, 0.75f, 1f);

        OnMadeStaticAction?.Invoke();
    }
    public void ClearStatic()
    {
        if (staticHighlight == null)
            return;

        Image i = staticHighlight.GetComponentInChildren<Image>();
        Destroy(staticHighlight.gameObject);

        Selectable.interactable = true;
        IsStatic = false;

        OnMadeUnstaticAction?.Invoke();
    }

    public void Deselect()
    {
        if (highlight != null)
            Destroy(highlight.gameObject);

        highlight = null;
        highlightImage = null;
        
        OnDeselectAction?.Invoke();
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (!Selectable.interactable)
            return;

        highlight = Instantiate(highlightPrefab, transform);
        highlight.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        highlightImage = highlight.GetComponentInChildren<Image>().GetComponent<RectTransform>();
        
        OnSelectAction?.Invoke();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (!Selectable.interactable)
            return;

        if (highlight != null)
            Destroy(highlight.gameObject);

        highlight = null;
        highlightImage = null;
        
        OnDeselectAction?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Selectable.interactable)
            return;

        Selectable.Select();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //OnDeselect(eventData);
    }
}