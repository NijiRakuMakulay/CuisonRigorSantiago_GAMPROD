using UnityEngine;

public class Tetronimo : MonoBehaviour
{
    public CraneController Crane;

    private bool hasLanded = false;

    void Start()
    {
        // Find the CraneController in the scene automatically
        Crane = FindFirstObjectByType<CraneController>();
        if (Crane == null)
        {
            Debug.LogError("No CraneController found in the scene!");
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(hasLanded == true)
        {
            return;
        }

        if(collision.gameObject.CompareTag("Block") || collision.gameObject.CompareTag("Ground"))
        {
            hasLanded = true;
            Crane.Landed(this.gameObject);
        }
    }
}
