using UnityEngine;
using Utilities;

public class PortalAbility :  Ability
{
    Portal portalA;
    Portal portalB;
    [SerializeField] private Portal portalPrefab;
    protected override void Activate()
    {
        Physics.Raycast(owner.transform.position, Vector3.down, out RaycastHit hit);
        Portal p = Instantiate(portalPrefab, hit.point -new Vector3(0, 0.5f, 0), Quaternion.LookRotation(hit.normal, Vector3.right));
        if (portalA)
        {
            Destroy(portalA.gameObject);
        }

        if (portalB)
        {
            portalA = portalB;
        }

        portalB = p;

        if (portalA)
        {
            portalA.Bind(portalB);
            portalB.Bind(portalA);
        }
    }

    public override bool CanActivate()
    {
        return base.CanActivate() && owner.GetIsGrounded();
    }
}
