using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.FilePathAttribute;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerControls playerControls;
    private float moveSpeed;
    private float walkSpeed = 4f;
    private float swimSpeed = 2f;
    private float speedBoost = 4f;
    private float jumpForce = 5f;

    private Vector3 playerRotation;
    private Vector3 cameraRotation;
    [SerializeField] private Transform cam;
    private float sensitivity = 8f;

    private bool isWalking = true;
    private bool isSwimming = false;
    private bool isFlying = false;

    private int waterColliderCount = 0;

    private delegate void MoveDelegate(Vector3 moveVector);
    private MoveDelegate move;

    void Start()
    {
        // Turn gravity off for now, but I may want it on later
        rb = GetComponent<Rigidbody>();

        playerControls = new PlayerControls();
        playerControls.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        playerRotation = transform.rotation.eulerAngles;
        cameraRotation = playerRotation;

        moveSpeed = walkSpeed;
        move = Walk;
    }

    
    void FixedUpdate()
    { 
        // Rotate
        Vector2 rotationVector = playerControls.PlayerMovement.Rotate.ReadValue<Vector2>();
        playerRotation.y += rotationVector.x * sensitivity;
        transform.localEulerAngles = playerRotation;

        cameraRotation.x -= rotationVector.y * sensitivity;
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -90f, 90f);
        cam.localEulerAngles = cameraRotation;

        // Move
        Vector3 moveVector = playerControls.PlayerMovement.Move.ReadValue<Vector3>();
        move(moveVector);
    }


    private void Walk(Vector3 moveVector)
    {
        moveVector = rb.transform.TransformDirection(moveVector);
        moveVector.y = 0;
        rb.MovePosition(rb.position + moveVector * moveSpeed * Time.fixedDeltaTime);
    }


    private void Fly(Vector3 moveVector)
    {
        moveVector = rb.transform.TransformDirection(moveVector);
        rb.MovePosition(rb.position + moveVector * moveSpeed * Time.fixedDeltaTime);
    }


    public void ToggleFlight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isFlying)
            {
                isFlying = true;
                isWalking = false;
                rb.useGravity = false;
                move = Fly;

                if (isSwimming)
                    moveSpeed = walkSpeed + (moveSpeed - swimSpeed);
            }
            else
            {
                isFlying = false;
                rb.useGravity = true;

                if (isSwimming)
                {
                    moveSpeed = swimSpeed + (moveSpeed - walkSpeed);
                }
                else
                {
                    isWalking = true;
                    move = Walk;
                }
            }
        }
    }


    public void Run(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (moveSpeed == walkSpeed || moveSpeed == swimSpeed)
                moveSpeed += speedBoost;
            else
                moveSpeed -= speedBoost;
        }
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && rb.velocity.y == 0 && !isFlying && !isSwimming)
        {
            rb.velocity = new Vector3(0f, jumpForce, 0f);
        }
    }




    // enter and exit are running for each block

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            waterColliderCount++;

            if (!isSwimming)
            {
                print("Player has entered the water!");
                isSwimming = true;
                isWalking = false;
                move = Fly;
                rb.drag = 10f;

                if (!isFlying)
                    moveSpeed = swimSpeed + (moveSpeed - walkSpeed);
            }
        }
    }

    // enter edited, need to edit exit and add collider count var up above

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            waterColliderCount--;

            if (waterColliderCount == 0)
            {
                print("Player has left the water!");
                isSwimming = false;
                rb.drag = 0f;

                if (!isFlying)
                {
                    moveSpeed = walkSpeed + (moveSpeed - swimSpeed);
                    isWalking = true;
                    move = Walk;
                }
            }
        }
    }
}
