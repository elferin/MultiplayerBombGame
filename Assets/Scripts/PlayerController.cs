using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] InputActionReference moveAction;
    [SerializeField] float maxSpeed = 6f;
    [SerializeField] float acceleration = 40f;

    Rigidbody body;
    Vector2 moveInput;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        moveAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
    }

    void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Top-down view: screen up maps to world +Z.
        Vector3 target = new Vector3(moveInput.x, 0f, moveInput.y) * maxSpeed;
        Vector3 velocity = body.linearVelocity;
        Vector3 planar = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 change = Vector3.MoveTowards(planar, target, acceleration * Time.fixedDeltaTime) - planar;
        body.AddForce(change, ForceMode.VelocityChange);
    }
}
