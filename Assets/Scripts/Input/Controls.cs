using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controls : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    #region SetUp
    private static Controls instance;
    public static Controls Instance
    {
        get
        {
            if (instance)
                return instance;
            else
                return new GameObject("_Controls").AddComponent<Controls>();
        }
    }
    private InputSystem_Actions.PlayerActions m_Player;
    private InputSystem_Actions m_Actions;                  // Source code representation of asset.
    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        m_Actions = new InputSystem_Actions();              // create asset object.
        m_Player = m_Actions.Player;                      // extract action map object.
        m_Player.AddCallbacks(this);                      // register callback interface iplayeractions.
    }

    void OnDestroy()
    {
        m_Actions.Dispose();                              // destroy asset object.
    }

    void OnEnable()
    {
        m_Player.Enable();                                // enable all actions within map.
    }

    void OnDisable()
    {
        m_Player.Disable();                               // disable all actions within map.
    }
    #endregion

    public event Action<Vector2> OnPlayerMove;
    public Vector2 MousePosition;
    public event Action OnPlayerAttack;
    public event Action OnPlayerInteract;
    public event Action OnPlayerPause;

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started)
            return;

        Vector2 input = context.ReadValue<Vector2>();
        OnPlayerMove?.Invoke(input);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;

        MousePosition = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnPlayerAttack?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.performed)
            OnPlayerInteract?.Invoke();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnPlayerPause?.Invoke();
    }
}