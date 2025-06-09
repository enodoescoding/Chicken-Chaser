using TMPro.EditorUtilities;
using UnityEngine;
using Utilities;

public abstract class Chicken : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float maxSpeed;
    [Header("Foot Management")]
    [SerializeField]
    protected float footRadius;
    [Header("Sockets")]
    [SerializeField]
    protected Transform head;
    [SerializeField]
    protected Transform foot;
    [SerializeField]
    protected float footDistance;
    protected Rigidbody physicsBody;
    protected Animator animatorController;
    protected bool isGrounded;
    protected float currentSpeed;
    protected float currentFallTime;
    protected Vector3 slopeNormal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        physicsBody = GetComponent<Rigidbody>();
        // get component in a children method as the chicken will be doing the animations, not the root object or parent object.
        animatorController = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleGroundState();
        HandleMovement();
        HandleAnims();
    }

    void HandleGroundState()
    {
        //check if the chicken is grounded, we are going spherecast downwards and detect if we've hit the floor 
        //StaticUtilities.GroundLayers helps the code know which layers to look at for floors, preventing players from registering grounded on illegal objects.
        bool isOnGround = Physics.SphereCast(foot.position, footRadius, Vector3.down, out RaycastHit slope, footDistance, StaticUtilities.GroundLayers);
        //if the onground state is different
        if (isOnGround != isGrounded)
        {
            //then we need to enter the state
            isGrounded = isOnGround;
            animatorController.SetBool(StaticUtilities.IsGroundedAnimID, isGrounded);
            
            //if we were falling
            if(currentFallTime >= 0)
            {
                HandleLanding();
                currentFallTime = 0;
            }
        }
        //if we are not grounded then update the air time
        if (!isGrounded)
        {
            currentFallTime += Time.deltaTime;
        }
        //otherwise, if we are grounded, keep track of the slope normal so that movement is smoother
        else
        {
            slopeNormal = slope.normal;
        }
    }

    protected virtual void HandleLanding()
    {
            
    }
    protected virtual void HandleAnims()
    {
        animatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, currentSpeed);
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public Vector3 GetLookDirection()
    {
        return head.forward;
    }

    //abstract functions, not defined here, will be defined in the child classes.

    protected abstract void HandleMovement();

    public abstract void OnFreedFromCage();

    public abstract void OnEscaped(Vector3 position);

    public abstract void OnCaptured();
}
