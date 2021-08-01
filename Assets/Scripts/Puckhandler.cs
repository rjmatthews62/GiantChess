using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puckhandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.position.y<-20f) {
            transform.position=new Vector3(0,10f,0);
        }
    }
}
