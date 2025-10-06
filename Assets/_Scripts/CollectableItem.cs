using System;
using UnityEngine;

[Serializable]
public struct CollectableItemData
{
    public string Name;
    public ItemType Type;
    public Rarity Rarity;
    public int Price;
    public Sprite Icon;
    public string Description;
}


[RequireComponent(typeof(Outline))]
public class CollectableItem : MonoBehaviour
{
    [SerializeField] private CollectableItemData _data;
    [SerializeField] private InteractionPanel _interactionPanel;
    private Outline _outline;
    private bool _interactable;

    public CollectableItemData Data => _data;

    private void Awake()
    {
        _interactionPanel = GetComponentInChildren<InteractionPanel>();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
        _interactionPanel.gameObject.SetActive(false);
    }


    public void OnPlayerViewOn()
    {
        _interactable = true;
        SetOutline(_interactable);
        SetInteractionPanel(_interactable);
    }

    public void OnPlayerViewOff()
    {
        _interactable = false;
        SetOutline(_interactable);
        SetInteractionPanel(_interactable);
    }


    private void SetOutline(bool show)
    {
        _outline.enabled = show;
    }

    private void SetInteractionPanel(bool visible)
    {
        _interactionPanel.gameObject.SetActive(visible);
    }


    public void _Collect()
    {
        G.Inventory.Add(this);

    }

}
