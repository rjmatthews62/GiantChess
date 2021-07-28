using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameRunner : MonoBehaviour
{

    public GameObject target;

    private Sprite mySprite = null;
    private AsyncOperationHandle<Sprite> h;
    private Dictionary<string, AsyncOperationHandle<Sprite>> spritelist = new Dictionary<string, AsyncOperationHandle<Sprite>>();
    private GameObject newthing = null;
    private List<GameObject> chessPieces = new List<GameObject>();
    private const float SquareSize = 6;
    string[] pieces = {"PawnW","KnightW","RookW","BishopW","QueenW","KingW",
                       "PawnB","KnightB","RookB","BishopB","QueenB","KingB"};
    string[] layout = {"Rook","Knight","Bishop","King","Queen","Bishop","Knight","Rook",
                       "Pawn","Pawn","Pawn","Pawn","Pawn","Pawn","Pawn","Pawn"};
    bool populated = false;
    GameObject chess_reference;

    public delegate void GameMessage(GameObject thing);


    // Start is called before the first frame update
    void Start()
    {
        h = Addressables.LoadAssetAsync<Sprite>("Assets/Sprites/fire.png");
        foreach (string aname in pieces)
        {
            spritelist[aname] = Addressables.LoadAssetAsync<Sprite>("Assets/Sprites/" + aname + ".png");
        }

        for (int y = 0; y < 8; y++)
        {
            bool white = ((y % 2) == 0);
            for (int x = 0; x < 8; x++)
            {
                GameObject square = GameObject.CreatePrimitive(PrimitiveType.Cube);
                square.transform.localScale = new Vector3(SquareSize, 0.1f, SquareSize);
                square.transform.Translate(new Vector3(SquarePos(x), 0, SquarePos(y)), Space.World);
                square.GetComponent<Renderer>().material.color = (white ? Color.white : Color.black);
                white = !white;
            }
        }
        populated = true;
        chess_reference = GameObject.Find("chess");
        StartCoroutine("loadPieces", 5f);
    }

    private float SquarePos(int v)
    {
        return (v - 4f) * SquareSize;
    }

    private int dice(int size)
    { // 0 based.
        return Mathf.FloorToInt(Random.value * size);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (mySprite == null)
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    mySprite = h.Result;
                }
                else
                {
                    print("Sprite not loaded.");
                }
            };
            if (mySprite != null)
            {
                if (newthing != null) Destroy(newthing);
                GameObject oldthing = chess_reference;
                Vector3 newpos = new Vector3(SquarePos(dice(8)), dice(50) + 10, SquarePos(dice(8)));
                newthing = Instantiate<GameObject>(oldthing, newpos, Quaternion.identity);
                SpriteRenderer r = newthing.GetComponent<SpriteRenderer>();
                r.sprite = mySprite;
                newthing.SetActive(true);
            }

        }
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            if (!populated && Keyboard.current.lKey.wasPressedThisFrame) // L - load chess pieces.
            {
                populated = true;
                StartCoroutine("loadPieces");
            }
            if (Keyboard.current.escapeKey.wasPressedThisFrame) Application.Quit(); // ESC - Quit
            if (Keyboard.current.rKey.wasPressedThisFrame) // R - Reload
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);

            }
            if (Keyboard.current.tKey.wasPressedThisFrame) TakePiece(FindTarget(), RemovePiece); // T - Take a random piece
        }
        if ( Mouse.current.rightButton.wasPressedThisFrame) TakePiece(FindTarget(), RemovePiece);
        if (target.transform.position.y < -20f)
        {
            target.transform.position = new Vector3(0, -target.transform.position.y, 0);
        }
    }

    public GameObject FindTargetRay()
    {
        GameObject result = null;
        RaycastHit hit;
        Vector2 center = new Vector2(Screen.width/2,Screen.height/2);
        Ray ray = Camera.main.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                GameObject o = hit.collider.gameObject;
                print($"Found {o}");
                if (chessPieces.Contains(o)) result=o;
            }
        }
        return result;
    }

    public GameObject FindTarget()
    {
        float distance = Mathf.Infinity;
        GameObject nearest = null;
        CameraState cam = GetComponent<CinemachineVirtualCamera>().State;
        Vector3 pos = cam.FinalPosition;
        Vector3 looking = cam.FinalOrientation * Vector3.forward;
        print($"Looking={looking}");
        foreach (var o in chessPieces)
        {
            var targetDir = o.transform.position - pos;
            var angleBetween = Vector3.SignedAngle(targetDir,looking, Vector3.up);
            print($"{o.name} {angleBetween} {targetDir}");
            if (Mathf.Abs(angleBetween) < 15f)  
            {
                var mag = targetDir.magnitude;
                if (mag < distance)
                {
                    nearest = o;
                    distance = mag;
                }
            }
        }
        return nearest;
    }

    public void DoTakePiece()
    {
        if (chessPieces.Count > 0)
        {
            TakePiece(chessPieces[dice(chessPieces.Count)], RemovePiece);
        }
    }


    private void TakePiece(GameObject piece, GameMessage tidy)
    {
        if (piece == null) return;
        ChessHandler script = piece.GetComponent<ChessHandler>();
        if (script == null) return;
        script.DoStartFalling(tidy);
        chessPieces.Remove(piece); // Don't want this affecting the world any more.
    }

    private void RemovePiece(GameObject thing)
    {
        chessPieces.Remove(thing);
        Destroy(thing, 0.0f);
    }

    private IEnumerator loadPieces(float timeout)
    {
        int position = 0;
        print($"Waiting for {timeout} seconds.");
        yield return new WaitForSeconds(timeout);
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                string aname = layout[position++];
                aname += "W";
                var hh = spritelist[aname];
                while (hh.Status != AsyncOperationStatus.Succeeded) yield return new WaitForSeconds(0.5f);
                MakeChessPiece(hh.Result, x, y, aname.StartsWith("Pawn") ? 1f : 1.5f, aname+"_"+x,(y*8)+x);
                yield return new WaitForSeconds(0.2f);
            }
        }
        position = 0;
        for (int y = 7; y > 5; y--)
        {
            for (int x = 7; x >= 0; x--)
            {
                string aname = layout[position++];
                aname += "B";
                var hh = spritelist[aname];
                while (hh.Status != AsyncOperationStatus.Succeeded) yield return new WaitForSeconds(0.5f);
                MakeChessPiece(hh.Result, x, y, aname.StartsWith("Pawn") ? 1f : 1.5f, aname+"_"+x,(y*8)+x);
                yield return new WaitForSeconds(0.2f);
            }
        }
        TakePiece(chess_reference, TidyChess);
    }

    private void TidyChess(GameObject chess)
    {
        print("Deactivating Chess");
        chess.SetActive(false);
    }

    private GameObject MakeChessPiece(Sprite sprite, int x, int y, float height, string aname, int square)
    {
        GameObject oldthing = chess_reference;
        Vector3 newpos = new Vector3(SquarePos(x), 20, SquarePos(y));
        GameObject thing = Instantiate<GameObject>(oldthing, newpos, Quaternion.identity);
        SpriteRenderer r = thing.GetComponent<SpriteRenderer>();
        ChessHandler script = thing.GetComponent<ChessHandler>();
        script.square=square;
        r.sprite = sprite;
        Vector3 scale = thing.transform.localScale;
        scale.y *= height;
        thing.transform.localScale = scale;
        thing.name=aname;
        chessPieces.Add(thing);
        return thing;
    }

}
