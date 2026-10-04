using System.Collections;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]

[RequireComponent(typeof(AudioSource))]
public abstract class Ball : MonoBehaviour
{
    [SerializeField] private LayerMask _collisionLayers;

    private AudioSource _audioSource;
    [SerializeField] private AudioClip _bounceClip;

    public Vector3 Velocity {get; protected set;}

    private MeshRenderer _renderer;
    private bool _waitingToReset = false;
    private bool _hitNoisePlayed = false;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _renderer = GetComponent<MeshRenderer>();
        BallAwake();
    }

    protected virtual void BallAwake(){}

    // Start is called before the first frame update
    void Start()
    {
        ResetBall(true, true);
        BallStart();
    }

    protected virtual void BallStart(){}

    void Update(){
        BallPreUpdate();
        _hitNoisePlayed = false;
        BallPostUpdate();
    }

    protected virtual void BallPreUpdate(){}
    protected virtual void BallPostUpdate(){}

    public abstract void ResetBall(bool playerLost, bool skipWait = false);

    protected virtual void PreFixedUpdate(){}
    protected virtual void PostFixedUpdate(){}

    void FixedUpdate(){
        PreFixedUpdate();

        if (_waitingToReset) { return; }

        // Handle collisions with a raycast check
        float collisionCheckDistance = Velocity.magnitude * Time.fixedDeltaTime * 1.5f;
        var distanceChecked = 0.0f;

        while (distanceChecked < collisionCheckDistance){
            var rayDistance = collisionCheckDistance - distanceChecked;
            var rayDirection = Velocity.normalized;
            if (Physics.Raycast(transform.position, rayDirection, out var hit, rayDistance, _collisionLayers, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.isTrigger){
                    TriggerHit(hit);
                } else {
                    ColliderHit(hit);
                }
                distanceChecked += hit.distance;
            } else
            {    
                distanceChecked += rayDistance;
            }
        }
        transform.position += Velocity * Time.fixedDeltaTime;

        PostFixedUpdate();
    }

    protected virtual void ColliderHit(RaycastHit hit){}
    protected virtual void TriggerHit(RaycastHit hit){}

    public void OnTimeScaleUpdated(float timeScale) {
        _audioSource.enabled = timeScale < 2.0f;
    }

    protected void SetVisualEnabledState(bool state)
    {
        _renderer.enabled = state;
    }

    private bool _cancelReset = false;
    protected void CancelReset()
    {
        _cancelReset = true;
    }

    // Coroutine that disables the GameObject, waits for a period, and then re-enables it
    protected IEnumerator ResetWait(float seconds)
    {
        // Check if the reset has been canceled
        if (_cancelReset)
        {
            _cancelReset = false;
            yield break;
        }
        
        // Disable the GameObject visuals and set the wait flag
        SetVisualEnabledState(false);
        _waitingToReset = true;

        // Wait for the specified duration
        yield return new WaitForSeconds(seconds);

        // Re-enable
        SetVisualEnabledState(true);
        _waitingToReset = false;
    }

    // Play a noise if the game isn't over and we haven't already done so since the last update
    protected void PlayBounceNoise(){
        if (_audioSource != null && _audioSource.enabled && !_hitNoisePlayed)
        {
            _audioSource.PlayOneShot(_bounceClip);
            _hitNoisePlayed = true;
        }
    }
}
