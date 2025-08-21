using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("View")]
    public Transform cameraView;
    public float maxAngle = 90f;
    public float minAngle = -90f;

    [Tooltip("Mouse sensitivity (no deltaTime scaling)")]
    public float mouseSensitivity = 100f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private const float gravity = -9.81f;
    private float verticalVelocity = 0f;
    private float xRotation = 0f;
    
    private Tween lookAtTween;
    
    public bool IsFrozen { get; set; }   // <— add this
    void Awake()
    {
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.minMoveDistance = 0f; // prevent high-FPS stutter
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
         HandleCameraRotation();
    }

    private void Update()
    {
        if (!IsFrozen)
            HandleMovement();
       
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraView.forward;
        Vector3 right = cameraView.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * vertical + right * horizontal).normalized * speed;

        if (characterController.isGrounded)
        {
            verticalVelocity = -1f; // keep grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // Rotate the player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minAngle, maxAngle);
        cameraView.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
