using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
[RequireComponent(typeof(Rigidbody))]

[RequireComponent(typeof(Collider))]
public class FlappyPlayer : MonoBehaviour
{
    [SerializeField] private FlappySettings _settings;
    private InputActions _inputs;
    private Rigidbody _rigidbody;

    // Event to alert the game controller when the player has collided with a pipe
    public UnityEvent playerCollidedWithPipe;
    public UnityEvent playerPassedThroughPipe;

    // Two mesh renderers for the player, one for falling and one for jumping.
    // Transition from one to the other based on the player's vertical velocity.
    [SerializeField] private MeshRenderer _fallingMesh;
    [SerializeField] private MeshRenderer _jumpingMesh;

    // Starting position stored for resetting
    private Vector3 _startingPosition;

    // Extent of the player collider in the Y axis, used to determine if the
    // player has touched the top or bottom of the screen
    public Bounds ColliderExtents { get; private set; } = new Bounds();

    private void Awake()
    {
        _inputs = new InputActions();
        _rigidbody = GetComponent<Rigidbody>();
        _startingPosition = transform.position;
    }

    private void OnEnable()
    {
        // Enable input actions
        _inputs.Flappy.Enable();

        // Subscribe to the input actions
        _inputs.Flappy.Flap.performed += OnJumpInputPerformed;

        // Calculate the player collider extents from all the colliders attached
        // to the player gameobject.
        var colliders = GetComponents<Collider>();
        var extents = new Bounds();
        var hasBounds = false;

        foreach (var collider in colliders)
        {
            // Debug.Log($"Player collider: {collider} with bounds: {collider.bounds}");
            if (!collider.enabled || !collider.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!hasBounds)
            {
                extents = collider.bounds;
                hasBounds = true;
            }
            else
            {
                extents.Encapsulate(collider.bounds);
            }
        }

        ColliderExtents = extents;
        // Debug.Log($"Player collider extents: {ColliderExtents}");
    }

    private void OnDisable()
    {
        // Disable input actions
        _inputs.Flappy.Disable();
    }

    private void Update()
    {
        // If the player is falling, show the falling mesh, otherwise show the jumping mesh
        if (_rigidbody.linearVelocity.y < 0)
        {
            _fallingMesh.enabled = true;
            _jumpingMesh.enabled = false;
        }
        else
        {
            _fallingMesh.enabled = false;
            _jumpingMesh.enabled = true;
        }
    }

    private void OnJumpInputPerformed(InputAction.CallbackContext context)
    {
        // Ignore input if the rigidbody is kinematic (i.e. the game is over)
        if (_rigidbody.isKinematic) { return; }

        // Reset the linear velocity and apply an impulse to the rigidbody to make the player jump
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.AddForce(Vector3.up * _settings.jumpImpulse, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If the player collides with a pipe, set the game over state to true
        if (collision.gameObject.CompareTag("BarrierVertical"))
        {
            // Debug.Log("Player collided with a pipe!");
            playerCollidedWithPipe.Invoke();
        }
    }

    // Because the player has multiple colliders they can trigger multiple
    // OnTriggerEnter events when passing through a pipe, we need to keep track
    // of how many times any player collider enters the pipe scoring trigger,
    // and when the corresponding exits occur. When the last player collider
    // exits the trigger, we can then invoke the event to increment the score.
    private int _GoalTriggerCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        // If the player passes through a pipe, increment the score
        if (other.gameObject.CompareTag("PlayerGoal"))
        {
            // Debug.Log("Player entered the trigger of a pipe!");
            _GoalTriggerCount++;
            // Debug.Log($"Goal Trigger Count: {_GoalTriggerCount}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the player exits the trigger of a pipe, increment the score
        if (other.gameObject.CompareTag("PlayerGoal"))
        {
            // Debug.Log("Player exited the trigger of a pipe!");
            if (--_GoalTriggerCount <= 0)
            {
                _GoalTriggerCount = 0;
                playerPassedThroughPipe.Invoke();
            }
            // Debug.Log($"Goal Trigger Count: {_GoalTriggerCount}");
        }
    }

    private void ResetRigidbody()
    {
        // Reset the player position and velocity and enable physics
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.rotation = Quaternion.identity;
    }

    // Called by the game controller when the game is over to reset things
    public void OnGameOverUpdated(bool isGameOver)
    {
        if (isGameOver)
        {
            _GoalTriggerCount = 0;
            transform.position = _startingPosition;
            ResetRigidbody();
        }
    }
}