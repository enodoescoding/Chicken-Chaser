using AI;
using UnityEngine;
using Utilities;

public class CluckAbility : Ability
{
    [SerializeField]
    private ParticleSystem cluckParticles;
    [SerializeField]
    private AudioClip cluckSound;

    private const float audioVolume = 0.3f;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public override bool CanActivate()
    {
        return base.CanActivate() && owner.GetCurrentSpeed() < 1.0f;
    }

    protected override int GetAbilityBoolID()
    {
        return StaticUtilities.CluckAnimID;
    }

    protected override int GetAbilityTriggerID()
    {
        return StaticUtilities.JumpAnimID;
    }

    protected override void Activate()
    {
        cluckParticles.Play();
        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(cluckSound, SettingsManager.currentSettings.SoundVolume * audioVolume);
        AudioDetection.onSoundPlayed.Invoke(transform.position, 10f, 20f, ScriptableObjects.EAudioLayer.ChickenEmergency);
    }
}
