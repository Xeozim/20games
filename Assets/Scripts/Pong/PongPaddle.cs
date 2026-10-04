using System.Collections.Generic;
using UnityEngine;

// Base class for paddles (player and AI)
public class PongPaddle : MonoBehaviour
{
    [SerializeField] protected Transform _paddle;
    [SerializeField] protected List<MeshRenderer> _renderers;
    [SerializeField] protected List<Collider> _colliders;
    [SerializeField] protected PongSettings _settings;

    protected PongBall _ball;
    protected Vector3 _targetPosition;

    // Y-axis limits for the paddle movement, calculated from the settings
    // for paddle height and the game area's vertical limits.
    protected Vector2 _yLimits;

    void Start()
    {
        _ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<PongBall>();
        _targetPosition = _paddle.transform.position;
    }

    protected void SettingsRefresh(){
        // NB pong paddles are rotated 90 degrees so they can use the maths from BallBehaviours
        _paddle.transform.localScale = new Vector3(_settings.paddleHeight,_settings.paddleWidth,1);
        _yLimits = new Vector2(_settings.yMinimum + _settings.paddleHeight * 0.5f, _settings.yMaximum - _settings.paddleHeight * 0.5f);
    }

    public void OnGameOverStateUpdated(bool isGameOver)
    {
        // Debug.Log($"OnGameOverStateUpdated ({isGameOver}) called on {transform.name}");
        foreach (var renderer in _renderers)
        {
            renderer.enabled = !isGameOver;
        }
        foreach (var collider in _colliders)
        {
            collider.enabled = !isGameOver;
        }

        if (!isGameOver) {
            // Game was restarted, reset to the centre of the screen
            _paddle.transform.position = new Vector3(_paddle.transform.position.x,0,_paddle.transform.position.z);
        }
    }

    // FixedUpdate is called at the same rate as the physics system update
    void FixedUpdate()
    {
        // Set velocity to achieve the target position, very simple proportional control by clamping
        // Multiplying position offset by 10 means we use full speed unless the target position is
        // within paddleSpeed / 10 of the current. In effect we always move at full speed.
        var velocity = Mathf.Clamp((_targetPosition.y - _paddle.transform.position.y) * 10, -_settings.paddleSpeed, _settings.paddleSpeed);

        // Update position, clamping to game limits
        var newYPosition = Mathf.Clamp(_paddle.transform.position.y + velocity * Time.fixedDeltaTime, _yLimits.x, _yLimits.y);
        _paddle.transform.position = new Vector3(_paddle.transform.position.x,newYPosition,_paddle.transform.position.z);
    }
}
