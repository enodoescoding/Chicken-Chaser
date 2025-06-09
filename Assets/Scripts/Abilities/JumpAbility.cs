using UnityEngine;
using Utilities;

public class JumpAbility : Ability
{
    [SerializeField]
    private float jumpForce;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override int GetAbilityTriggerID()
    {
        return StaticUtilities.JumpAnimID;
    }

    public override bool CanActivate()
    {
        return base.CanActivate() && owner.GetIsGrounded();
    }

    protected override void Activate()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
