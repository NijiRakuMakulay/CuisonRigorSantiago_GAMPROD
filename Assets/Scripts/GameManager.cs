using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public int balance;
    public int MaxInstability = 10; //game ends if instability hits thresholds of -10 or 10
    bool SeriousInstability = false;
    public float towerCenterX = 0.5f; //ship's center based on x position in scene
    public TextMeshProUGUI DisplayText;
    public TextMeshProUGUI ResultText;
    public CanvasGroup EndPanel;
    int BlockCount;
    int MissCount;
    public int GameOverThreshold = 3;
    string message;
    // Start is called before the first frame update
    void Start()
    {
        BlockCount = 0;   
    }

    // Update is called once per frame
    void Update()
    {
        DisplayText.text = string.Format("Blocks placed:{0}\nBlocks Missed:{1}/{2}\nInstability:{3}/{4}\n10 or -10 is a loss", BlockCount, MissCount, GameOverThreshold, balance, MaxInstability);
        ResultText.text = message;
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
    }

    public void AddBlockCount()
    {
        BlockCount++;
    }
    public void AddMissCount()
    {
        MissCount++;
        if (MissCount >= GameOverThreshold) { Debug.Log("Game Over. Miss count reached maximum miss threshold."); }
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
}
