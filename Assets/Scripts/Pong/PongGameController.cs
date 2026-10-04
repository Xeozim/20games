using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PongGameController : MonoBehaviour
{
    // Scores
    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }

    public UnityEvent<int> playerScoreUpdated;
    public UnityEvent<int> opponentScoredUpdated;

    // Event which alerts other things to the state of the game.
    // True means the game is over / waiting for a new one to start.
    public UnityEvent<bool> gameOverStateUpdated;
    private bool _gameOver = false;

    // Controls for restarting etc.
    private InputActions _controls;
    [SerializeField] private PongSettings _settings;
    
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _goalScoredClip;

    private void Awake()
    {
        _controls = new InputActions();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Enable input actions
        _controls.Menu.Enable();

        // Subscribe to the input actions
        _controls.Menu.Restart.performed += RestartGame;
    }

    // Called by the UI to restart the game
    public void RestartGame(InputAction.CallbackContext context){
        // Ignore if the game isn't over
        if (!_gameOver) { return; }
        
        PlayerScore = OpponentScore = 0;
        playerScoreUpdated.Invoke(PlayerScore);
        opponentScoredUpdated.Invoke(OpponentScore);

        _gameOver = false;
        gameOverStateUpdated.Invoke(_gameOver);
    }

    // Called when any goal is scored to check for end game
    public void CheckEndgame()
    {
        // If either player has 11+ points, change the game over state to true
        // and send an event for other gameobjects to respond to
        if (PlayerScore >= _settings.winningScore || OpponentScore >= _settings.winningScore){
            _gameOver = true;
            gameOverStateUpdated.Invoke(_gameOver);
        }
    }

    void ScoreNoise(){
        if (_audioSource.enabled) { _audioSource.PlayOneShot(_goalScoredClip); }
    }

    // Called by the ball when entering the player goal
    public void PlayerScored()
    {
        PlayerScore++;
        playerScoreUpdated.Invoke(PlayerScore);
        ScoreNoise();
        // Debug.Log($"Player Score: {PlayerScore}");
        CheckEndgame();
    }
    
    // Called by the ball when entering the opponent goal
    public void OpponentScored()
    {
        OpponentScore++;
        opponentScoredUpdated.Invoke(OpponentScore);
        ScoreNoise();
        // Debug.Log($"Opponent Score: {OpponentScore}");
        CheckEndgame();
    }

    public void OnTimeScaleUpdated(float timeScale) {
        _audioSource.enabled = timeScale < 2.0f;
    }
}
