using System;
using System.Collections;
using System.Globalization;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class ScoreManager : MonoBehaviour
{
    [Header("HUDs")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Canvas endCanvas;
    
    //This should actually be it's own script.
    [Header("Score")] 
    [SerializeField] private AnimationCurve scoreCurve; // 0 is Quick, 1 is very long
    [SerializeField] private float expectedEndTime;
    [SerializeField] private int maximumTimePoints = 10000;
    [SerializeField] private int pointsPerSavedChicken = 5000;
    
    [Header("End Canvas")] 
    [SerializeField] private TextMeshProUGUI endStatus;
    [SerializeField] private TextMeshProUGUI numChickensSaved;
    [SerializeField] private TextMeshProUGUI timeSpent;
    [SerializeField] private TextMeshProUGUI finalScore;
    [SerializeField] private GameObject hopeIsNotLost;
    [SerializeField] private Button mainMenu;
    private float _cachedTime;
    private bool _canLoadMenu;
    private bool _cachedDidWin;
    
    private void WinGame(Vector3 _) => OnBeginEndGame(true);
    private void LoseGame(Vector3 _) => OnBeginEndGame(false);
    
    public static ScoreManager Instance { get; private set; }
    private void Awake()
    {
        print("Score Awake");
        if(Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
           
        hudCanvas.gameObject.SetActive(true);
        endCanvas.gameObject.SetActive(false);
        
        OnUIScaleChanged(SettingsManager.currentSettings.UIScale);
    }
    private void OnPlayerRescued()
    {
        _canLoadMenu = false;
        StopAllCoroutines();
        hudCanvas.gameObject.SetActive(true);
        endCanvas.gameObject.SetActive(false);
    }
    
    //These need to be enabled as the developer adds them...
    private void OnEnable()
    {
        PlayerChicken.onPlayerCaught += LoseGame;
        PlayerChicken.onPlayerRescued += OnPlayerRescued;
        PlayerChicken.onPlayerEscaped += WinGame;
        SettingsManager.SaveFile.onUIScaleChanged += OnUIScaleChanged;
        SettingsManager.SaveFile.onUIScaleChanged += OnUIScaleChanged;
    }

    private void OnDisable()
    {
        PlayerChicken.onPlayerCaught -= LoseGame;
        PlayerChicken.onPlayerRescued -= OnPlayerRescued;
        PlayerChicken.onPlayerEscaped -= WinGame;
        SettingsManager.SaveFile.onUIScaleChanged -= OnUIScaleChanged;
    }

    private void OnBeginEndGame(bool won)
    {
        _canLoadMenu = true;
        StartCoroutine(EndGameTimer());
        hudCanvas.gameObject.SetActive(false);
        endCanvas.gameObject.SetActive(true);

        endStatus.text = won ? "ESCAPED" : "CAUGHT";
        //This is currently not recieving updates when a regular chicken escapes...
        _cachedTime = GameManager.TimeInLevel;
        _cachedDidWin = won;
        UpdateScore();
        //We need a method to tell whether we won.
    }

    
    public void UpdateScore()
    {
        numChickensSaved.text = GameManager.NumChickensSaved + "/" + GameManager.NumChickens;
        TimeSpan s = TimeSpan.FromSeconds(_cachedTime);
        timeSpent.text = $"{s.Minutes}m {s.Seconds}s {s.Milliseconds}ms";
        finalScore.text = ((_cachedDidWin?1 - scoreCurve.Evaluate(_cachedTime / expectedEndTime):0) * maximumTimePoints + (pointsPerSavedChicken * GameManager.NumChickensSaved)).ToString(CultureInfo.InvariantCulture);

        //bool x = true;
        bool x = AIChicken.GetNumActiveChickens() == 0;
        
        //Determine which button to show
        hopeIsNotLost.SetActive(!x);
        mainMenu.gameObject.SetActive(x);
            
        mainMenu.Select();
    }
    
    public void LoadMainMenu()
    {
        //Only allow this to run once.
        if (!_canLoadMenu) return;
        GameManager.LoadMainMenu();
        _canLoadMenu = false;
    }
    
    //Simple timer
    private IEnumerator EndGameTimer()
    {
        yield return new WaitForSeconds(90); 
        LoadMainMenu();
    }
    //Let's just take responsibility for this...
    private void OnUIScaleChanged(float obj)
    {
        endCanvas.scaleFactor = obj;
        hudCanvas.scaleFactor = obj;
    }
}