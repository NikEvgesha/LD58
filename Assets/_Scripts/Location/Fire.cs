using UnityEngine;


public class Fire : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        if (other.tag != "Player") return;                                      
        
        G.PlayerStatManager.Fear = 0;
    }
}
