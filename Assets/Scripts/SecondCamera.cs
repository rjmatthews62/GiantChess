using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondCamera : MonoBehaviour
{
    float speed=0;
    const float maxSpeed=10f;
    [Tooltip("Max Camera acceleration.")]
    public float acceleration=20f;
    [Tooltip("Target to follow")]
    public GameObject target;
    Rigidbody myBody;
    [Tooltip("Cameraheight above target.")]
    public float cameraHeight=3f;
    // Start is called before the first frame update
        void Start()
    {
        //puck=GameObject.Find("puck");
        myBody=GetComponent<Rigidbody>();
        //cameraHeight=transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 newpos=target.transform.position;
        Vector3 camerapos=transform.position;
        float newHeight=newpos.y+cameraHeight;
        newpos.y=newHeight;
        Vector3 delta=(newpos-camerapos);
        float newspeed=0;
/*        float speedDiff=acceleration*Time.fixedDeltaTime;
        if (Mathf.Abs(speed-newspeed) <speedDiff) newspeed=speed;
        else if (newspeed>speed) newspeed=speed+speedDiff;
        else if (newspeed<speed) newspeed=speed-speedDiff;
        speed=Mathf.Clamp(newspeed,0,maxSpeed);} */
        //delta=delta.normalized*speed;
        //newpos= camerapos+delta*Time.fixedDeltaTime;
        //transform.position=newpos; //Vector3.Slerp(camerapos,newpos,0.5f);

        newpos-=delta.normalized*3f; // Stay 3 units away.
        newpos.y= newHeight;
        delta=newpos-camerapos;
        newspeed=delta.magnitude*2f;
        if (newspeed>acceleration) delta=delta.normalized*acceleration;
        else if (newspeed<0.2f) delta=Vector3.zero;
        myBody.AddForce(delta,ForceMode.Acceleration);
        transform.LookAt(target.transform.position);
    }
}
