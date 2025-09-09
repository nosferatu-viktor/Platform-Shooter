using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    [Header("Animation Settings")]
    [SerializeField] private float _walkAnimationSpeed = 1f;
    [SerializeField] private string _idleAnimationName = "Idle";
    [SerializeField] private string _walkAnimationName = "Walk";
    [SerializeField] private string _shootAnimationName = "Shoot";
    [SerializeField] private Animator _animator;
    private AnimationState _currentAnimationState = AnimationState.Idle;
    private bool _isShooting=false;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    public void UpdateAnimations(AnimationState animationState)
    {
        if (_animator == null)
            return;
        _currentAnimationState = animationState;
        if (!_isShooting)
        {
            switch (_currentAnimationState)
            {
                case AnimationState.Idle:
                    _animator.SetBool("IsWalking", false);
                    _animator.Play(_idleAnimationName);
                    break;
                case AnimationState.Walk:
                    _animator.SetBool("IsWalking", true);
                    _animator.Play(_walkAnimationName);
                    //animasyon hýzýný movement hýzýna ayarladýk
                    //_animator.speed = Mathf.Lerp(0.8f, 1.5f, _currentVelocity.magnitude / _moveSpeed);
                    break;
                case AnimationState.Shoot:
                    _isShooting = true;
                    _animator.SetTrigger("Shoot");
                    Invoke(nameof(StopShooting), 0.5f);
                    break;
            }
        }
    }
    public void StartShooting()
    {
        _animator.SetBool("IsShooting",true);
        
    }
    public void StopShooting()
    {
        _animator.SetBool("IsShooting", false);
        _isShooting = false;
    }
    public bool IsMoving() => _currentAnimationState == AnimationState.Walk;

}
