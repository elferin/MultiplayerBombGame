using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] InputActionReference moveAction;
    [SerializeField] float maxSpeed = 6f;
    [SerializeField] float acceleration = 40f;

    Rigidbody body;
    Vector2 moveInput;
    bool hasEnabledInput;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    // Ownership is given right after spawning, so it's checked every frame rather than once.
    void Update()
    {
        if (!isOwner)
            return;

        // The action is shared by every player object, so only the owner turns it on.
        if (!hasEnabledInput)
        {
            moveAction.action.Enable();
            hasEnabledInput = true;
        }
        moveInput = moveAction.action.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Other players' objects follow their owner through NetworkRigidbody.
        if (!isOwner)
            return;

        // Top-down view: screen up maps to world +Z.
        Vector3 target = new Vector3(moveInput.x, 0f, moveInput.y) * maxSpeed;
        Vector3 velocity = body.linearVelocity;
        Vector3 planar = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 change = Vector3.MoveTowards(planar, target, acceleration * Time.fixedDeltaTime) - planar;
        body.AddForce(change, ForceMode.VelocityChange);
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
        // isOwner may already be false here, so this remembers whether this object turned input on.
        if (hasEnabledInput)
        {
            moveAction.action.Disable();
            hasEnabledInput = false;
        }
    }
}
