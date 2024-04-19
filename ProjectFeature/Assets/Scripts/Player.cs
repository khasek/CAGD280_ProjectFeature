using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerControls playerControls;
    private float moveSpeed = 3f;


    void Start()
    {
        // Turn gravity off for now, but I may want it on later
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        playerControls = new PlayerControls();
        playerControls.Enable();
    }

    
    void FixedUpdate()
    {
        Vector3 moveVector = playerControls.PlayerMovement.Fly.ReadValue<Vector3>();
        rb.MovePosition(rb.position + moveVector * moveSpeed * Time.fixedDeltaTime);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Terrain")
        {
            print("Player collided with terrain!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            print("Player is standing in water!");
        }
    }
}
