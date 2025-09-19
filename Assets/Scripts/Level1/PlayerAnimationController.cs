using TMPro;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    [Header("Animation Settings")]
    [SerializeField] private string _idleAnimationName = "Idle";
    [SerializeField] private string _walkAnimationName = "Walk";
    [SerializeField] public Animator _animator;
    private AnimationState _currentAnimationState = AnimationState.Idle;
    public bool _isShooting=false;
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
                    _animator.speed = Mathf.Lerp(0.8f, 1.5f, PlayerController.instance._currentVelocity.magnitude / 2.5f);
                    break;
                case AnimationState.Shoot:
                    _isShooting = true;
                    _animator.SetTrigger("Shoot");
                    Invoke(nameof(StopShooting), 0.5f);
                    break;
                case AnimationState.Hurt:
                    _animator.SetTrigger("IsHurt");
                    break;
                case AnimationState.Recharge:
                    _animator.SetBool("IsRecharging",true);
                    _animator.SetTrigger("Recharge");
                    break;
                case AnimationState.Dead:
                    _animator.SetTrigger("IsDead");
                    _animator.Play("Dead");
                    break;
            }
        }
    }
    public void StartShooting()
    {
        _animator.SetBool("IsShooting",true);
        PlayerController.IsShoot();
    }
    public void StopShooting()
    {
        _animator.SetBool("IsShooting", false);
        _isShooting = false;
        PlayerController.IsShoot();
    }
    public bool IsMoving() => _currentAnimationState == AnimationState.Walk;

}
