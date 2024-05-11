using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;    // Speed of camera movement
    public float lookSpeed = 2f;     // Speed of camera rotation

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Lock the cursor initially, but make it visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Camera movement
        float moveHorizontal = Input.GetAxis("Horizontal"); // A and D
        float moveVertical = Input.GetAxis("Vertical"); // W and S
        float moveUpDown = 0;

        if (Input.GetKey(KeyCode.E))
        {
            moveUpDown = 0.2f; // Move up with E key
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            moveUpDown = -0.2f; // Move down with Q key
        }

        Vector3 move = transform.right * moveHorizontal + transform.forward * moveVertical + transform.up * moveUpDown;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Camera rotation (only when right mouse button is held down)
        if (Input.GetMouseButton(1))
        {
            // Lock the cursor to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rotationX += Input.GetAxis("Mouse X") * lookSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            transform.eulerAngles = new Vector3(rotationY, rotationX, 0);
        }
        else
        {
            // Unlock the cursor when the right mouse button is not held down
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDisable()
    {
        // Unlock the cursor when the script is disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
