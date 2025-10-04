using UnityEngine;
using UnityEngine.InputSystem;

public class CustomPlayerInput : MonoBehaviour
{
    [SerializeField] private InputAction _interactionAction;
    [SerializeField] private InputAction _menuAction;
    [SerializeField] private InputAction _inventoryAction;
    [SerializeField] private InputAction _restartAction;


    public InputAction Interaction => _interactionAction;
    public InputAction Menu => _menuAction;
    public InputAction Inventory => _inventoryAction;
    public InputAction Restart => _restartAction;

    private void Awake()
    {
        if (G.Input == null)
        {
            G.Input = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
        CheckDefaultBindings();
    }

    void OnEnable()
    {
        if (_interactionAction != null) _interactionAction.Enable();
        if (_menuAction != null) _menuAction.Enable();
        if (_inventoryAction != null) _inventoryAction.Enable();
        if (_restartAction != null) _restartAction.Enable();
    }

    void OnDisable()
    {
        if (_interactionAction != null) _interactionAction.Disable();
        if (_menuAction != null) _menuAction.Disable();
        if (_inventoryAction != null) _inventoryAction.Disable();
        if (_restartAction != null) _restartAction.Disable();
    }

    private void CheckDefaultBindings()
    {
        if (_interactionAction == null || _interactionAction.bindings.Count == 0)
        {
            _interactionAction = new InputAction("Interaction", InputActionType.Button);
            _interactionAction.AddBinding("<Keyboard>/E");
        }
    }

    private void Update()
    {
        
    }
}