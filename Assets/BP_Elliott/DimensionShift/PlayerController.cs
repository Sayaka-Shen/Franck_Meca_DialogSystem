using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 2.2f;
    public float gravity = -25f;
    public float groundedStickForce = -2f;

    [Header("Input")]
    private string horizontalAxis = "Horizontal";
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Optional: camera-relative left/right")]
    [SerializeField] private bool cameraRelative = true;
    [SerializeField] private Transform movementBasis;
    private bool invert = false;


    private CharacterController cc;
    private float verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float input = Input.GetAxisRaw(horizontalAxis);
        if (invert) input = -input;

        Vector3 rightFlat = Vector3.ProjectOnPlane(movementBasis.right, Vector3.up).normalized;
        Vector3 horizontalMove = rightFlat * (input * moveSpeed);

        bool grounded = cc.isGrounded;
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = groundedStickForce;

        if (grounded && Input.GetKeyDown(jumpKey))
            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalMove;
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);
    }

}
