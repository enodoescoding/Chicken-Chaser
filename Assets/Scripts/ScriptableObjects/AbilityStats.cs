using UnityEngine;

[CreateAssetMenu(fileName = "AbilityStats", menuName = "ChickenChaser/AbilityStats")]
public class AbilityStats : ScriptableObject
{
    [SerializeField]
    private Sprite icon;
    [SerializeField]
    private float coolDown;
    [SerializeField]
    private bool canBeHeld;

    public Sprite Icon => icon;
    public float CoolDown => coolDown;
    public bool CanBeHeld => canBeHeld;
}
