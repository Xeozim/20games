using UnityEngine;
using UnityEngine.Events;

public class FlappyPipe : MonoBehaviour
{
    [SerializeField] private FlappySettings _settings;

    [SerializeField] private Transform _topPipe;
    [SerializeField] private Transform _bottomPipe;

    // Height of the pipe mesh, used to calculate the Y position of the top and bottom pipes based on the gap size.
    private static readonly float PIPE_HEIGHT = 8f;

    // Event triggered by this pipe when it goes offscreen.
    // Used by the game controller to know when to respawn the pipe at the right side of the screen.
    public UnityEvent<FlappyPipe> PipeWentOffscreen;

    // Update is called once per frame
    void Update()
    {
        // Move the pipe to the left at a constant speed
        transform.position += _settings.pipeSpeed * Time.deltaTime * Vector3.left;
    }

    // Set the position of the top and bottom pipes based on the gap size and 
    // the current Y position of the pipe
    public void SetPipePositions(float gapSize)
    {
        _topPipe.localPosition = new Vector3(0, PIPE_HEIGHT / 2 + gapSize / 2, 0);
        _bottomPipe.localPosition = new Vector3(0, -PIPE_HEIGHT / 2 - gapSize / 2, 0);
    }
}
