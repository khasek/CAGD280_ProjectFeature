using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerControls playerControls;
    private Vector3 position;
    private float moveSpeed = 3f;

    void Start()
    {
        playerControls = new PlayerControls();
        playerControls.Enable();
        position = transform.position;

        // Turn gravity off for now, but I may want it on later
        GetComponent<Rigidbody>().useGravity = false;
    }

    
    void Update()
    {
        Vector3 moveVector = playerControls.PlayerMovement.Fly.ReadValue<Vector3>();

        position.x += moveVector.x * moveSpeed * Time.deltaTime;
        position.y += moveVector.y * moveSpeed * Time.deltaTime;
        position.z += moveVector.z * moveSpeed * Time.deltaTime;

        transform.position = position;
    }
}
