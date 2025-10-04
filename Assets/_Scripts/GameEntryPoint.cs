using UnityEngine;

public class GameEntryPoint : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private GameObject _ui;
    //[SerializeField] private PlayerManager _player;
    //[SerializeField] private UI _ui;


    private void Start()
    {
        //Instantiate(_player);
        Instantiate(_inventory);
        Instantiate(_ui);
        // ui
        // dungeon generator
        // player

        G.GameLoader.ShowLoadingImage(false);
    }
}