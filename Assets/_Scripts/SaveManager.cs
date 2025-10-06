using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private void Awake()
    {
        if (G.SaveManager == null)
        {
            G.SaveManager = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

    }


    public void SaveCoins(int amount)
    {
        PlayerPrefs.SetInt("Coins", amount);
    }

    public int LoadCoins() 
    {
        return PlayerPrefs.GetInt("Coins", 0);
    }

    public void SaveCollection(HashSet<CollectableItemData> items)
    {
        string itemNames = "";
        foreach (CollectableItemData item in items) 
        {
            itemNames += (item.Name + "\n");
        }
        PlayerPrefs.SetString("Collection", itemNames);
    }

    public List<string> LoadCollection()
    {
        return PlayerPrefs.GetString("Collection", "").Split('\n').ToList();
    }

    public void SaveItem(MarketItemData item, int amount)
    {
        PlayerPrefs.SetInt(item.Name, amount);
    }

    public int LoadItem(MarketItemData item)
    {
        return PlayerPrefs.GetInt(item.Name, 0);
    }

}
