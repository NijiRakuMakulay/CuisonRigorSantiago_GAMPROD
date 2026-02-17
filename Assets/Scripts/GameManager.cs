using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
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
        DisplayText.text = string.Format("Blocks placed:{0}\nBlocks Missed:{1}/{2}", BlockCount, MissCount, GameOverThreshold);
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
        else
        {
            message = " ";
            EndPanel.alpha = 0.0f;
            EndPanel.interactable = false;
            EndPanel.blocksRaycasts = false;
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

    public int GetBlockCount() {  return BlockCount; }
    public int GetMissCount() {  return MissCount; }
}
