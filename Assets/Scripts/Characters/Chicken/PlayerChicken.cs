using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerChicken : Chicken
{
    protected Rigidbody rb;
    private Vector3 moveDirection;
    private Vector2 lookDirection;
    [Header("Looking")]
    [SerializeField, Range(0, 90)]
    private float pitchLimit = 30f;
    [SerializeField, Range(0, 180)]
    private float yawLimit = 180f;
    [SerializeField]
    private float lookSpeed = 5.0f;
    [Header("Ablities")]
    [SerializeField]
    private Ability jumpAbility;
    [SerializeField]
    private Ability cluckAbility;
    [SerializeField]
    private Ability dashAbility;
    // Awake is called before start and should be used to define things
    protected override void Awake()
    {
        base.Awake();
   //   rb = GetComponent<Rigidbody>();
        PlayerControls.Initialize(this);
        PlayerControls.UseGameControls();
    }

    #region States
    private void OnDisable()
    {
        PlayerControls.DisablePlayer();
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
        physicsBody.AddForce(transform.rotation * moveDirection * speed, ForceMode.Acceleration);
        // were only dealing with the x and z axes, so we dont want to affect the falling y axes
        Vector2 groundVelocity = new Vector2(physicsBody.linearVelocity.x, physicsBody.linearVelocity.z);
        // if we are moving too fast then we need to clamp our speed
        currentSpeed = groundVelocity.magnitude;
        if (currentSpeed > maxSpeed)
        {
            groundVelocity = groundVelocity.normalized * maxSpeed;
            //limit the speed, but be sure to keep the gravity y speed
            physicsBody.linearVelocity = new Vector3(groundVelocity.x, physicsBody.linearVelocity.y, groundVelocity.y);
            // lock the speed to prevent weird bugs
            currentSpeed = maxSpeed;
        }
        HandleLooking();
    }

    public override void OnFreedFromCage()
    {

    }

    public override void OnEscaped(Vector3 position)
    {

    }

    public override void OnCaptured()
    {

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
}
