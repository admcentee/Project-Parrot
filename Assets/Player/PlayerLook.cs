using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    public float unZoomedSensitivity = 150f;
    public float FOV = 110f;
    private bool locked = false;
    float xRotation = 0f;
    float yRotation = 0f;
    private float lookSensitivity = 0f;
    private bool initalized = true;


    void Start()
    {
        lookSensitivity = unZoomedSensitivity;
        // cam = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<Camera>();
        locked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //Keep cursor confined to game window

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (locked) { locked = false; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            else { locked = true; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Collider col = GetComponent<Collider>();
            if (col.enabled) col.enabled = false;
            else col.enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (GetComponent<Rigidbody>().useGravity) GetComponent<Rigidbody>().useGravity = false;
            else GetComponent<Rigidbody>().useGravity = true;
        }
        if (locked && initalized)
        {
            // GetComponent<Rigidbody>().freezeRotation = false;
            // cam.fieldOfView = FOV;
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

            // Debug.Log(mouseX + " - " + mouseY);

            xRotation -= mouseY;    //Vertical
            yRotation += mouseX;    //Horizontal

            //Clamp vertical FOV to +-85 degrees
            xRotation = Mathf.Clamp(xRotation, -85f, 85f);
            //Rotate camera vertically
            cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            //Rotate body horizontally
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
        }
        else
        {
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

    }
    public void Zoom(bool state)
    {
        if (state)
        {
            cam.fieldOfView = FOV / 2f;
            lookSensitivity = unZoomedSensitivity / 5f;
        }
        else
        {
            cam.fieldOfView = FOV;
            lookSensitivity = unZoomedSensitivity;
        }
    }
    public void Initialize()
    {
        initalized = true;
    }

}
