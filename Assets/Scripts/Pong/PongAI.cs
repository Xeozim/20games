using UnityEngine;

public class PongAI : PongPaddle
{
    // For paddle classes, update is used to set the target position. The parent class will move
    // the paddle to reach this position in it's FixedUpdate function.
    private void Update()
    {
        SettingsRefresh();

        // If the ball is moving away from us, move towards the centre of the screen
        var ballToPaddleDirection = _paddle.transform.position - _ball.transform.position;
        ballToPaddleDirection.Normalize();
        var ballMovementDirection = _ball.Velocity;
        ballMovementDirection.Normalize();
        if (Vector3.Dot(ballMovementDirection, ballToPaddleDirection) < 0)
        {
            _targetPosition.y = 0;
        } else {
            // Predict the y impact of the ball and move towards that
            // Time until the ball reaches our x position (assumes we only move in y and that
            // the ball won't lose energy en route)  
            var timeToIntercept = (_paddle.transform.position.x - _ball.transform.position.x) / _ball.Velocity.x;

            _targetPosition.y = _ball.transform.position.y + (_ball.Velocity.y * timeToIntercept);
        }
    }
}
