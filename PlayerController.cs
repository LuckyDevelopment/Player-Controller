using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // REQUIRES AN INPUT ACTION MAP SETUP! SCROLL DOWN TO FIND THE VARIABLES THAT USE IT!  -----------------------------------------------------------------------------

    // This script controls the player and camera.
    
    // PUBLICS
    [Header("Object Settings")]
    [SerializeField] private Transform Orientation;
    [SerializeField] Transform CameraPosition;
    [SerializeField] Transform CameraHolder;
    [SerializeField] GameObject Camera;

    [Header("Player Settings")]
    [SerializeField] private float WalkSpeed = 5f;
    [SerializeField] private float SprintSpeed = 7f;
    
    [Header("Jumping Settings")]
    [SerializeField] private float JumpPower = 3f;
    [Range(0, 10)] [SerializeField] private float TimeBetweenJump = 0.3f;
    [Range(0, 1)] [SerializeField] private float GroundDistance = 0.25f;
    [SerializeField] private LayerMask GroundMask;

    [Header("Drag Settings (Not Recommended)")]
    [SerializeField] private float GroundDrag = 0.5f;
    [SerializeField] private float AirDrag = 0.2f;

    [Header("Camera Settings")]
    [Tooltip("The max Y axis that the camera can go, recommend 90 deg.")] [Range(0, 90)] [SerializeField] private float MaxLookAngle = 90f;
    public float CameraSensitivity = 300f;
    [SerializeField] private bool ShowCursor = false;
    
    
    [Header("Other Settings")]
    [SerializeField] private float Gravity = -20f;
    [Range(0, 90)] [SerializeField] private float MaxSlopeAngle = 45;
    [Range(0, 3)] [SerializeField] private float MaxStepHeight = 0.3f;
    [SerializeField] private float Radius = 0.5f;
    [SerializeField] private float Height = 2f;
    [SerializeField] private Vector3 Center = Vector3.zero;

    public bool isGrounded {get; private set;}

    // PRIVATES
    private CharacterController controller;
    private Vector3 velocity;
    private bool canJump = true;
    private Vector3 moveDirection;
    private float MouseX;
    private float MouseY;
    private float cameraRotX;
    private float cameraRotY;
    private PlayerControls playerControls; // CHANGE PLAYERCONTROLS TO YOUR ACTION MAP NAME.  -----------------------------------------------------------------------------

    // Runs when game starts.
    void Start()
    {
        // Get controller and reset velocity to zero.
        controller = GetComponent<CharacterController>();
        velocity = Vector3.zero;

        // New Controls
        playerControls = new PlayerControls(); // CHANGE TO YOUR ACTION MAP NAME.  -----------------------------------------------------------------------------
        playerControls.Enable();

        // Assign variables to controller.
        controller.slopeLimit = MaxSlopeAngle;
        controller.stepOffset = MaxStepHeight;
        controller.radius = Radius;
        controller.height = Height;
        controller.center = Center;

        // lock the mouse.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = ShowCursor;
    }

    // Runs in a fixed update loop.
    void FixedUpdate()
    {
         if (isGrounded)
         {
            velocity = velocity * ( 1 - Time.deltaTime * GroundDrag);
         } else
         {
            velocity = velocity * ( 1 - Time.deltaTime * AirDrag);
         }
    }
    // Runs per frame.
    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, Height / 2 + GroundDistance);

        // Handle player stuff;
        HandleMovement();
        HandleCamera();

        // Assign velocity.
        controller.Move(moveDirection);


        if (isGrounded)
        {
            velocity.y = -0.1f * Time.deltaTime; 
        }

        if (playerControls.Gameplay.Jump.ReadValue<float>() > 0.1f && isGrounded && canJump) //CHANGE TO YOUR JUMP AREA -----------------------------------------------------------------------------
        {
            canJump = false;
            velocity.y = (float)Math.Sqrt(JumpPower * -2f * Gravity);
            StartCoroutine(nameof(JumpWaitTime), TimeBetweenJump);
        }

        // Add gravity
        velocity.y += Time.deltaTime * Gravity;
        // Assign gravity velocity.
        controller.Move(velocity * Time.deltaTime);       
    }
   
    // This delays jumping.
    IEnumerator JumpWaitTime(float time)
    {
        yield return new WaitForSeconds(time);
        canJump = true;
    }


    // This function handles all the camera stuff.
    void HandleCamera()
    {
        // Set camera holder position to the told camera position.
        CameraHolder.position = CameraPosition.position;

        // Get input and do some calculations on it.
        MouseX = playerControls.Gameplay.Look.ReadValue<Vector2>().x * CameraSensitivity * Time.deltaTime; // CHANGE BOTH TO LOOK VALUES  -----------------------------------------------------------------------------
        MouseY = playerControls.Gameplay.Look.ReadValue<Vector2>().y * CameraSensitivity * Time.deltaTime;
        
        cameraRotX += MouseX;
        cameraRotY -= MouseY;

        cameraRotY = Mathf.Clamp(cameraRotY, -MaxLookAngle, MaxLookAngle);

        // Assign the rot values.

        Camera.transform.rotation = Quaternion.Euler(cameraRotY, cameraRotX, 0);
        Orientation.transform.rotation = Quaternion.Euler(0, cameraRotX, 0);
    }

    // This function handles all the movement.
    void HandleMovement()
    {
        // Get the input, and decide it the player is sprinting.
        float playerSpeed = playerControls.Gameplay.Sprint.ReadValue<float>() > 0.1f ? SprintSpeed : WalkSpeed; // CHANGE TO SPRINT VALUES -----------------------------------------------------------------------------

        float horizontal = playerControls.Gameplay.Movement.ReadValue<Vector2>().x * playerSpeed * Time.deltaTime; // CHANGE TO MOVEMENT VALUES -----------------------------------------------------------------------------
        float vertical = playerControls.Gameplay.Movement.ReadValue<Vector2>().y * playerSpeed * Time.deltaTime;


        // Get direction
        moveDirection = Orientation.forward * vertical + Orientation.right * horizontal;
    }


}
