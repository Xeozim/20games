using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class FlappyPlayer : MonoBehaviour
{
    [SerializeField] private FlappySettings _settings;
    private InputActions _inputs;
    private Rigidbody _rigidbody;

    // Event to alert the game controller when the player has collided with a pipe
    public UnityEvent playerCollidedWithPipe;
    public UnityEvent playerPassedThroughPipe;

    private void Awake()
    {
        _inputs = new InputActions();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        // Enable input actions
        _inputs.Flappy.Enable();

        // Subscribe to the input actions
        _inputs.Flappy.Flap.performed += OnJumpInputPerformed;
    }

    private void OnDisable()
    {
        // Disable input actions
        _inputs.Flappy.Disable();
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

    private void OnTriggerEnter(Collider other)
    {
        // If the player passes through a pipe, increment the score
        if (other.gameObject.CompareTag("PlayerGoal"))
        {
            // Debug.Log("Player passed through a pipe!");
            playerPassedThroughPipe.Invoke();
        }
    }
}