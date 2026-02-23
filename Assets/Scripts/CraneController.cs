using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CraneController : MonoBehaviour
{
    GameManager Game;
    public GameObject[] TetoPrefabs;
    public Transform Tetspawn;
    int Misses;
    bool GameOverDueToInstability;

    public float speed = 2f;
    public float leftLim = 0f;
    public float rightLim = 11f;

    private bool movingRight = true;
    private GameObject currentTet;
    private Rigidbody2D currentRB;
    private bool falling = false;
    float gridSize = 1f;

    public Camera mainCamera;
    public float cameraStep = 1f;
    public float craneStep = 1f;
    public float heightThreshold = 4f;
    public float smoothTime = 0.3f; // Time in seconds to reach the target
    private Vector3 cameraVelocity = Vector3.zero;
    private Vector3 craneVelocity = Vector3.zero;

    private float highestReachedY;

    Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / gridSize) * gridSize,
            Mathf.Round(pos.y / gridSize) * gridSize,
            pos.z
        );
    }

    Vector3 SnapTetronimoParentToGrid(GameObject tet)
    {
        if (tet.transform.childCount == 0) return tet.transform.position;

        Transform lowestChild = tet.transform.GetChild(0);
        Transform leftMostChild = tet.transform.GetChild(0);

        foreach (Transform child in tet.transform)
        {
            if (child.position.y < lowestChild.position.y)
                lowestChild = child;
            if (child.position.x < leftMostChild.position.x)
                leftMostChild = child;
        }

        float offsetX = Mathf.Round(leftMostChild.position.x) - leftMostChild.position.x;
        float offsetY = Mathf.Round(lowestChild.position.y) - lowestChild.position.y;
        
        tet.transform.position += new Vector3(offsetX, offsetY, 0);

        return tet.transform.position;
    }

    public GameObject foundationObject;

    void Awake()
    {
        Game = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (Game == null)
        {
            Debug.LogError("Where is the Game Manager?");
        }
        else
        {
            Misses = Game.GetMissCount();
        }
    }

    void Start()
    {
        highestReachedY = heightThreshold;
        SpawnTet();
    }

    void Update()
    {
        Misses = Game.GetMissCount();
        GameOverDueToInstability = Game.IsStructureInstable();
        AutoMove();
        HandleInput();
        if (Input.GetKeyDown(KeyCode.R)) { SceneManager.LoadScene(0); }
    }

    void AutoMove()
    {
        if (Misses >= Game.GameOverThreshold) return;
        else if (GameOverDueToInstability) return;
        float moveAmount = speed * Time.deltaTime;
        if (currentTet == null || falling == true) return;

        float currentY = transform.position.y; 

        if (movingRight)
        {
            transform.position = new Vector3(transform.position.x + moveAmount, currentY, transform.position.z);
            if (transform.position.x >= rightLim) movingRight = false;
        }
        else
        {
            transform.position = new Vector3(transform.position.x - moveAmount, currentY, transform.position.z);
            if (transform.position.x <= leftLim) movingRight = true;
        }
    }

    void HandleInput()
    {
        if (currentTet == null || falling == true || Misses >= Game.GameOverThreshold || GameOverDueToInstability) return;

        if (Input.GetMouseButtonDown(1))
        {
            currentTet.transform.Rotate(0, 0, 90);
        }

        if (Input.GetMouseButtonDown(0))
        {
            DropTet();
        }

        currentTet.transform.position = Tetspawn.position;
    }

    public void Missed()
    {
        currentTet.tag = "MissedPiece";
        falling = false;
        Destroy(currentTet.gameObject);
        currentTet = null;
        currentRB = null;
        Game.AddMissCount();
        NextPiece();
    }

    public void Landed(GameObject landedTet)
    {
        Game.AddBlockCount();
        StartCoroutine(SnapAfterPhysics(landedTet));
    }

    IEnumerator SnapAfterPhysics(GameObject landedTet)
    {
        yield return new WaitForSeconds(0.3f);

        Rigidbody2D rb = landedTet.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        SnapTetronimoParentToGrid(landedTet);

        rb.bodyType = RigidbodyType2D.Static;

        //camera
        float trueTopY = landedTet.transform.position.y;
        foreach (Transform child in landedTet.transform)
        {
            if (child.position.y > trueTopY)
            {
                trueTopY = child.position.y;
            }
        }

        if (trueTopY > heightThreshold)
        {
            if (trueTopY > highestReachedY)
            {
                highestReachedY = trueTopY;
            }

            float cameraTargetY = highestReachedY;
            
            float craneTargetY = cameraTargetY + mainCamera.orthographicSize - 0.5f;

            Vector3 cameraTarget = new Vector3(mainCamera.transform.position.x, cameraTargetY, mainCamera.transform.position.z);
            Vector3 craneTarget = new Vector3(transform.position.x, craneTargetY, transform.position.z);

            float elapsed = 0;
            while (elapsed < smoothTime * 2)
            {
                mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, cameraTarget, ref cameraVelocity, smoothTime);
                
                float newCraneY = Vector3.SmoothDamp(transform.position, craneTarget, ref craneVelocity, smoothTime).y;
                transform.position = new Vector3(transform.position.x, newCraneY, transform.position.z);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        currentTet.tag = "Block";
        NextPiece();
    }
    
    void SpawnTet()
    {
        int index = Random.Range(0, TetoPrefabs.Length);
        currentTet = Instantiate(TetoPrefabs[index], Tetspawn.position, Quaternion.identity);
        currentTet.tag = "CurrentPiece";
        Collider2D col = currentTet.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Tetronimo t = currentTet.GetComponent<Tetronimo>();
        if (t != null)
        t.Crane = this;

        currentRB = currentTet.GetComponent<Rigidbody2D>();
        currentRB.bodyType = RigidbodyType2D.Kinematic;
        currentRB.gravityScale = 0f;
    }

    void DropTet()
    {
        if (currentTet == null)
        {
            return;
        }

        Collider2D col = currentTet.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        currentTet.transform.parent = null; 
        falling = true;

        currentRB.bodyType = RigidbodyType2D.Dynamic;
        currentRB.gravityScale = 1f;
    }

    void NextPiece()
    {
        falling = false;
        currentTet = null;
        currentRB = null;
        SpawnTet();
    }
}