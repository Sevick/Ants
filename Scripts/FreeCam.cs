using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour
{
    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private float initYaw;
    private float initPitch;

    private bool movingCam = false;

    void Update() {

        if (Input.GetMouseButton(1)) {
            if (!movingCam) {
                initYaw = transform.eulerAngles[0];
                initPitch = transform.eulerAngles[1];
                movingCam = true;
            }
            else {
                yaw += speedH * Input.GetAxis("Mouse X");
                pitch -= speedV * Input.GetAxis("Mouse Y");
                transform.Rotate(pitch, yaw, 0.0f);
            }
        }
        else {
            movingCam = false;
        }
    }
}
