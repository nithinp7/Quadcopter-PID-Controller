using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QControl : MonoBehaviour
{
    public float outp;
    [Header("Controls")]
    public float thrust = 0.0f;
    public float yaw = 0.0f;
    public float pitch = 0.0f;
    public float roll = 0.0f;

    [Header("PID Altitude")]
    public float prop_alt = 1.5f;
    public float der_alt = 1.0f;
    public float int_alt = 0.0f;
    private float cumErr_alt = 0.0f;

    [Header("PID Tilt")]
    public float prop_tilt = 1.0f;
    public float der_tilt = 0.5f;
    public float int_tilt = 0.0f;
    private float cumErr_tiltLeft = 0.0f;
    private float cumErr_tiltForward = 0.0f;

    [Header("PID Position")]
    public float prop_pos = 1.0f;
    public float der_pos = 0.5f;
    public float int_pos = 0.0f;
    private float cumErr_posX = 0.0f;
    private float cumErr_posZ = 0.0f;

    [Header("Target Tilt (DEBUG ONLY)")]
    public float tiltLeft = 0.0f;
    public float tiltForward = 0.0f;

    [Header("Autopilot")]
    public bool holdAltitude = true;
    public float altitude = 10.0f;
    public bool hover = false;
    public float hoverAggression = 5.0f;
    public bool holdPosition = false;
    public float posX = 100.0f;
    public float posZ = 100.0f;

    Rigidbody rb;
    Motor m1, m2, m3, m4;

    class Motor
    {
        public GameObject g;
        public QMotor motorScript;

        public Motor(GameObject g_)
        {
            g = g_;
            motorScript = g.GetComponent<QMotor>();
        }
    }

    void Start()
    {
        rb = this.gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody>();

        m1 = new Motor(this.gameObject.transform.GetChild(1).gameObject);
        m2 = new Motor(this.gameObject.transform.GetChild(2).gameObject);
        m3 = new Motor(this.gameObject.transform.GetChild(3).gameObject);
        m4 = new Motor(this.gameObject.transform.GetChild(4).gameObject);
    }

    private void setTilt(float tx, float tz)
    {
        tiltLeft = tx;
        tiltForward = tz;
        cumErr_tiltLeft = 0;
        cumErr_tiltForward = 0;
    }

    void Update()
    {
        //CURRENT STATE
        Vector3 p = rb.position;
        Vector3 v = rb.velocity;
        Vector3 locv = rb.transform.InverseTransformDirection(v);

        //INPUT
        if (Input.GetKey(KeyCode.DownArrow)) tiltForward -= 0.01f;
        if (Input.GetKey(KeyCode.UpArrow)) tiltForward += 0.01f;
        tiltForward = Mathf.Clamp(tiltForward, -2.0f, 2.0f);

        if (Input.GetKey(KeyCode.RightArrow)) tiltLeft -= 0.01f;
        if (Input.GetKey(KeyCode.LeftArrow)) tiltLeft += 0.01f;
        tiltLeft = Mathf.Clamp(tiltLeft, -2.0f, 2.0f);

        if (Input.GetKey(KeyCode.Q)) altitude -= 0.01f;
        if (Input.GetKey(KeyCode.E)) altitude += 0.01f;
        altitude = Mathf.Clamp(altitude, 0.0f, 100.0f);

        if (Input.GetKey(KeyCode.X)) yaw = -3.0f;
        else if (Input.GetKey(KeyCode.Z)) yaw = 3.0f;
        else yaw = 0.0f;
        yaw = Mathf.Clamp(yaw, -5.0f, 5.0f);

        if (Input.GetKeyUp(KeyCode.Space))
        {
            hover = !hover;
            setTilt(0.0f, 0.0f);
        } else
        {
            tiltLeft = -8.0f * Mathf.Pow(2.0f * Input.mousePosition.x / Screen.width - 1.0f, 3);
            tiltForward = 8.0f * Mathf.Pow(2.0f * Input.mousePosition.y / Screen.height - 1.0f, 3);
        }

        if (Input.GetKeyUp(KeyCode.H))
        {
            posX = p.x;
            posZ = p.z;
            hover = false;
            holdPosition = !holdPosition;
            cumErr_posX = 0.0f;
            cumErr_posZ = 0.0f;
            setTilt(0.0f, 0.0f);
        }

        //CONTROL SYSTEM
        Vector3 Nt = new Vector3(tiltLeft, tiltForward, 1.0f).normalized;//rb.transform.InverseTransformDirection(new Vector3(tiltX, 1.0f, tiltZ)).normalized;
        Vector3 N = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 locAngV = rb.transform.InverseTransformDirection(rb.angularVelocity);

        if (hover)
        {
            float tx = Mathf.Clamp(hoverAggression * 0.01f * -v.x * Mathf.Abs(v.x), -2.0f, 2.0f);
            float tz = Mathf.Clamp(hoverAggression * 0.01f * -v.z * Mathf.Abs(v.z), -2.0f, 2.0f);
            //Vector3 locTilt 
            Nt = rb.transform.InverseTransformDirection(new Vector3(tx, 1.0f, tz)).normalized;
        }

        //TODO remove redundant clamps
        //tiltLeft = Mathf.Clamp(tiltLeft, -2.0f, 2.0f);
        //tiltForward = Mathf.Clamp(tiltForward, -2.0f, 2.0f);

        /*
        if (holdPosition)
        {
            float errX = posX - p.x;
            float errZ = posZ - p.z;

            tiltLeft = prop_pos * errX - der_pos * v.x + int_pos * cumErr_posX;
            tiltForward = prop_pos * errZ - der_pos * v.z + int_pos * cumErr_posZ;

            //tiltX = Mathf.Clamp(tiltX, -0.5f, 0.5f);
            //tiltForward = Mathf.Clamp(tiltForward, -0.5f, 0.5f);
            tiltLeft = Mathf.Clamp(tiltLeft, -2.0f, 2.0f);
            tiltForward = Mathf.Clamp(tiltForward, -2.0f, 2.0f);

            cumErr_posX += errX;
            cumErr_posZ += errZ;
        }
        */

        Vector3 crs = Vector3.Cross(Nt, N);
        //Vector3 crsV = locAngV;// Vector3.Cross(locAngV, N);//??-locAngV

        if (holdAltitude)
        {
            float err = altitude - p.y;
            thrust = prop_alt * err - der_alt * v.y + int_alt * cumErr_alt;
            thrust = Mathf.Clamp(thrust, 0.0f, 3.0f);
            cumErr_alt += err;
        }
        else 
            cumErr_alt = 0.0f;

        pitch = prop_tilt * crs.x - der_tilt * locAngV.x + int_tilt * cumErr_tiltLeft;
        pitch = Mathf.Clamp(pitch, -1.0f, 1.0f);
        cumErr_tiltLeft += crs.x;

        roll = prop_tilt * crs.y - der_tilt * locAngV.y + int_tilt * cumErr_tiltForward;
        roll = Mathf.Clamp(roll, -1.0f, 1.0f);
        cumErr_tiltForward += crs.y;

        //if (locv.magnitude > 20)
        //    yaw = -prop_tilt * Vector3.Cross(locv, N).z;// - der_tilt * locAngV.z;

        m1.motorScript.thrust = thrust - yaw + pitch + roll;
        m2.motorScript.thrust = thrust + yaw + pitch - roll;
        m3.motorScript.thrust = thrust - yaw - pitch - roll;
        m4.motorScript.thrust = thrust + yaw - pitch + roll;
    }
}

