using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using UI; //needed for settings

[DefaultExecutionOrder(-1000)]
public class HudManager : MonoBehaviour
{
    [Header("UI Interactables")]
    [SerializeField] private AbilityUIBind abilityA;
    [SerializeField] private AbilityUIBind abilityB;
    [SerializeField] private AbilityUIBind abilityC;

    public static HudManager Instance { get; private set; }
    private PlayerChicken owner;

    [Header("Hud")]
    [SerializeField] private Transform trappedParent;
    [SerializeField] private Transform freedParent;
    [SerializeField] private Sprite caughtImg;
    [SerializeField] private Sprite freedImg;
    [SerializeField] private Image chickenImgPrefab;
    //we need a dictorynary so we can rembember which chicken is which
    private Dictionary<AIChicken, Image> hudChickens = new();

    #if(!UNITY_STANDALONE && !UNITY_WEBGL) || UNITY_EDITOR
    
    [SerializeField] private Joystick joyStick;
    [SerializeField] private Button settingsButton;
    [SerializeField] private float lookSpeed = 20f;
    [SerializeField, Range(0, 1)] private float stickDeadZoneY = 0.02f;
    [SerializeField] private bool isEditorPhoneMode = true;
    private float lookMultiplier;
    
    #endif
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //avoid multiple ibstances of the HudManager
        if (Instance != null & Instance != this)
        {
            Destroy(Instance);
            return;
        }
        Instance = this;
        #if !UNITY_STANDALONE && !UNITY_WEBGL
        joyStick.gameObject.SetActive(true);
        settingsButton.gameObject.SetActive(true);
        settingsButton.onClick.AddListener(EnterSettings);
        #endif
    }

    #region Registering Chickens
    public void BindPlayer(PlayerChicken player)
    {
        owner = player;

        //Bind the abilities to the UI
        abilityA.SetTargetAbility(player.GetCluckAbility());
        abilityB.SetTargetAbility(player.GetJumpAbility());
        abilityC.SetTargetAbility(player.GetDashAbility());
    }

    public void RegisterChicken(AIChicken chicken)
    {
        //create a prefab and edit to the dictionary
        Image clone = Instantiate(chickenImgPrefab);
        hudChickens.Add(chicken, clone);
        //bind our events to automate chicken captures and releases
        chicken.onCaught += () => CatchChicken(clone);
        chicken.onFreed += () => FreeChicken(clone);
        //assume the chicken is caught
        CatchChicken(clone);
    }

    public void UnRegisterChicken(AIChicken chicken)
    {
        Destroy(hudChickens[chicken]);
        hudChickens.Remove(chicken);
    }

    private void CatchChicken(Image target)
    {
        target.transform.SetParent(trappedParent, false);
        target.sprite = caughtImg;
    }

    private void FreeChicken(Image target)
    {
        target.transform.SetParent(freedParent, false);
        target.sprite = freedImg;
    }
    #endregion
    
    #region Mobile
    
        #if !UNITY_STANDALONE && !UNITY_WEBGL
            private static Vector2 displayScale;

            private void OnEnable()
            {
                //bind the sens settings
                SettingsManager.SaveFile().onLookSensChanged += OnLookSensChanged;
                //set the current state
                OnLookSensChanged(SettingsManager.currentSettings).lookSpeed;
            }

            private void OnDisable()
            {
                //unbind the settings
                SettingsManager.SaveFile().onLookSensChanged -= OnLookSensChanged;
            }
            #if UNITY_EDITOR
            
            #endif

            private void Update()
            {
                #if UNITY_EDITOR
                    if(!EditorPhoneMode)
                    {
                        return;
                    }
                #endif
                owner.SetMoveDirection(joyStick.Direction != Vector2.zero ? Vector2.up : Vector2.zero);
                //for looking, we should check to see if the magnitude is some really small number.
                //if it is, we should actually just ignore it
                Vector2 value = new Vector2(joyStick.Direction.x Mathf.Abs(joyStick.Direction.y) > joyStick.deadZone ? joyStick.Direct.y / ((float) Screen.currentResolution.width / Screen.currentResolution.height) : 0);
                //force the owner to look as if it's receiving mouse input
                owner.SetLookDirection(Vector2.Scale(value, displayScale));
                
            }

            private void OnLookSensChanged(float obj)
            {
                //when the sens is changed, double check the screen scale and recalucate values
                //incase the screen was rotated, there are other ways to check this through
                lookMultiplier = obj;
                displayScale = new Vector2(1, (float)( Screen.width / Screen.height * lookMultiplier) lookSpeed * lookMultiplier);
            }
    
            private void EnterSettings()
            {
                //enter the settings menu
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(ExitSettings);
                Settings.OpenSettings(false);
            }
            
            private void ExitSettings()
            {
                //close the settings menu
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(EnterSettings);
                settings.CloseSettings();
            }
    
        #endif
    
    #endregion
}
