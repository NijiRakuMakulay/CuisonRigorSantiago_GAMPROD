using GameAnalyticsSDK;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int balance;
    public int MaxInstability = 10; //game ends if instability hits thresholds of -10 or 10
    bool SeriousInstability = false;
    bool GoalAchieved = false;
    public float towerCenterX = 0.5f; //ship's center based on x position in scene
    public TextMeshProUGUI DisplayText;
    public TextMeshProUGUI ResultText;
    public CanvasGroup EndPanel;
    int BlockCount;
    int MissCount;
    float BlockHeight;
    public int GameOverThreshold = 3;
    public int GameClearThreshold = 10;
    string message;
    CraneController Crane;
    Dictionary<string, object> playResult = new Dictionary<string, object>();

    void Awake()
    {
        Crane = FindFirstObjectByType<CraneController>();
        if (Crane == null)
        {
            Debug.LogError("No CraneController found in the scene!");
        }
        GameAnalytics.Initialize();
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "StackHigh");
    }

    // Start is called before the first frame update
    void Start()
    {
        BlockCount = 0;
        SeriousInstability = false;
        GoalAchieved = false;
    }

    // Update is called once per frame
    void Update()
    {
        BlockHeight = Crane.getHeight();
        DisplayText.text = string.Format("Blocks placed:{0}\nBlocks Missed:{1}/{2}\nInstability:{3}/{4}\n10 or -10 is a loss\nHeight:{5}\n{6} is a win", BlockCount, MissCount, GameOverThreshold, balance, MaxInstability, BlockHeight, GameClearThreshold);
        ResultText.text = message;
        //Result Screen Handler
        if(MissCount >= GameOverThreshold)
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
        }
		else if(SeriousInstability)
		{
			EndPanel.alpha = 1.0f;
            EndPanel.interactable = true;
            EndPanel.blocksRaycasts = true;
            message =
                "Oops! Your structure is too unstable to stand straight.\n" +
                "Please try again to stack a better tower.\n" +
                "Press R to Try Again.";
		}
        else if (GoalAchieved)
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
        }
        else
        {
            message = " ";
            EndPanel.alpha = 0.0f;
            EndPanel.interactable = false;
            EndPanel.blocksRaycasts = false;
        }
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

        //game over placeholder
        if(Mathf.Abs(balance) >= MaxInstability)
        {
            SeriousInstability = true;
        }
		
		if(SeriousInstability)
		{
            playResult.Add("Reason for Game Over:", "Serious instability detected!");
            playResult.Add("Blocks Placed:", BlockCount);
            playResult.Add("Structure Height:", BlockHeight);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "StackHigh", playResult);
		}
    }

    public void AddBlockCount()
    {
        BlockCount++;
    }
    public void AddMissCount()
    {
        MissCount++;
        if (MissCount >= GameOverThreshold)
        {
            Debug.Log("Game Over. Miss count reached maximum miss threshold.");
            playResult.Add("Reason for Game Over:", "Missed three times");
            playResult.Add("Blocks Placed:", BlockCount);
            playResult.Add("Structure Height:", BlockHeight);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "StackHigh", playResult);
        }
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

    public void RestartLevel()
    {
        playResult.Clear();
        SceneManager.LoadScene(0);
    }

    public void CheckHeight()
    {
        Debug.Log("Height:" + BlockHeight);
        if (BlockHeight == GameClearThreshold) { Debug.Log("Okay! The final block! Place it well!"); }
        if (BlockHeight > GameClearThreshold) {
            GoalAchieved = true;
            playResult.Add("Reason for Game Over:", "Not Applicable. You cleared it!");
            playResult.Add("Blocks Placed:", BlockCount);
            playResult.Add("Structure Height:", BlockHeight);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "StackHigh", playResult);
        }
    }
}
