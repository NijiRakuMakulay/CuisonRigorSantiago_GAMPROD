using UnityEngine;

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
        // Find the CraneController in the scene automatically
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
                Crane.Landed(this.gameObject);
            }
        }
        
    }
}