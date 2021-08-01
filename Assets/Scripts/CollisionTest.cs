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

public class CollisionTest : MonoBehaviour
{

    public GameObject target;
    public Canvas gameUI;

    private Dictionary<string, AsyncOperationHandle<Sprite>> spritelist = new Dictionary<string, AsyncOperationHandle<Sprite>>();

    GameObject endGame;
    GameObject gameover;
    public bool useThread=false;


    // Start is called before the first frame update
    void Start()
    {
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

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            print("Gamepad trigger");
            PushPuck();
        } else if (Mouse.current.rightButton.wasPressedThisFrame) {
            PullPuck();
        }

        if (target.transform.position.y < -20f)
        {
            target.transform.position = new Vector3(0, -target.transform.position.y, 0);
        }
    }

    public void DoRestart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);

    }

    Vector2 Get2dPos(GameObject o) {
        return new Vector2(o.transform.position.x,o.transform.position.z);

    }

    Vector3 Get3From2(Vector2 v) {
        return new Vector3(v.x,0,v.y);
    }

    public void PushPuck() {
        Vector2 current = Get2dPos(target);
        GameObject puck = GameObject.Find("puck");
        Vector2 pv = Get2dPos(puck);
        Vector2 push = pv-current;
        Rigidbody body=puck.GetComponent<Rigidbody>();
        body.AddForce(Get3From2(push.normalized*2000f),ForceMode.Impulse);

    }

    public void PullPuck() {
        Vector2 current = Get2dPos(target);
        GameObject puck = GameObject.Find("puck");
        Vector2 pv = Get2dPos(puck);
        Vector2 push = pv-current;
        Rigidbody body=puck.GetComponent<Rigidbody>();
        body.AddForce(Get3From2(push.normalized*-2000f),ForceMode.Impulse);
    }

}
