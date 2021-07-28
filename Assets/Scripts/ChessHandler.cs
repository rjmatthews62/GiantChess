using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessHandler : MonoBehaviour
{
    // Start is called before the first frame update
    Camera myCam;
    public AudioClip bang; 
    public int square {get; set;}

    private GameRunner.GameMessage shutdown=null;

    void Start()
    {
        myCam=Camera.main;
        DoStopFalling();
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation=myCam.transform.rotation;
        transform.eulerAngles=new Vector3(0,transform.eulerAngles.y,0);
        if (transform.position.y<-20f) {
            if (shutdown==null) Destroy(this,0.5f);
            else shutdown(this.gameObject);
        }
    }
    void OnCollisionEnter() {
        print("Entered collision.");
        AudioSource sound=GetComponent<AudioSource> ();
        sound.Stop();
        sound.PlayOneShot(bang);
    }

    
    public void DoStartFalling(GameRunner.GameMessage tidy) {
        print("Start falling.");
        Collider c = GetComponent<Collider>();
        if (c!=null) c.enabled=false;
        for (int i=0; i<transform.childCount; i++) {
            c=transform.GetChild(i).transform.GetComponent<Collider>();
            if (c!=null) c.enabled=false;
        }
        shutdown=tidy;
        GetComponent<AudioSource>().Play();
    }

    public void DoStopFalling() {
        Collider c = GetComponent<Collider>();
        if (c!=null) c.enabled=true;
        for (int i=0; i<transform.childCount; i++) {
            c=transform.GetChild(i).transform.GetComponent<Collider>();
            if (c!=null) c.enabled=true;
        }
    }
}
