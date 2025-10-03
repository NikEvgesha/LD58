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
}
