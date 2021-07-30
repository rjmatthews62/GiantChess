using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;
using RoughChess;

public class GameRunner : MonoBehaviour
{

    public GameObject target;
    public Canvas gameUI;

    private Sprite mySprite = null;
    private AsyncOperationHandle<Sprite> h;
    private Dictionary<string, AsyncOperationHandle<Sprite>> spritelist = new Dictionary<string, AsyncOperationHandle<Sprite>>();
    private GameObject newthing = null;
    private List<GameObject> chessPieces = new List<GameObject>();
    public const float SquareSize = 6;
    public const float ChessSpeed = 6;

    public bool useThread = false;


    string[] pieces = {"PawnW","KnightW","RookW","BishopW","QueenW","KingW",
                       "PawnB","KnightB","RookB","BishopB","QueenB","KingB"};
    string[] layout = {"Rook","Knight","Bishop","King","Queen","Bishop","Knight","Rook",
                       "Pawn","Pawn","Pawn","Pawn","Pawn","Pawn","Pawn","Pawn"};
    bool populated = false;

    GameObject chess_reference;
    GameObject endGame;
    GameObject gameover;

    public delegate void GameMessage(GameObject thing);

    ChessLogic myLogic = new ChessLogic();
    AiState myState = null;

    public class Move
    {
        public int from, dest;
        public Move(int from, int dest)
        {
            this.from = from;
            this.dest = dest;
        }
    }

    public List<Move> movelist = new List<Move>();


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
                square.tag = "board";
                square.name = $"Board{x}{y}";
                white = !white;
            }
        }
        populated = true;
        chess_reference = GameObject.Find("chess");
        //PopulateMoveList();
        endGame = GameObject.Find("EndGame");
        gameover = GameObject.Find("GameOver");
        endGame.SetActive(false);
#if UNITY_WEBGL
        useThread = false;
#else
        useThread = true;
#endif
#if UNITY_ANDROID
        gameUI.gameObject.SetActive(true);
#else         
        gameUI.gameObject.SetActive(false);
