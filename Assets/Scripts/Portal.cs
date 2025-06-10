using System;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private Portal partner;
    private bool isEntrancePortal;
    
    
    public void Bind(Portal portal)
    {
        partner = portal;    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!partner || partner.isEntrancePortal)
        {
            return;
        }
        
        Rigidbody rb = other.attachedRigidbody;
        if (rb && rb.TryGetComponent(out IPortable target))
        {
            target.EnterPortal();
            isEntrancePortal = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!partner)
        {
            return;
        }

        Rigidbody rb = other.attachedRigidbody;
        
        
        if (rb && rb.TryGetComponent(out IPortable target))
        {
            if (isEntrancePortal)
            {
                Vector3 oldVelocity = rb.linearVelocity;
                oldVelocity.y = -oldVelocity.y;
                target.ApplyPortalForce(partner.transform.position, oldVelocity);
            }
            else
            {
                target.ExitPortal();
                RemovePortal();
                partner.RemovePortal();
            }
        }
    }

    private void RemovePortal()
    {
        Destroy(gameObject);
    }
}
