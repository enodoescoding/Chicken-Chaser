using AI;
using Interfaces;
using ScriptableObjects;
using System;
using Characters;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Utilities;
using Random = UnityEngine.Random;

public abstract class Chicken : MonoBehaviour, IVisualDetectable, ITrappable
{
    [SerializeField] protected ChickenStats stats;
    [Header("Sockets")]
    [SerializeField] protected Transform head;
    [SerializeField] protected Transform foot;
    [SerializeField] private ParticleSystem landEffect;
    protected float visibility = 1;
    protected Rigidbody physicsBody;
    protected Animator animatorController;
    protected bool isGrounded;
    protected float currentSpeed;
    protected float currentFallTime;
    protected Vector3 slopeNormal;
    protected Collider bodyCollider;

    [SerializeField] private FaceTarget faceTarget;
    [SerializeField] private AudioDetection audioDetection;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private HearStats activeHearing;

    public Action onCaught;
    public Action onFreed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        physicsBody = GetComponent<Rigidbody>();
        // get component in a children method as the chicken will be doing the animations, not the root object or parent object.
        animatorController = GetComponentInChildren<Animator>();
        bodyCollider = GetComponentInChildren<Collider>();
        ChickenAnimatorReceiver car = transform.GetChild(0).GetComponent<ChickenAnimatorReceiver>();
        car.OnLandEffect += HandleLanding;
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
        bool isOnGround = Physics.SphereCast(foot.position, stats.FootRadius, Vector3.down, out RaycastHit slope, stats.FootDistance, StaticUtilities.GroundLayers);
        //if the onground state is different
        if (isOnGround != isGrounded)
        {
            //then we need to enter the state
            isGrounded = isOnGround;
            animatorController.SetBool(StaticUtilities.IsGroundedAnimID, isGrounded);
            
            //if we were falling
            if(currentFallTime >= 0)
            {
                HandleLanding(Mathf.Max(currentFallTime / 2, 3));
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

    protected virtual void HandleLanding(float force)
    {
        landEffect.emission.SetBurst(0, new ParticleSystem.Burst(0, Random.Range(10,20) * force));
        landEffect.Play();
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

    public void AddVisibility(float visibility)
    {
        this.visibility += visibility;
    }

    public void RemoveVisibility(float visibility)
    {
        this.visibility -= Mathf.Max(0, visibility);
    }

    public float GetVisibility()
    {
        return visibility;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool CanBeTrapped()
    {
        return isActiveAndEnabled;
    }

    public void OnPreCapture()
    {
        enabled = false;
    }
}