#endif

        StartCoroutine("loadPieces", 5f);
    }

    private void PopulateMoveList()
    {
        movelist.Clear();
        movelist.Add(new Move(57, 40));
        movelist.Add(new Move(13, 21));
        movelist.Add(new Move(53, 37));
        movelist.Add(new Move(10, 18));
        movelist.Add(new Move(40, 34));
        movelist.Add(new Move(21, 29));
        movelist.Add(new Move(56, 57));
        movelist.Add(new Move(11, 27));
        movelist.Add(new Move(34, 44));
        movelist.Add(new Move(15, 31));
        movelist.Add(new Move(60, 53));
        movelist.Add(new Move(3, 17));
        movelist.Add(new Move(51, 35));
        movelist.Add(new Move(12, 20));
        movelist.Add(new Move(55, 47));
        movelist.Add(new Move(17, 3));
        movelist.Add(new Move(59, 43));
        movelist.Add(new Move(3, 39));
        movelist.Add(new Move(53, 45));
        movelist.Add(new Move(5, 19));
        movelist.Add(new Move(52, 36));
        movelist.Add(new Move(39, 37));
        movelist.Add(new Move(45, 52));
        movelist.Add(new Move(29, 36));
        movelist.Add(new Move(43, 59));
        movelist.Add(new Move(8, 16));
        movelist.Add(new Move(44, 27));
        movelist.Add(new Move(37, 39));
        movelist.Add(new Move(27, 17));
        movelist.Add(new Move(0, 8));
        movelist.Add(new Move(17, 2));
        movelist.Add(new Move(8, 0));
        movelist.Add(new Move(2, 19));
        movelist.Add(new Move(4, 12));
        movelist.Add(new Move(19, 9));
        movelist.Add(new Move(39, 21));
        movelist.Add(new Move(54, 46));
        movelist.Add(new Move(12, 11));
        movelist.Add(new Move(9, 26));
        movelist.Add(new Move(11, 3));
        movelist.Add(new Move(26, 9));
        movelist.Add(new Move(3, 2));
        movelist.Add(new Move(9, 19));
        movelist.Add(new Move(2, 10));
        movelist.Add(new Move(19, 4));
        movelist.Add(new Move(10, 2));
        movelist.Add(new Move(4, 21));
        movelist.Add(new Move(14, 21));
        movelist.Add(new Move(57, 56));
        movelist.Add(new Move(2, 10));
        movelist.Add(new Move(58, 37));
        movelist.Add(new Move(20, 28));
        movelist.Add(new Move(35, 28));
        movelist.Add(new Move(21, 28));
        movelist.Add(new Move(37, 28));
        movelist.Add(new Move(10, 17));
        movelist.Add(new Move(28, 7));
        movelist.Add(new Move(17, 9));
        movelist.Add(new Move(50, 42));
        movelist.Add(new Move(18, 26));
        movelist.Add(new Move(49, 41));
        movelist.Add(new Move(16, 24));
        movelist.Add(new Move(52, 44));
        movelist.Add(new Move(0, 8));
        movelist.Add(new Move(59, 31));
        movelist.Add(new Move(9, 10));
        movelist.Add(new Move(31, 26));
        movelist.Add(new Move(10, 3));
        movelist.Add(new Move(26, 8));
        movelist.Add(new Move(1, 18));
        movelist.Add(new Move(8, 15));
        movelist.Add(new Move(18, 8));
        movelist.Add(new Move(15, 6));
        movelist.Add(new Move(3, 10));
        movelist.Add(new Move(44, 36));
        movelist.Add(new Move(10, 18));
        movelist.Add(new Move(62, 52));
        movelist.Add(new Move(18, 26));
        movelist.Add(new Move(6, 20));
        movelist.Add(new Move(26, 25));
        movelist.Add(new Move(36, 27));
        movelist.Add(new Move(24, 32));
        movelist.Add(new Move(41, 32));
        movelist.Add(new Move(25, 32));
        movelist.Add(new Move(20, 16));
    }

    public float SquarePos(int v)
    {
        return (v - 4f) * SquareSize;
    }

    public static int dice(int size)
    { // 0 based.
        return Mathf.FloorToInt(UnityEngine.Random.value * size);
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
                Vector3 newpos = new Vector3(transform.position.x, dice(50) + 10, transform.position.z);
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
            if (Keyboard.current.rKey.wasPressedThisFrame) DoRestart(); // R - Reload

            if (Keyboard.current.tKey.wasPressedThisFrame) TakePiece(FindTarget(), RemovePiece); // T - Take a random piece
            if (Keyboard.current.mKey.wasPressedThisFrame) DoMovePiece(); // M - next move.
        }
        if (Mouse.current.rightButton.wasPressedThisFrame) TakePiece(FindTarget(), RemovePiece);
        ProcessMove();
        if (target.transform.position.y < -20f)
        {
            target.transform.position = new Vector3(0, -target.transform.position.y, 0);
        }
    }

    public void DoMovePiece()
    {
        /* if (movelist.Count > 0)
        {
            Move move = movelist[0];
            movelist.RemoveAt(0);
            ExecuteMove(move);
        }*/
        if (useThread)
        {
            print("Calculating move (in thread)");
            myLogic.AiMoveThreaded(myLogic.player, myLogic.boardPos, 1);
        }
        else
        {
            print("Calculating move (state machine)");
            myState = myLogic.AiMoveState(myLogic.player, myLogic.boardPos, 1);

        }

    }

    public void DoRestart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);

    }

    public void ProcessMove()
    {
        AiResult result;
        if (useThread)
        {
            if (!myLogic.threadReady) return;

            result = myLogic.threadResult;
            myLogic.threadReady = false;

        }
        else
        {
            if (myState == null) return;
            if (!myLogic.AiMoveNext(ref myState)) return;
            result = myState.result;
            myState = null;
        }

        if (result.success)
        {
            myLogic.ExecuteMove(result.from.AsMove(), result.dest.AsMove());
            ExecuteMove(new Move(result.from.AsIndex(), result.dest.AsIndex()));
        }
        else
        {
            string message = "Game Over\n" + (myLogic.isCheck() ? "Checkmate" : "Draw");
            gameover.GetComponent<Text>().text = message;
            endGame.SetActive(true);
        }
    }

    private void ExecuteMove(Move move)
    {
        ChessHandler fromPiece = FindBySquare(move.from);
        ChessHandler toPiece = FindBySquare(move.dest);
        if (fromPiece == null)
        {
            print("No piece found.");
            return;
        }
        print($"Moving {fromPiece.gameObject.name} from {move.from} to {move.dest}");
        fromPiece.MakeMove(move.dest, toPiece);
    }

    private ChessHandler FindBySquare(int square)
    {
        foreach (GameObject o in chessPieces)
        {
            ChessHandler h = o.GetComponent<ChessHandler>();
            if (h.square == square) return h;
        }
        return null;
    }

    public GameObject FindTargetRay()
    {
        GameObject result = null;
        RaycastHit hit;
        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(center);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                GameObject o = hit.collider.gameObject;
                print($"Found {o}");
                if (chessPieces.Contains(o)) result = o;
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
            var angleBetween = Vector3.SignedAngle(targetDir, looking, Vector3.up);
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
        TakePiece(FindTarget(), RemovePiece);
    }


    private void TakePiece(GameObject piece, GameMessage tidy)
    {
        if (piece == null) return;
        ChessHandler script = piece.GetComponent<ChessHandler>();
        if (script == null) return;
        script.DoStartFalling(tidy);
        chessPieces.Remove(piece); // Don't want this affecting the world any more.
    }

    public void RemovePiece(GameObject thing)
    {
        chessPieces.Remove(thing);
        Destroy(thing, 0.0f);
    }

    private IEnumerator loadPieces(float timeout)
    {
        print($"Waiting for {timeout} seconds.");
        yield return new WaitForSeconds(timeout);
        for (int i = 0; i < 64; i++)
        {
            string aname = "?";
            AsyncOperationHandle<Sprite> hh;
            string piece = myLogic.boardPos.Substring(i, 1);
            if (piece != " ")
            {
                try
                {
                    aname = myLogic.pieces[piece];
                    hh = spritelist[aname];
                }
                catch (System.Exception e)
                {
                    print($"Missing piece {piece} {aname} Error= {e}");
                    continue;
                }
                while (hh.Status != AsyncOperationStatus.Succeeded) yield return new WaitForSeconds(0.5f);
                int x = i % 8;
                int y = i / 8;
                MakeChessPiece(hh.Result, x, y, aname.StartsWith("Pawn") ? 1f : 1.5f, aname + "_" + x, (y * 8) + x);
                yield return new WaitForSeconds(0.2f);
            }
        }
        TakePiece(chess_reference, TidyChess);
    }

    private void TidyChess(GameObject chess)
    {
        print("Deactivating Chess");
        chess.SetActive(false);
        DoMovePiece();
    }

    private GameObject MakeChessPiece(Sprite sprite, int x, int y, float height, string aname, int square)
    {
        GameObject oldthing = chess_reference;
        Vector3 newpos = new Vector3(SquarePos(x), 20, SquarePos(y));
        GameObject thing = Instantiate<GameObject>(oldthing, newpos, Quaternion.identity);
        SpriteRenderer r = thing.GetComponent<SpriteRenderer>();
        ChessHandler script = thing.GetComponent<ChessHandler>();
        script.square = square;
        script.runner = this;
        script.isFalling = true;
        r.sprite = sprite;
        Vector3 scale = thing.transform.localScale;
        scale.y *= height;
        thing.transform.localScale = scale;
        thing.name = aname;
        thing.tag = "piece";
        chessPieces.Add(thing);
        return thing;
    }

}
