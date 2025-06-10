using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Utilities;

public class PlayerChicken : Chicken, IPortable
{
    // observed player events
    public static Action<Vector3> onPlayerCaught;
    public static Action<Vector3> onPlayerEscaped;
    public static Action onPlayerRescued;

    private Vector3 moveDirection;
    private Vector2 lookDirection;
    [Header("Looking")]
    [SerializeField, Range(0, 90)]
    private float pitchLimit = 30f;
    [SerializeField, Range(0, 180)] private float yawLimit = 180f;
    [SerializeField] private float lookSpeed = 5.0f;
    [Header("Ablities")]
    [SerializeField] Ability jumpAbility;
    [SerializeField] private Ability cluckAbility;
    [SerializeField] private Ability dashAbility;
    [Header("Effects")]
    [SerializeField] private GameObject lossCam;
    
    // Awake is called before start and should be used to define things
    protected override void Awake()
    {
        base.Awake();
        HudManager.Instance.BindPlayer(this);
        //   rb = GetComponent<Rigidbody>();
        PlayerControls.Initialize(this);
        PlayerControls.UseGameControls();
    }

    private void OnEnable()
    {
        physicsBody.isKinematic = false;
        bodyCollider.enabled = true;
        SettingsManager.SaveFile.onLookSenseChanged += onLookSenseChanged;
        lookSpeed = SettingsManager.currentSettings.LookSensitivity;
    }

    private void onLookSenseChanged(float val)
    {
        lookSpeed = val;
    }

    #region States
    private void OnDisable()
    {
        physicsBody.isKinematic = true;
        bodyCollider.enabled = false;
        PlayerControls.DisablePlayer();
        jumpAbility.ForceCancelAbility();
        cluckAbility.ForceCancelAbility();
        dashAbility.ForceCancelAbility();
        SettingsManager.SaveFile.onLookSenseChanged -= onLookSenseChanged;
    }

    public void SetDashState(bool state)
    {
        if (state == true)
        {
            dashAbility.StartUsingAbility();
        }
        else
        {
            dashAbility.StopUsingAbility();
        }
    }

    public void SetCluckState(bool state)
    {
        if (state == true)
        {
            cluckAbility.StartUsingAbility();
        }
        else
        {
            cluckAbility.StopUsingAbility();
        }
    }

    public void SetJumpState(bool state)
    {
        if (state == true)
        {
            jumpAbility.StartUsingAbility();
        }
        else
        {
            jumpAbility.StopUsingAbility();
        }
    }

    #endregion
    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = new Vector3(direction.x , 0, direction.y);
    }

    public void SetLookDirection(Vector2 direction)
    {
        lookDirection = direction;
    }

    protected override void HandleMovement()
    {
        Vector3 direction = moveDirection;
        if (isGrounded)
        {
            // if we are grounded then the direction we want to move should be projected on to the plane / ground. Doing this will help us move up steep slopes easier
            direction = Vector3.ProjectOnPlane(direction, slopeNormal).normalized;
        }
        physicsBody.AddForce(transform.rotation * moveDirection * stats.Speed, ForceMode.Acceleration);
        // were only dealing with the x and z axes, so we dont want to affect the falling y axes
        Vector2 groundVelocity = new Vector2(physicsBody.linearVelocity.x, physicsBody.linearVelocity.z);
        // if we are moving too fast then we need to clamp our speed
        currentSpeed = groundVelocity.magnitude;
        if (currentSpeed > stats.MaxSpeed)
        {
            groundVelocity = groundVelocity.normalized * stats.MaxSpeed;
            //limit the speed, but be sure to keep the gravity y speed
            physicsBody.linearVelocity = new Vector3(groundVelocity.x, physicsBody.linearVelocity.y, groundVelocity.y);
            // lock the speed to prevent weird bugs
            currentSpeed = stats.MaxSpeed;
        }
        HandleLooking();
    }

    public override void OnFreedFromCage()
    {
        OnEnable();
        PlayerControls.UseGameControls();
        onPlayerRescued?.Invoke();
        cluckAbility.StopUsingAbility();
        lossCam.SetActive(false);
    }

    public override void OnEscaped(Vector3 position)
    {
        onPlayerEscaped?.Invoke(transform.position);
        Debug.Log("Player escaped");

        //create an ai to take over for us
        NavMeshAgent agent = gameObject.AddComponent<NavMeshAgent>();
        agent.enabled = true;
        agent.baseOffset = 0.16f;
        agent.height = 0.32f;
        agent.radius = 0.2f;
        agent.agentTypeID = 0;
        agent.SetDestination(position);
        animatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, stats.MaxSpeed);
        enabled = false;
        GameManager.PlayUISound(stats.EscapeSound);
    }

    public override void OnCaptured()
    {
        Debug.Log("Player got captured");
        animatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, 0);
        cluckAbility.StartUsingAbility();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        //make humans ignore us
        visibility = 0;
        onPlayerCaught?.Invoke(transform.position);
        lossCam.SetActive(true);
        GameManager.PlayUISound(stats.CaughtSound);
    }
    
    private void HandleLooking()
    {
        // caching the time (time.delta time) value is important if your using more then once. It says Ram
        float timeShift = Time.deltaTime;
        float pitchChange = head.localEulerAngles.x - lookSpeed * lookDirection.y * timeShift;
        float yawChange = transform.localEulerAngles.y + lookSpeed * lookDirection.x * timeShift;
        // apply limits so we dont gimbal lock ourselves.
        if (pitchChange > pitchLimit && pitchChange < 180)
        {
            pitchChange = pitchLimit;
        }
        else if (pitchChange < 360 - pitchLimit && pitchChange > 180)
        {
            pitchChange = -pitchLimit;
        }

        if (yawChange > yawLimit && yawChange < 180)
        {
            yawChange = yawLimit;
        }
        else if (yawChange < 360 - yawLimit && yawChange > 180)
        {
            yawChange = -yawLimit;
        }

        // apply the modifications to each part (be sure to use localeurlerangles() so that other systems work correctly.
        transform.localEulerAngles = new Vector3(0, yawChange, 0);
        head.localEulerAngles = new Vector3(pitchChange, 0, 0);
    }

    public Ability GetCluckAbility()
    {
        return cluckAbility;
    }

    public Ability GetJumpAbility()
    {
        return jumpAbility;
    }

    public Ability GetDashAbility()
    {
        return dashAbility;
    }

    public void ApplyPortalForce(Vector3 pos, Vector3 dir)
    {
        transform.position = pos;
        physicsBody.AddForce(dir, ForceMode.VelocityChange); 
    }

    public void EnterPortal()
    {
        bodyCollider.enabled = false;
    }

    public void ExitPortal()
    {
        bodyCollider.enabled = true;
    }
}
