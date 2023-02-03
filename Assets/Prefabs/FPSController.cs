using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    
    public Transform cam;
    public Transform body;
    public float mouseSenitivity = 300f;
    public float speed = 10f;
    float xRotation = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSenitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSenitivity * Time.deltaTime;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
        cam.Rotate(Vector3.up * mouseX);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        float h = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float v = Input.GetAxis("Vertical")   * speed * Time.deltaTime;

        transform.Translate(h, 0f, v);

    }
}
