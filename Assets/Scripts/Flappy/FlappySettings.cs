using UnityEngine;

[CreateAssetMenu(fileName = "NewFlappySettings", menuName = "ScriptableObjects/FlappySettings")]
public class FlappySettings : ScriptableObject
{
    public float pipeSpeed;
    [Range(0.1f, 5f)] public float pipeSpacing; // How far apart the pipes are in the X direction
    public float pipeGapMinimum;
    public float pipeGapMaximum;
    public float pipeYMinimum;
    public float pipeYMaximum;
    public float pipeYChangeMaximum; // Maximum change in Y position between consecutive pipes
    public float jumpImpulse;
    public float jumpWait;
}