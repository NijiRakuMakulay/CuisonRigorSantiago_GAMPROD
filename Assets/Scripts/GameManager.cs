using GameAnalyticsSDK;
using GameAnalyticsSDK.Setup;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    AudioManager sound;
    int balance;
    float NormalizedInstability;
    public int MaxInstability = 10; //game ends if instability hits thresholds of -10 or 10
    bool TooManyMissed = false;
    bool SeriousInstability = false;
    bool GoalAchieved = false;
    public float towerCenterX = 0.5f; //ship's center based on x position in scene
    public TextMeshProUGUI ResultText;
    public CanvasGroup EndPanel;
    public CanvasGroup MainUI;
    public Slider Progressbar;
    public TextMeshProUGUI ProgressText;
    public TextMeshProUGUI MissText;
    public TextMeshProUGUI PlaceText;
    public Scrollbar InstabilityMeter;
    public TextMeshProUGUI InstabilityText;
    int BlockCount;
    int MissCount;
    int BlockHeight;
    public int GameOverThreshold = 3;
    public int GameClearThreshold = 10;
    string message;
    CraneController Crane;
    int score;
    bool GameOver;

    void Awake()
    {
        sound = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        if (sound == null) { Debug.LogError("No audio manager!"); }
        Crane = FindFirstObjectByType<CraneController>();
        if (Crane == null)
        {
            Debug.LogError("No CraneController found in the scene!");
        }
        GameAnalytics.Initialize();
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "StackHigh");
    }

    void LoseBGM()
    {
        sound.PlayAudio(0);
        sound.SetLoop(false);
    }

    void LevelBGM()
    {
        sound.PlayAudio(1);
        sound.SetLoop(true);
    }

    void WinBGM()
    {
        sound.PlayAudio(2);
        sound.SetLoop(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        BlockCount = 0;
        InstabilityMeter.value = 0.5f;
        GameOver = false;
        TooManyMissed = false;
        SeriousInstability = false;
        GoalAchieved = false;
        LevelBGM();
    }

    // Update is called once per frame
    void Update()
    {
        float heightchk = Crane.getHeight();
        BlockHeight = (int)heightchk;
        NormalizedInstability = Mathf.InverseLerp(-10.0f, 10.0f, balance);
        ResultText.text = message;
        //Result Screen Handler
        score = (int)BlockHeight;
        if (!GameOver)
        {
            message = " ";
            EndPanel.alpha = 0.0f;
            EndPanel.interactable = false;
            EndPanel.blocksRaycasts = false;
            MainUI.alpha = 1.0f;
            MainUI.interactable = false;
            MainUI.blocksRaycasts = true;
            Progressbar.minValue = 0;
            Progressbar.value = BlockHeight;
            Progressbar.maxValue = GameClearThreshold;
            ProgressText.text = string.Format("{0} / {1} ft", BlockHeight, GameClearThreshold);
            MissText.text = string.Format("{0}/{1}", MissCount, GameOverThreshold);
            PlaceText.text = string.Format("{0}", BlockCount);
            InstabilityMeter.value = NormalizedInstability;
            InstabilityText.text = string.Format("{0}", balance);
        }
        else
        {
            MainUI.alpha = 0.0f;
            MainUI.interactable = false;
            MainUI.blocksRaycasts = false;
        }
        if (GoalAchieved)
        {
            
            if (!SeriousInstability)
            {
                if (!GameOver){ GameClear(); }
            }
            else
            {
                if (!GameOver) { GameOverInstability(); }
            }
        }
        else
        {
            if (SeriousInstability)
            {
                if (!GameOver) { GameOverInstability(); }
            }
            if (TooManyMissed)
            {
                if (!GameOver) { GameOverTooManyMissed(); }
            }
        }
    }

    private void GameOverTooManyMissed()
    {
        EndPanel.alpha = 1.0f;
        EndPanel.interactable = true;
        EndPanel.blocksRaycasts = true;
        message = string.Format(
            "Too many pieces missed.\n" +
            "The game ends here because of missing {0} pieces.\n" +
            "Please try again to stack a better tower.\n" +
            "Press R to Try Again.",
            GameOverThreshold
        );
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "StackHigh", "Missed too many times", score);
        GameOver = true;
        sound.StopAudio();
        LoseBGM();
    }

    private void GameOverInstability()
    {
        EndPanel.alpha = 1.0f;
        EndPanel.interactable = true;
        EndPanel.blocksRaycasts = true;
        if (GoalAchieved)
        {
            message =
            "I'm sorry, but your structure is not stable enough to complete.\n" +
            "Please try again to stack a better tower.\n" +
            "Press R to Try Again.";
        }
        else
        {
            message =
            "Oops! Your structure is too unstable to stand straight.\n" +
            "Please try again to stack a better tower.\n" +
            "Press R to Try Again.";
        }
        
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "StackHigh", "Serious instability detected!", score);
        GameOver = true;
        sound.StopAudio();
        LoseBGM();
    }

    private void GameClear()
    {
        EndPanel.alpha = 1.0f;
        EndPanel.interactable = true;
        EndPanel.blocksRaycasts = true;
        message = string.Format(
            "You did it!. You stacked a {0}-foot long tower!\n" +
            "You dropped {1} pieces.\n" +
            "Press R to Play Again.",
            BlockHeight, BlockCount
        );
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "StackHigh", score);
        GameOver = true;
        sound.StopAudio();
        WinBGM();
    }

    public void AddInstability(int amount, bool isRight)
    {
        //check if block is supporting ship
        bool isCorrecting = (balance > 0 && !isRight) || (balance < 0 && isRight);

        if (isCorrecting)
        {
            //half recovery, so it's more difficult to fix a lean
            balance += isRight ? (amount / 2) : -(amount / 2);
        }
        else
        {
            balance += isRight ? amount : -amount;
        }

        if(Mathf.Abs(balance) >= MaxInstability)
        {
            SeriousInstability = true;
        }
    }

    public void AddBlockCount() { BlockCount++; }
    public void AddMissCount()
    {
        MissCount++;
        if (MissCount >= GameOverThreshold) { TooManyMissed = true; }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        // Draws a vertical line at the intended center
        Gizmos.DrawLine(new Vector3(towerCenterX, -10, 0), new Vector3(towerCenterX, 10, 0));
    }

    public int GetBlockCount() {  return BlockCount; }
    public int GetMissCount() {  return MissCount; }
    public bool IsStructureInstable() { return SeriousInstability; }
    public bool IsCompleted() { return GoalAchieved; }

    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }

    public void CheckHeight()
    {

        Debug.Log("Height:" + BlockHeight);
        if (BlockHeight > GameClearThreshold) {
            GoalAchieved = true;
        }
    }
}
