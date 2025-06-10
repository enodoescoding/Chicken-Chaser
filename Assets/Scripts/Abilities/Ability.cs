using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField]
    protected AbilityStats stats;
    //we need to know to whom this ablitiy belongs to
    protected Chicken owner;
    protected Animator animatorController;
    // we need to track our state
    private bool isReady = true;
    private bool isBeingHeld;
    private float currentCooldownTime;

    //getter functions for access to the private variables
    public Sprite GetIcon()
    {
        return stats.Icon;
    }

    public float GetCooldownPercent()
    {
        return currentCooldownTime / stats.CoolDown;
    }

    private bool IsTriggerAnimation()
    {
        return GetAbilityTriggerID() != 0;
    }

    private bool IsBooleanAnimation()
    {
        return GetAbilityBoolID() != 0;
    }

    //use this function because we dont own these componets
    private void Start()
    {
        owner = GetComponentInParent<Chicken>();
        animatorController = GetComponentInChildren<Animator>();
    }

    //IEnumerator is a coroutine which is a function which can pause its execution and return control to unity but then continue where it left off on the following frame
    private IEnumerator BeginCooldown()
    {
        do
        {
            //wait until we can activate (this can be optimized by caching the WaitUntil() function)
            yield return new WaitUntil(CanActivate);
            //if we let go, then we should leave the loop
            if (!isBeingHeld)
            {
                yield break;
            }
            //activate and animate the ability
            Activate();
            if (IsTriggerAnimation())
            {
                animatorController.SetTrigger(GetAbilityTriggerID());
            }
            //refresh the cooldown
            currentCooldownTime = 0f;
            isReady = false;

            while (currentCooldownTime < stats.CoolDown)
            {
                currentCooldownTime += Time.deltaTime;
                //wait until the next frame
                yield return null;
            }
            //mark the ablitity as ready and set the current time to cooldown time until the cooldown is full
            currentCooldownTime = stats.CoolDown;
            isReady = true;
        }
        //this will loop until the coroutine is stopped
        while (isBeingHeld && stats.CanBeHeld);
        StopUsingAbility();
    }

    //accessibility functions
    public void StartUsingAbility()
    {
        isBeingHeld = true;
        if (isReady)
        {
            StartCoroutine(BeginCooldown());
        }
        if (IsBooleanAnimation())
        {
            animatorController.SetBool(GetAbilityBoolID(), true);
        }
    }

    public void StopUsingAbility()
    {
        isBeingHeld = false;
        if (IsBooleanAnimation())
        {
            animatorController.SetBool(GetAbilityBoolID(), false);
        }
    }

    //these functions will be overridden in the child classes
    public virtual bool CanActivate()
    {
        return isReady;
    }

    public virtual void ForceCancelAbility()
    {
        currentCooldownTime = stats.CoolDown;
        isReady = true;
        StopAllCoroutines();
        StopUsingAbility();
    }

    protected virtual int GetAbilityBoolID()
    {
        return 0;
    }

    protected virtual int GetAbilityTriggerID()
    {
        return 0;
    }

    protected abstract void Activate();
}
