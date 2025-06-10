using UnityEngine;
using Utilities;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class DashAbility : Ability
{
    [Header("Dash Settings")]
    [SerializeField]
    private float dashDistance;
    [SerializeField]
    private float dashDirection;
    [Header("Dash Effects")]
    [SerializeField]
    private ParticleSystem feathers;

    private Rigidbody rb;
    private bool canDash = true;
    private float radius;
    private const int NUM_SAMPLES = 10;
    private static readonly Vector3 maxVertical = new Vector3 (0, 1, 0 );

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SphereCollider collider = GetComponentInChildren<SphereCollider>();
        radius = collider.radius * collider.transform.lossyScale.x;
    }

    private IEnumerator ActivateAbility(Vector3 direction ) 
    {
        // do not dash upwards
        direction = new Vector3(direction.x, 0, direction.z) * dashDistance;
        canDash = false;
        // find the highest point on a slope
        Vector3 origin = transform.position;
        float yMax = 0;
        Vector3 start = origin + maxVertical;

        for (int i = 0; i < NUM_SAMPLES; i++)
        {
            float percent = i / (float)NUM_SAMPLES;

            if (Physics.Raycast(start + direction * percent, Vector3.down, out RaycastHit targetHit, dashDistance, StaticUtilities.VisibilityLayer))
            {
                Debug.DrawRay(start + direction * percent, Vector3.down * dashDistance, (targetHit.point.y > yMax) ? Color.green:Color.red, 3.0f);
                if (targetHit.point.y > yMax)
                {
                    yMax = targetHit.point.y;
                }
            }
            else
            {
                Debug.DrawRay(start + direction * percent, Vector3.down * dashDistance, Color.red, 3.0f);
            }
        }

        //make sure going upwards actually makes sense by subtracting the target y from the current y

        float directionY = yMax - origin.y + radius * 2;
        //if this direction was negiative or 0, then just go foward, or else, set the target y direction

        if (directionY > 0)
        {
            direction.y = directionY;
        }
        direction = direction.normalized;
        //face the opposite direction of ourselves
        feathers.transform.forward = -direction;
        feathers.Play();

        Vector3 endPoint = Physics.SphereCast(origin, radius, direction, out RaycastHit hit, dashDistance, StaticUtilities.VisibilityLayer)
            ? hit.point + hit.normal * (radius * 2) : direction * dashDistance + transform.position;

        Debug.DrawLine(origin, endPoint, Color.magenta, 5f);
        float currentTime = 0;
        //lock the rigidbody physics
        rb.isKinematic = true;
        while (currentTime < dashDirection)
        {
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, endPoint, currentTime / dashDirection);
            yield return null; 
        }
        transform.position = Vector3.Lerp(transform.position, endPoint, 1.0f);
        //unlock the rigidbody physics
        rb.isKinematic = false;
        feathers.Stop();
        canDash = true;
    }

    public override bool CanActivate()
    {
        return canDash && base.CanActivate();
    }

    protected override void Activate()
    {
        StartCoroutine(ActivateAbility(owner.GetLookDirection()));
    }

    private void OnDrawGizmosSelected()
    {
        SphereCollider c = GetComponentInChildren<SphereCollider>();
        radius = c.radius * c.transform.lossyScale.x;

        Gizmos.color = Color.yellow;
        GizmosExtras.DrawWireSphereCast(transform.position, transform.forward, dashDistance, radius);
    }

    public override void ForceCancelAbility()
    {
        base.ForceCancelAbility();
        feathers.Stop();
    }

    protected override int GetAbilityTriggerID()
    {
        return StaticUtilities.DashAnimID;
    }
}
