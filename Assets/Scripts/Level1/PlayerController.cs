using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _smoothTime = 0.1f;
    [SerializeField] private float _zSpeedMultiplier = 0.8f; //z ekseninde daha yava� hareket etmesini istiyorum

    [Header("Sprite Direction")]
    [SerializeField] private bool _flipSpriteForDirection = true; //sprite'� y�n�m�ze g�re �evirme kontrol�
    [SerializeField] private float _directionThreshHold = 0.1f; //y�n de�i�tirmek i�in gereken minimum hareket miktar�

    [Header("Animation Settings")]
    [SerializeField] private float _walkAnimationSpeed = 1f;
    [SerializeField] private string _idleAnimationName = "Idle";
    [SerializeField] private string _walkAnimationName = "Walk";

    [Header("Shooting Settings")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 0.3f;

    private Rigidbody2D _playerRB;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Vector2 _currentVelocity;
    private Vector2 _velocitySmoothing;
    private Vector2 _inputVector;
    private bool _facingRight = true;
    private float _nextFireTime;

    private AnimationState _currentAnimationState = AnimationState.Idle;

    private void Start()
    {
        _playerRB = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _playerRB.gravityScale = 0f;
    }
    private void Update() 
    {
        SetInput();
        SetShooting();
        UpdateAnimations();
        UpdateSpriteDirection();
    }
    private void FixedUpdate()
    {
        SetMovement();
    }
    private void SetInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //Input vekt�r�n� kaydet
        _inputVector = new Vector2(horizontal, vertical).normalized;
    }
    private void SetMovement()
    {
        Vector2 moveVelocity = new Vector2(_inputVector.x * _moveSpeed, _inputVector.y * _moveSpeed * _zSpeedMultiplier); //z ekseni i�in farkl� h�z
        _currentVelocity = Vector2.SmoothDamp(_currentVelocity, moveVelocity , ref _velocitySmoothing, _smoothTime); //yumu�ak hareket ge�i�i
        _playerRB.linearVelocity = _currentVelocity;
        _currentAnimationState = _currentVelocity.magnitude > 0.1f? AnimationState.Walk : AnimationState.Idle; //state g�ncelledik
        UpdateDepthSorting(); //z ekseni i�in depth sorting
    }
    private void UpdateDepthSorting()
    {
        Vector3 pos = transform.position;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = (int)MathF.Round(-pos.y *100); //karakterin orderini de�i�tirerek nesnelerin karakterin �n�ne veya arkas�na ge�mesi durumu
        }
    }
    private void UpdateSpriteDirection()
    {
        if(!_flipSpriteForDirection ||_spriteRenderer == null)
            return;

        //X eksenindeki hareket edi� miktar�ma g�re sprite�m� �evir
        if (_inputVector.x > _directionThreshHold && !_facingRight)
        {
            FlipSprite();
        }
        else if (_inputVector.x < -_directionThreshHold && _facingRight)
        {
            FlipSprite();
        }
    }
    private void FlipSprite()
    {
        _facingRight = !_facingRight;
        _spriteRenderer.flipX = !_facingRight;
    }
    private void UpdateAnimations()
    {
        if (_animator == null)
            return;

        switch (_currentAnimationState)
        {
            case AnimationState.Idle:_animator.Play(_idleAnimationName);
                break;
            case AnimationState.Walk:
                _animator.Play(_walkAnimationName);
                //animasyon h�z�n� movement h�z�na ayarlad�k
                _animator.speed = Mathf.Lerp(0.8f, 1.5f, _currentVelocity.magnitude / _moveSpeed);
                break;
        }
    }
    private void SetShooting()
    {//Input.GetButton("Fire1")
        if ( Input.GetMouseButtonDown(0)&& Time.time >= _nextFireTime)
        {
            Shoot();
        }
    }
    private void Shoot()
    {
        _nextFireTime = Time.time + _fireRate;
        if (_bulletPrefab != null && _firePoint!=null)
        {
            Vector3 ShootDirection = _facingRight ? Vector3.right : Vector3.left;
            GameObject bullet = Instantiate( _bulletPrefab,_firePoint.position,Quaternion.identity);
            //mermi y�n�n� ayarlama
            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            if(bulletRB!=null)
            {
                bulletRB.linearVelocity = ShootDirection*10; //mermi h�z�
            }
        }
    }
    public bool IsMoving() => _currentAnimationState == AnimationState.Walk;
    public Vector2 GetVelocity() => _currentVelocity;
    public bool IsFacingRight() => _facingRight;
    public Vector2 GetInputDirection() => _inputVector;
    private void OnDrawGizmos()
    {
        //hareket y�n�n� g�ster
        if(Application.isPlaying && _inputVector.magnitude >0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, new Vector3(_inputVector.x,0,_inputVector.y) * 2f);
        }
        if (_firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position,0.2f);
        }
    }
}
