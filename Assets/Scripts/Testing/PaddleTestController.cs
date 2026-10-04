using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleTestController : MonoBehaviour
{
    [SerializeField] protected Transform _paddle;
    [SerializeField] private float _translationSensitivity;
    [SerializeField] private float _rotationSensitivity;

    private InputActions _inputs;
    private float _leftRightInput;
    private float _upDownInput;
    private float _rotateInput;

    private void Awake(){
        _inputs = new InputActions();
    }

    private void OnEnable()
    {
        // Enable input actions
        _inputs.PaddleTest.Enable();

        // Subscribe to the input actions
        _inputs.PaddleTest.LeftRight.performed += OnLeftRightInputPerformed;
        _inputs.PaddleTest.LeftRight.canceled += OnLeftRightInputCancelled;
        _inputs.PaddleTest.UpDown.performed += OnUpDownInputPerformed;
        _inputs.PaddleTest.UpDown.canceled += OnUpDownInputCancelled;
        _inputs.PaddleTest.Rotate.performed += OnRotateInputPerformed;
        _inputs.PaddleTest.Rotate.canceled += OnRotateInputCancelled;
    }

    private void OnDisable()
    {
        // Disable input actions
        _inputs.PaddleTest.Disable();
    }

    private void OnLeftRightInputPerformed(InputAction.CallbackContext context)
    {
        _leftRightInput = context.ReadValue<float>();
    }
    private void OnLeftRightInputCancelled(InputAction.CallbackContext context)
    {
        _leftRightInput = 0;
    }
    private void OnUpDownInputPerformed(InputAction.CallbackContext context)
    {
        _upDownInput = context.ReadValue<float>();
    }
    private void OnUpDownInputCancelled(InputAction.CallbackContext context)
    {
        _upDownInput = 0;
    }
    private void OnRotateInputPerformed(InputAction.CallbackContext context)
    {
        _rotateInput = context.ReadValue<float>();
    }
    private void OnRotateInputCancelled(InputAction.CallbackContext context)
    {
        _rotateInput = 0;
    }

    void FixedUpdate()
    {
        _paddle.transform.Translate(x: _leftRightInput * _translationSensitivity, y: _upDownInput * _translationSensitivity, z:0, relativeTo: Space.World);
        _paddle.transform.Rotate(Vector3.forward, _rotateInput * _rotationSensitivity);
    }
}
