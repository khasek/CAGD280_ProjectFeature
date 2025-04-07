/*******************************************************************************
 * Author: Kendal Hasek
 * Date: 05/09/2024
 * Description: Player.cs manages player actions
*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Rigidbody rb;
    private PlayerControls playerControls;
    public static Player Instance;

    // Movement variables ------------------------------------------------------
    private float moveSpeed;
    private float walkSpeed = 4f;
    private float swimSpeed = 2f;
    private float speedBoost = 4f;
    private float jumpForce = 6.5f;

    private bool isSwimming = false;
    private bool isFlying = false;

    private delegate void MoveDelegate(Vector3 moveVector);
    private MoveDelegate move;

    // Rotation variables ------------------------------------------------------
    private Vector3 playerRotation;
    private Vector3 cameraRotation;
    [SerializeField] private Transform cam;
    private float sensitivity = 3f;


    // Unity functions ---------------------------------------------------------

    private void Awake()
    {
        // This project will only have one player, so I've made it a singleton
        // for ease of reference

        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this.gameObject);
    }

    // -------------------------------------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        playerControls = new PlayerControls();
        playerControls.Enable();

        playerRotation = transform.rotation.eulerAngles;
        cameraRotation = playerRotation;

        moveSpeed = walkSpeed;
        move = Walk;
    }

    // -------------------------------------------------------------------------

    // Rotation doesn't use physics
    private void Update()
    {
        Vector2 rotationVector = playerControls.PlayerMovement.Rotate.ReadValue<Vector2>();
        playerRotation.y += rotationVector.x * sensitivity;
        transform.localEulerAngles = playerRotation;

        cameraRotation.x -= rotationVector.y * sensitivity;
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -90f, 90f);
        cam.localEulerAngles = cameraRotation;
    }

    // -------------------------------------------------------------------------

    // Movement uses physics
    void FixedUpdate()
    { 
        Vector3 moveVector = playerControls.PlayerMovement.Move.ReadValue<Vector3>();
        move(moveVector);
    }

    // -------------------------------------------------------------------------

    // If the player is standing in water, their movement speed and gravity
    // should both be reduced. They should also be able to move up and down.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            isSwimming = true;
            move = Fly;
            rb.drag = 10f;

            if (!isFlying)
                moveSpeed = swimSpeed + (moveSpeed - walkSpeed);
        }
    }

    // -------------------------------------------------------------------------

    // Return player movement to normal upon exiting the water
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            isSwimming = false;
            rb.drag = 0f;

            if (!isFlying)
            {
                moveSpeed = walkSpeed + (moveSpeed - swimSpeed);
                move = Walk;
            }
        }
    }


    // Custom functions --------------------------------------------------------

    /// <summary>
    /// Allows movement only along the horizontal plane
    /// </summary>
    private void Walk(Vector3 moveVector)
    {
        moveVector = rb.transform.TransformDirection(moveVector);
        moveVector.y = 0;
        rb.MovePosition(rb.position + moveVector * moveSpeed * Time.fixedDeltaTime);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Allows movement in all three dimensions
    /// </summary>
    private void Fly(Vector3 moveVector)
    {
        moveVector = rb.transform.TransformDirection(moveVector);
        rb.MovePosition(rb.position + moveVector * moveSpeed * Time.fixedDeltaTime);
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Toggle flight mode with TAB. Flight mode takes precedence over both
    /// walking and swimming.
    /// </summary>
    public void ToggleFlight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isFlying)
            {
                isFlying = true;
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
                    move = Walk;
                }
            }
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Press the SPACE bar to jump; cannot jump while already in the air
    /// </summary>
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && !isFlying)
        {
            if (Mathf.Abs(rb.velocity.y) < 0.01)
                rb.velocity = new Vector3(0f, jumpForce, 0f);
        }
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Press the SHIFT key to speed up player movement
    /// </summary>
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
}
