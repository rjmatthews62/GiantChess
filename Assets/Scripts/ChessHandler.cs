using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessHandler : MonoBehaviour
{
    // Start is called before the first frame update
    Camera myCam;
    public AudioClip bang;
    public AudioClip clunk;
    public int square { get; set; }
    public GameRunner runner;

    private GameRunner.GameMessage shutdown = null;
    private ChessHandler takePiece;
    public bool isMoving = false;
    public bool isFalling = true;

    void Start()
    {
        myCam = Camera.main;
        DoStopFalling();

    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = myCam.transform.rotation;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        if (transform.position.y < -20f)
        {
            if (shutdown == null) Destroy(this, 0.5f);
            else shutdown(this.gameObject);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        GameObject o = collision.gameObject;
        AudioSource sound = GetComponent<AudioSource>();
        if (collision.gameObject.tag != "board") print($"{gameObject.name} entered collision with {o.name} {o.tag} vel = {collision.relativeVelocity}");
        if (collision.gameObject.name.StartsWith("Player"))
        {
            GameObject player = collision.gameObject;
            Collider collider = player.GetComponent<Collider>();
            Vector3 pos = collider.transform.position;
            pos.y=-6f;
            collider.transform.position = pos;
        }
        else if (collision.gameObject.tag == "board")
        {
            if (isFalling) sound.Stop();
            isFalling = false;
            //print($"Speed={collision.relativeVelocity.y}");
            if (Mathf.Abs(collision.relativeVelocity.y)>=2) sound.PlayOneShot(bang);
        }
        else sound.PlayOneShot(clunk);

    }


    public void DoStartFalling(GameRunner.GameMessage tidy)
    {
        print("Start falling.");
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            c = transform.GetChild(i).transform.GetComponent<Collider>();
            if (c != null) c.enabled = false;
        }
        shutdown = tidy;
        GetComponent<AudioSource>().Play();
    }

    public void DoStopFalling()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = true;
        for (int i = 0; i < transform.childCount; i++)
        {
            c = transform.GetChild(i).transform.GetComponent<Collider>();
            if (c != null) c.enabled = true;
        }
    }

    public void MakeMove(int dest, ChessHandler destPiece)
    {
        int x = dest % 8;
        int y = dest / 8;
        Vector2 target = new Vector2(runner.SquarePos(x), runner.SquarePos(y));
        takePiece = destPiece;
        StartCoroutine("Moving", target);
        square = dest;
    }

    IEnumerator Moving(Vector2 target)
    {
        yield return new WaitForFixedUpdate();
        isMoving = true;
        Rigidbody body = GetComponent<Rigidbody>();
        //body.isKinematic = true;
        body.useGravity=false;
        float moveAmt = 0;
        float destHeight = 2f;
        if (gameObject.name.StartsWith("Kni")) destHeight = 6f;
        destHeight+=body.position.y;
        body.velocity=new Vector3(0,GameRunner.ChessSpeed,0);
        body.constraints=RigidbodyConstraints.None;
        while ( body.position.y < destHeight)
        {
            float delta = GameRunner.ChessSpeed * Time.fixedDeltaTime;
            moveAmt += delta;
//            transform.Translate(Vector3.up * delta);
            yield return new WaitForFixedUpdate();
        }
        while (true)
        {
            Vector2 current = new Vector2(transform.position.x, transform.position.z);
            float distance = Vector2.Distance(current, target);
            if (distance < 0.1f) break;
            if (distance < GameRunner.SquareSize && takePiece != null)
            {
                takePiece.DoStartFalling(runner.RemovePiece);
                takePiece = null;
            }
            Vector2 vel = (target-current).normalized;
            vel*=GameRunner.ChessSpeed; 
            body.velocity = new Vector3(vel.x,0,vel.y);
            //print($"Forward {body.velocity}");

            //Vector2 tmp = Vector2.Lerp(current, target, (GameRunner.ChessSpeed / distance) * Time.fixedDeltaTime);
            //transform.position = new Vector3(tmp.x, transform.position.y, tmp.y);
            yield return new WaitForFixedUpdate();
        };
        body.velocity=Vector3.zero;
        body.constraints=RigidbodyConstraints.FreezePositionX|RigidbodyConstraints.FreezePositionZ;

        //body.isKinematic = false;
        body.useGravity=true;
        yield return new WaitForSeconds(3);
        isMoving = false;
        runner.DoMovePiece();
    }
}
