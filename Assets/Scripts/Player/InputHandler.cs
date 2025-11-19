using System;
using ArrowPath.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputHandler : Singleton<InputHandler>
{
    private InputSystem_Actions inputActions;
    public Vector2 MoveInput  = Vector2.zero;

    public bool JumpInput;
    public float CoyoteTime = 0.2f;
    private float coyoteTimeCounter;

    public Vector2 AimInput  = Vector2.zero;
    public Vector2 AimStartPosition  = Vector2.zero;
    public bool StartAimingInput;
    public bool FireInput;
    public bool RicochetToggleInput;
    public bool[] ArrowSelectInput;
    public float ArrowScrollInput;

    public Action InteractInput;
    public bool CancelInput;
    protected override void Awake()
    {
        base.Awake();
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => MoveInput = Vector2.zero;
        
        inputActions.Player.Jump.performed += _ => JumpInput = true;
        inputActions.Player.Jump.canceled += _ => JumpInput = false;
        
        inputActions.Player.Aim.performed += ctx => AimInput = ctx.ReadValue<Vector2>();
        inputActions.Player.StartAiming.performed += _ =>
        {
            AimStartPosition = AimInput;
            StartAimingInput = true;
        };
        inputActions.Player.StartAiming.canceled += _ => StartAimingInput = false;
        inputActions.Player.Fire.canceled += _ => FireInput = true;
        inputActions.Player.Fire.performed += _ => FireInput = false;
        inputActions.Player.RicochetToggle.performed += _ => RicochetToggleInput = true;
        inputActions.Player.RicochetToggle.canceled += _ => RicochetToggleInput = false;
        
        ArrowSelectInput = new bool[5];
        inputActions.Player.ArrowSelect1.performed += _ => ArrowSelectInput[0] = true;
        inputActions.Player.ArrowSelect1.canceled += _ => ArrowSelectInput[0] = false;
        inputActions.Player.ArrowSelect2.performed += _ => ArrowSelectInput[1] = true;
        inputActions.Player.ArrowSelect2.canceled += _ => ArrowSelectInput[1] = false;
        inputActions.Player.ArrowSelect3.performed += _ => ArrowSelectInput[2] = true;
        inputActions.Player.ArrowSelect3.canceled += _ => ArrowSelectInput[2] = false;
        inputActions.Player.ArrowSelect4.performed += _ => ArrowSelectInput[3] = true;
        inputActions.Player.ArrowSelect4.canceled += _ => ArrowSelectInput[3] = false;
        inputActions.Player.ArrowSelect5.performed += _ => ArrowSelectInput[4] = true;
        inputActions.Player.ArrowSelect5.canceled += _ => ArrowSelectInput[4] = false;
        
        inputActions.Player.ArrowScrollWheel.performed += ctx => ArrowScrollInput = ctx.ReadValue<float>();
        inputActions.Player.ArrowScrollWheel.canceled += _ => ArrowScrollInput = 0f;

        inputActions.Player.Interact.performed += _ => InteractInput?.Invoke();
        inputActions.Player.Cancel.performed += _ => CancelInput = true;
        inputActions.Player.Cancel.canceled += _ => CancelInput = false;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();    
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
}