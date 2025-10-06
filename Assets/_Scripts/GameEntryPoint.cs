using UnityEngine;

public class GameEntryPoint : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private GameObject _gameUI;
    [SerializeField] private GameObject _lobbyUI;
    //[SerializeField] private PlayerManager _player;
    //[SerializeField] private UI _ui;
    [SerializeField] private ProceduralGeneration _proceduralGeneration;


    private void Start()
    {
        Instantiate(_inventory).Init();
        Instantiate(_proceduralGeneration).Init();

        Instantiate(_lobbyUI);
        Instantiate(_player);
        
        Instantiate(_gameUI);

        G.GameLoader.ShowLoadingImage(false);
        G.Game.OnGameStart();
    }
}