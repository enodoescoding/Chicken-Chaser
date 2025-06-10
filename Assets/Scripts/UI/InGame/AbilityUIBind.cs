using UI;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUIBind : MonoBehaviour
{
    
    [SerializeField] private CustomButton abilityButton;
    [SerializeField] private Image abilityFillBar;
    [SerializeField] private Image abilityIcon;
    
   // You may need to rename AbilityBase to AbstractAbility
    private Ability _targetAbility;
    
    //You may need to rename AbilityBase to AbstractAbility
    public void SetTargetAbility(Ability ability)
    {
      //  Bind the ability
        _targetAbility = ability;
        
        //Bind Image,
        abilityIcon.sprite = ability.GetIcon(); //You may need to rename Icon to GetIcon()
        //Bind OnClick and CUSTOM OnRelease
        abilityButton.onClick.RemoveAllListeners();
        abilityButton.onReleased.RemoveAllListeners();
        abilityButton.onClick.AddListener(() => _targetAbility.StartUsingAbility()); //You may need to rename StartAbility() to StartUsingAbility()
        abilityButton.onReleased.AddListener(() => _targetAbility.StopUsingAbility()); //You may need to rename StopAbility() to StopUsingAbility()
    }
    
    //Every single frame, we need to be updating and checking for ability status changes.
   // NOTE: This can be done better by batching abilities in a manager and updating everything simultaneously instead of individually.
    private void LateUpdate()
    {
        abilityFillBar.fillAmount = _targetAbility.GetCooldownPercent(); //You may need to rename GetReadyPercent() to GetCooldownPercent()
        abilityButton.interactable = _targetAbility.CanActivate();
    }
    
}
