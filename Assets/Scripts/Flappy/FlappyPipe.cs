using UnityEngine;
using UnityEngine.Events;

public class FlappyPipe : MonoBehaviour
{
    [SerializeField] private FlappySettings _settings;

    [SerializeField] private Transform _topPipe;
    [SerializeField] private Transform _bottomPipe;
    [SerializeField] private BoxCollider _pipeScoringTrigger;

    // Height of the pipe mesh, used to calculate the Y position of the top and bottom pipes based on the gap size.
    private static readonly float PIPE_HEIGHT = 8f;

    // Game over state, set by the game controller when the player collides with a pipe
    private bool _gameOver = false;

    // On Enable, check that all the required components are set, and if not, log an error to the console
    private void OnEnable()
    {
        if (_topPipe == null) { Debug.LogError("Top pipe transform is not set!"); }
        if (_bottomPipe == null) { Debug.LogError("Bottom pipe transform is not set!"); }
        if (_pipeScoringTrigger == null) { Debug.LogError("Pipe scoring trigger is not set!"); }
    }

    // Update is called once per frame
    void Update()
    {
        // If the game is over, wait for the player to restart the game
        if (_gameOver) { return; }

        // Move the pipe to the left at a constant speed
        transform.position += _settings.pipeSpeed * Time.deltaTime * Vector3.left;
    }

    // Set the position of the top and bottom pipes based on the gap size and 
    // the current Y position of the pipe
    public void SetPipePositions(float gapSize)
    {
        _topPipe.localPosition = new Vector3(0, PIPE_HEIGHT / 2 + gapSize / 2, 0);
        _bottomPipe.localPosition = new Vector3(0, -PIPE_HEIGHT / 2 - gapSize / 2, 0);
        // Resize the scoring trigger to ensure it covers the gap
        _pipeScoringTrigger.size = new Vector3(_pipeScoringTrigger.size.x, gapSize + 1, _pipeScoringTrigger.size.z);
    }

    // Called by the game controller when the game is over to stop the pipe from moving
    public void SetGameOverState(bool gameOver)
    {
        _gameOver = gameOver;
    }
}
