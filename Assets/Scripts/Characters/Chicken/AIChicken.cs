using System;
using AI;
using Interfaces;
using ScriptableObjects;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using System.Collections;
using Managers;

public class AIChicken : Chicken, IDetector
{
    [SerializeField] private FaceTarget faceTarget;
    [SerializeField] private AudioDetection audioDetection;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private HearStats activeHearing;

    private static int numActiveChickens;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    protected override void Awake()
    {
        base.Awake();
        faceTarget = GetComponent<FaceTarget>();
        audioDetection = GetComponent<AudioDetection>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.speed = stats.MaxSpeed;
        navMeshAgent.acceleration = stats.Speed;

        HudManager.Instance.RegisterChicken(this);
        //used in the score
        GameManager.RegisterAIChicken();
    }

    private void OnEnable()
    {
        //used for caged chickens
        faceTarget.enabled = false;
        navMeshAgent.enabled = true;
        audioDetection.SetStats(activeHearing);
        bodyCollider.enabled = true;
        animatorController.SetBool(StaticUtilities.CluckAnimID, true);
        animatorController.enabled = true;

        //subscribe to the player events
        PlayerChicken.onPlayerCaught += MoveTo;
        PlayerChicken.onPlayerEscaped += MoveTo;

        numActiveChickens += 1;
        ScoreManager.Instance.UpdateScore();
    }

    private void OnDisable()
    {
        //unsubscribe to the player events (as we complete them)
        PlayerChicken.onPlayerCaught -= MoveTo;
        PlayerChicken.onPlayerEscaped -= MoveTo;
        //disable any active anims
        animatorController.SetBool(StaticUtilities.CluckAnimID, false);
        animatorController.enabled = false;
        navMeshAgent.ResetPath();
        navMeshAgent.enabled = false;
        bodyCollider.enabled = false;
        faceTarget.enabled = false;

        numActiveChickens -= 1;
        ScoreManager.Instance.UpdateScore();
    }

    private void OnDestroy()
    {
        HudManager.Instance.UnRegisterChicken(this);
    }

    private void MoveTo(Vector3 location)
    {
        navMeshAgent.SetDestination(location);
    }

    protected override void HandleMovement()
    {
        //move close to the target and decelerate when we are near them
        currentSpeed = Mathf.Max(0, navMeshAgent.remainingDistance - navMeshAgent.stoppingDistance) + 0.2f;
        //update the animator
        animatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, currentSpeed);
    }

    public override void OnFreedFromCage()
    {
        enabled = true;
        onFreed.Invoke();
    }

    public override void OnEscaped(Vector3 position)
    {
        //print who is trying to escape (, when doing comma gamebobject the debug will show the gameobject when pressed on in unity)
        Debug.Log("I am trying to escape", gameObject);
        //moveto the location to escape
        MoveTo(position);
        // we should not escape just yet because the ai needs time to actually get to the exit
        //let's start a coroutine and see if we escaped
        StartCoroutine(CheckForEscaped());
        //hide the AI so we don't have unfair captures
        visibility = 0;
        GameManager.PlayUISound(stats.EscapeSound);
    }

    public override void OnCaptured()
    {
        animatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, 0);
        onCaught.Invoke();
        GameManager.PlayUISound(stats.EscapeSound);
    }

    public void AddDetection(Vector3 location, float detection, EDetectionType type)
    {
        //If the AIChicken is not enabled or our detection is not enabled, range wasn't high enough, dont proceed.
        if (!enabled || detection < 1) return;
        print("I'm moving toward " + location);
        navMeshAgent.SetDestination(location);
        //stop doing cluck animation
        animatorController.SetBool(StaticUtilities.CluckAnimID, false);
    }

    private IEnumerator CheckForEscaped()
    {
        //cached move until the path is done generating and we reached the target
        WaitUntil target = new WaitUntil(() => navMeshAgent.hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance);
        yield return target;
        Debug.Log("I'm trying to escape");
        Destroy(gameObject);
    }

    public static int GetNumActiveChickens()
    {
        return numActiveChickens;
    }
}
