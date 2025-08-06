using System;
        using System.Collections;
        using System.Collections.Generic;
        using UnityEngine;
        using UnityEngine.XR;
        
        [RequireComponent(typeof(CharacterController))]
        public class PlayerMovement : MonoBehaviour
        {
            public float speed = 5f;
            public Transform cameraView;
            public float maxAngle = 90f;
            public float minAngle = -90f;
            public float mouseSensitivity = 100f;
        
            private CharacterController characterController;
            private Vector3 moveDirection;
            private float gravity = -9.81f;
            private float verticalVelocity = 0f;
            private float xRotation = 0f;
        
            private void Start()
            {
                characterController = GetComponent<CharacterController>();
                Cursor.lockState = CursorLockMode.Locked;
            }
        
            private void Update()
            {
                HandleMovement();
                HandleCameraRotation();
            }
        
            private void HandleMovement()
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");
        
                Vector3 forward = cameraView.forward;
                Vector3 right = cameraView.right;
        
                forward.y = 0;
                right.y = 0;
        
                forward.Normalize();
                right.Normalize();
        
                Vector3 move = (forward * vertical + right * horizontal).normalized * speed;
        
                if (characterController.isGrounded)
                {
                    verticalVelocity = -1f;
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
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
                // Rotate the player body horizontally
                transform.Rotate(Vector3.up * mouseX);
        
                // Rotate the camera vertically
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, minAngle, maxAngle);
                cameraView.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            }
        }