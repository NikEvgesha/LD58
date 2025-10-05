using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Reward
{
    public CollectableItem item;
    public float weight;
}

public class Chest : DungeonTreasures
{
    [SerializeField] private List<Reward> _rewards;
    [SerializeField] private Transform _itemPoint;
    private InteractionPanel _interactionPanel;
    private Animator _animator;
    private BoxCollider _interactionTriggerCollider;

    private bool _opened;
    private float _totalWeight;

    private void Awake()
    {
        _interactionTriggerCollider = GetComponent<BoxCollider>();
        _interactionPanel = GetComponentInChildren<InteractionPanel>();
        _animator = GetComponent<Animator>();
        _interactionPanel.gameObject.SetActive(false);
        _totalWeight = _rewards.Sum(x => x.weight);
    }

    public void _OnRaycastHit()
    {
        if (_opened) return;
        ShowInteractionPanel(true);
    }

    public void _OnRaycastNoHit()
    {
        if (_opened) return;
        ShowInteractionPanel(false);
    }



    private void ShowInteractionPanel(bool show)
    {
        _interactionPanel.gameObject.SetActive(show);
    }


    public void Open()
    {
        _opened = true;
        _animator.SetTrigger("Open");
        Destroy(_interactionPanel.gameObject);
        _interactionTriggerCollider.enabled = false;
        SpawnReward();

    }

    private void SpawnReward()
    {
        float rand = UnityEngine.Random.Range(0, _totalWeight);
        Debug.Log("rand: " + rand);
        Debug.Log("total weight: " + _totalWeight);
        float current = 0;
        foreach (Reward reward in _rewards)
        {
            if (current + reward.weight > rand)
            {
                Debug.Log("item: " + reward.item.Data.Name);
                Instantiate(reward.item.gameObject, _itemPoint);
                break;
            }

            current += reward.weight;
        }
    }
}
