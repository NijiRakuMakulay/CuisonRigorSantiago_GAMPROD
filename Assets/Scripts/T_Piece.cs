using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class T_Piece : MonoBehaviour
{
    public Tetronimo tPiece;
    CraneController crane;
    Sprite[] blockSkins;
    Sprite placedSkin;
    SpriteRenderer blockrenderer;
    bool blockLanded;
    GameObject[] Blocks;
    void Awake()
    {
        tPiece = this.transform.parent.gameObject.GetComponent<Tetronimo>();
        crane = GameObject.Find("Crane").GetComponent<CraneController>();
        blockrenderer = GetComponent<SpriteRenderer>();
        placedSkin = Resources.Load<Sprite>("Blocks/Skins/Crate_PlacedBlock");
        blockSkins = new Sprite[crane.TetoPrefabs.Length];
        for (int x = 0; x < crane.TetoPrefabs.Length; x++)
        {
            string respath = string.Format("Blocks/Skins/Crate_Piece{0}", x + 1);
            blockSkins[x] = Resources.Load<Sprite>(respath);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        blockrenderer.sprite = blockSkins[tPiece.GetBlockID()];
    }

    // Update is called once per frame
    void Update()
    {
        Blocks = GameObject.FindGameObjectsWithTag("IndividualBlock");
        blockLanded = tPiece.LandCheck();
        if (blockLanded) { blockrenderer.sprite = placedSkin; }
        else { blockrenderer.sprite = blockSkins[tPiece.GetBlockID()]; }
    }
}
