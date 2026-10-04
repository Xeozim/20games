using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FlappyGameController : MonoBehaviour
{
    // Score
    public int Score { get; private set; }

    // Player transform
    [SerializeField] private Transform _playerTransform;

    // List of references to the pipe gameobjects in the scene, these are
    // created by this controller at the start of the game and reused throughout
    // the game. We store the references in a queue so that we can easily get
    // the next pipe to respawn by looking at the front of the queue and then
    // moving it to the back of the queue.
    private Queue<FlappyPipe> _pipeQueue = new();
    [SerializeField] private FlappyPipe _pipePrefab;

    // Reference to the last pipe in the queue
    private FlappyPipe _lastPipe;

    // Event to alert the UI when the score changes
    public UnityEvent<int> scoreUpdated;

    // Event which alerts other things to the state of the game.
    // True means the game is over / waiting for a new one to start.
    public UnityEvent<bool> gameOverStateUpdated;
    private bool _gameOver = false;

    // Controls for restarting etc.
    private InputActions _controls;
    [SerializeField] private FlappySettings _settings;

    private void Awake()
    {
        _controls = new InputActions();
    }

    private void OnEnable()
    {
        // Enable input actions
        _controls.Menu.Enable();

        // Subscribe to the input actions
        _controls.Menu.Restart.performed += RestartGame;
    }

    // Update is called once per frame
    void Update()
    {
        // If the game is over, wait for the player to restart the game
        if (_gameOver) { return; }

        // If the X distance between the player and the LAST pipe in the queue is
        // less than the distance between the player and the edge of the screen,
        // the we need to either create a new pipe or move an existing one.
        // If the first pipe in the queue is offscreen, then we can move it to
        // the right side of the screen and reuse it. If not, then we need to
        // create a new pipe and add it to the queue. In either case the X
        // position of the new pipe is determined by the last pipe in the queue
        // and the pipe spacing setting.
        // This logic replaces the need for creating all the pipes at the start
        // of the game, instead they are created as needed and reused when they go offscreen.
        var playerToLastPipeDistance = _lastPipe != null ? _lastPipe.transform.position.x - _playerTransform.position.x : float.MinValue;
        var screenRightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        var screenLeftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        var playerToScreenEdgeDistance = screenRightEdge - _playerTransform.position.x;

        Debug.Log($"Last pipe position: {(_lastPipe != null ? _lastPipe.transform.position.x : float.NaN)}");
        Debug.Log($"Player to last pipe distance: {playerToLastPipeDistance}, Player to screen edge distance: {playerToScreenEdgeDistance}");

        if (playerToLastPipeDistance < playerToScreenEdgeDistance)
        {
            // If the first pipe in the queue is offscreen, then we can move it to
            // the right side of the screen and reuse it. If not, then we need to
            // create a new pipe and add it to the queue.

            // Properties of the "new" pipe are the same regardless of whether we
            // are reusing an existing pipe or actually creating a new one.
            var newPipeXPosition = _lastPipe != null ? _lastPipe.transform.position.x + _settings.pipeSpacing : screenRightEdge + _settings.pipeSpacing;
            var newPipeYPosition = CalculateNextPipeYPosition();

            // After moving the pipe we'll need to trigger a reposition of the
            // top and bottom pipe meshes, so calculate the gap between them
            // in advance.
            var gapSize = Random.Range(_settings.pipeGapMinimum, _settings.pipeGapMaximum);

            var firstPipe = _pipeQueue.Count > 0 ? _pipeQueue.Peek() : null;
            if (firstPipe != null && firstPipe.transform.position.x < screenLeftEdge - _settings.pipeSpacing)
            {
                // Move the first pipe in the queue to the right side of the
                // screen, set its position, and put it to the back of the queue.
                firstPipe.transform.position = new Vector3(newPipeXPosition, newPipeYPosition, 0f);
                _pipeQueue.Enqueue(_pipeQueue.Dequeue());
                _lastPipe = firstPipe;

                Debug.Log($"Reusing pipe {firstPipe.gameObject.name} and moving it to X: {newPipeXPosition}, Y: {newPipeYPosition} with gap size: {gapSize}");
            }
            else
            {
                // Create a new pipe and add it to the queue.
                var pipe = Instantiate(_pipePrefab, new Vector3(newPipeXPosition, newPipeYPosition, 0f), Quaternion.identity);
                _pipeQueue.Enqueue(pipe);
                _lastPipe = pipe;

                Debug.Log($"Creating new pipe {pipe.gameObject.name} at X: {newPipeXPosition}, Y: {newPipeYPosition} with gap size: {gapSize}");
            }

            _lastPipe.SetPipePositions(gapSize);
        }
    }

    // Called by the UI to restart the game
    public void RestartGame(InputAction.CallbackContext context){
        // Ignore if the game isn't over
        if (!_gameOver) { return; }
        
        Score = 0;
        scoreUpdated.Invoke(Score);

        _gameOver = false;
        gameOverStateUpdated.Invoke(_gameOver);

        // Remove all pipes from the scene and clear the queue
        foreach (var pipe in _pipeQueue)
        {
            Destroy(pipe.gameObject);
        }
        _pipeQueue.Clear();
    }

    // Called by the player when passing through a pipe to increment the score
    public void PlayerScored()
    {
        Score++;
        scoreUpdated.Invoke(Score);
        // Debug.Log($"Player Score: {Score}");
    }
    
    // Calculates the Y position of the next pipe to be spawned based on the
    // Y position of the last pipe and the maximum change in Y position allowed
    // This function does not change the value of _lastPipeYPosition (or
    // anything else for that matter).
    private float CalculateNextPipeYPosition()
    {
        // Calculate the Y position from the maximum range and then clamp, this
        // will encourage more extreme changes in Y position
        var nextPipeYPosition = Random.Range(_settings.pipeYMinimum, _settings.pipeYMaximum);
        var lastPipeYPosition = _lastPipe != null ? _lastPipe.transform.position.y : 0f;
        nextPipeYPosition = Mathf.Clamp(nextPipeYPosition, lastPipeYPosition - _settings.pipeYChangeMaximum, lastPipeYPosition + _settings.pipeYChangeMaximum);
        return nextPipeYPosition;
    }

    // Create the pipes at the start of the game and add them to the queue for reuse
    private void CreatePipes()
    {
        // var spawnToDespawnDistance = _despawnPoint.position.x - _spawnPoint.position.x;
        // var numberOfPipes = Mathf.CeilToInt(spawnToDespawnDistance / _settings.pipeSpacing);

        // for (int i = 0; i < numberOfPipes; i++)
        // {
        //     // Calculate the initial X position of the pipe based on the spawn point and the spacing
        //     var pipeXPosition = _spawnPoint.position.x + (i * _settings.pipeSpacing);
        //     // Calculate the Y position of the pipe based on the last pipe's Y position and the maximum change in Y position allowed
        //     var pipeYPosition = CalculateNextPipeYPosition();
        //     // Instantiate the pipe prefab and set its position
        //     var pipe = Instantiate(_pipePrefab, new Vector3(pipeXPosition, pipeYPosition, 0f), Quaternion.identity);
        //     pipe.PipeWentOffscreen.AddListener(OnPipeWentOffscreen);
        //     _pipeQueue.Enqueue(pipe);
        // }
    }

    // Called when a pipe goes offscreen to respawn it at the right side of the screen
    // To ensure that the pipe spacing is consistent
    private void OnPipeWentOffscreen(FlappyPipe pipe)
    {
        
    }
}
