// FirstPersonController.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed    = 5f;
    public float sprintSpeed  = 8f;
    public float jumpHeight   = 1.5f;
    public float gravity      = -9.81f;

    [Header("Mouse Look")]
    public Transform cameraPivot;      // assign your Camera here
    public float mouseSenseX  = 2f;
    public float mouseSenseY  = 2f;
    public float pitchMin     = -75f;
    public float pitchMax     = +75f;

    private CharacterController cc;
    private Vector3            velocity;
    private float              pitch = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cameraPivot == null)
            Debug.LogError("FirstPersonController: assign cameraPivot!");
  }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSenseX;
        float my = Input.GetAxis("Mouse Y") * mouseSenseY;

        // yaw rotation on player
        transform.Rotate(Vector3.up * mx);

        // pitch on camera
        pitch = Mathf.Clamp(pitch - my, pitchMin, pitchMax);
        cameraPivot.localEulerAngles = Vector3.right * pitch;
    }

    void HandleMovement()
    {
        bool isGrounded = cc.isGrounded;

        // reset downward velocity when grounded
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // input
        Vector3 input = new Vector3(
            Input.GetAxis("Horizontal"), 
            0f, 
            Input.GetAxis("Vertical")
        );
        input = Vector3.ClampMagnitude(input, 1f);

        // choose speed
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = sprint ? sprintSpeed : moveSpeed;

        // move in local space
        Vector3 move = transform.TransformDirection(input) * speed;
        cc.Move(move * Time.deltaTime);

        // jump
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // apply gravity
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}
