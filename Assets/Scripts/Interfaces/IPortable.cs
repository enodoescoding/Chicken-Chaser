using UnityEngine;
using Interfaces;

public interface IPortable
{
    public void ApplyPortalForce(Vector3 pos, Vector3 dir);
    
    public void EnterPortal();
    
    public void ExitPortal();
}
