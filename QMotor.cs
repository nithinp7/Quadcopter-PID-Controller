using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QMotor : MonoBehaviour
{
    public Rigidbody blade;

    public bool blenderCoords = false;
    public bool motorDir = false;
    public float thrust = 0.0f;

    Rigidbody rb;
    Vector3 up;
    Vector3 eul;
    float mDir = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (blenderCoords)
        {
            up = Vector3.forward;
            eul = new Vector3(1, 0, 0);
        }
        else
        {
            up = Vector3.up;
            eul = new Vector3(0, 0, 1);
        }

        if (motorDir)
            mDir = -1.0f;

        blade.maxAngularVelocity = 1000.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (blade.gameObject.GetComponent<ConfigurableJoint>() == null)
            return;
        
        thrust = Mathf.Clamp(thrust, 0, 5);
        rb.AddRelativeForce(up * thrust);
        rb.AddRelativeTorque(200.0f * mDir * up * thrust);
        blade.AddRelativeTorque(200.0f * -up * mDir * thrust);
    }
}
