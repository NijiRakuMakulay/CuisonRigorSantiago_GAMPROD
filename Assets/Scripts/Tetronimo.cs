using UnityEngine;
using System.Collections;

public class Tetronimo : MonoBehaviour
{
    public CraneController Crane;
    Transform MissTrigger;
    GameManager Game;
    private bool hasLanded = false;
    void Awake()
    { 
        Game = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (Game == null) { Debug.LogError("Where is the Game Manager?"); }
        MissTrigger = GameObject.Find("MissBorder").transform;
        if (Game == null) { Debug.LogError("Where is the Miss Trigger?"); }
    }

    void Start()
    {
        Crane = FindFirstObjectByType<CraneController>();
        if (Crane == null)
        {
            Debug.LogError("No CraneController found in the scene!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.tag == "CurrentPiece")
        {
            if (collision.CompareTag("MissTrigger"))
            {
                Crane.Missed();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(hasLanded)
        {
            return;
        }
        else
        {
            if (collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Ground"))
            {
                hasLanded = true;
                StartCoroutine(CheckStability());
                Crane.Landed(this.gameObject);
            }
        }
        
    }

    IEnumerator CheckStability()
    {
        float rayLength = 0.6f; 
        yield return new WaitForSeconds(0.7f);

        foreach (Transform child in transform)
        {
            RaycastHit2D hit = Physics2D.Raycast(child.position, Vector2.down, rayLength);

            if (hit.collider != null)
            {
                child.GetComponent<SpriteRenderer>().color = Color.white;
            }
            else
            {
                //checks how far a block is from center
                float distance = Mathf.Abs(child.position.x - Game.towerCenterX);
                int calculatedAmount = Mathf.CeilToInt(distance);

                bool isRight = child.position.x > Game.towerCenterX;
                
                Game.AddInstability(calculatedAmount, isRight);
                //change colors of unstable blocks depending on if they're left instability, or right
                child.GetComponent<SpriteRenderer>().color = isRight ? Color.blue : Color.red;
            }
        }
    }
}