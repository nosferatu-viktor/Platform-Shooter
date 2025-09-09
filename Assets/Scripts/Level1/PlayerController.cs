using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _smoothTime = 0.1f;
    [SerializeField] private float _zSpeedMultiplier = 0.8f; //z ekseninde daha yavaþ hareket etmesini istiyorum

    [Header("Sprite Direction")]
    [SerializeField] private bool _flipSpriteForDirection = true; //sprite'ý yönümüze göre çevirme kontrolü
    [SerializeField] private float _directionThreshHold = 0.1f; //yön deðiþtirmek için gereken minimum hareket miktarý


    [Header("Shooting Settings")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 0.3f;
    private float _nextFireTime;
    Vector3 _fireposition;

    private Rigidbody2D _playerRB;
    [SerializeField]private SpriteRenderer _spriteRenderer;
    
    private Vector2 _currentVelocity;
    private Vector2 _velocitySmoothing;
    private Vector2 _inputVector;
    private bool _facingRight = true;
    private BoxCollider2D _boxCollider;
    [SerializeField] private PlayerAnimationController _playerAnimationController;
    //-1.1 / -4.0
    private void Start()
    {
        _playerRB = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();

        _playerRB.gravityScale = 0f;
    }
    private void Update() 
    {
        SetInput();
        SetShooting();
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

            //Input vektörünü kaydet
            _inputVector = new Vector2(horizontal, vertical).normalized;
    }
    private void SetMovement()
    {
            Vector2 moveVelocity = new Vector2(_inputVector.x * _moveSpeed, _inputVector.y * _moveSpeed * _zSpeedMultiplier); //z ekseni için farklý hýz
            _currentVelocity = Vector2.SmoothDamp(_currentVelocity, moveVelocity, ref _velocitySmoothing, _smoothTime); //yumuþak hareket geçiþi
            _playerRB.linearVelocity = _currentVelocity;
            _playerAnimationController.UpdateAnimations(_currentVelocity.magnitude > 0.1f ? AnimationState.Walk : AnimationState.Idle);
            UpdateDepthSorting();//z ekseni için depth sorting
    }
    private void UpdateDepthSorting()
    {
        Vector3 pos = transform.position;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = (int)MathF.Round(-pos.y); //karakterin orderini deðiþtirerek nesnelerin karakterin önüne veya arkasýna geçmesi durumu
        }
    }
    private void UpdateSpriteDirection()
    {
        if(_spriteRenderer == null)
        {  return; }

        
        //X eksenindeki hareket ediþ miktarýma göre spriteýmý çevir
        if (_inputVector.x > _directionThreshHold && !_facingRight)
        {
            FlipSprite();
            _boxCollider.offset = new Vector2(-0.07360274f, 0);
        }
        else if (_inputVector.x < -_directionThreshHold && _facingRight)
        {
            FlipSprite();
            _boxCollider.offset = new Vector2(0.09f, 0);
        }
    }
    private void FlipSprite()
    {
        _facingRight = !_facingRight;
        _spriteRenderer.flipX = !_facingRight;
    }
    private void SetShooting()
    {//Input.GetButton("Fire1")
        if (Input.GetMouseButtonDown(0) && Time.time >= _nextFireTime)
        {
            _playerAnimationController.StartShooting();
            _playerAnimationController.UpdateAnimations(AnimationState.Shoot);
            Invoke(nameof(Shoot),0.4f);
        }
    }
    private void Shoot()
    {
        _nextFireTime = Time.time + _fireRate;
        if (_bulletPrefab != null && _firePoint != null)
        {
            if(_facingRight) _fireposition = new Vector3(_firePoint.transform.position.x + 0.2f, _firePoint.transform.position.y + 0.7f, 0);
            else _fireposition = new Vector3(_firePoint.transform.position.x - 0.2f, _firePoint.transform.position.y + 0.7f, 0);
            Vector3 ShootDirection = _facingRight ? Vector3.right : Vector3.left;
            GameObject bullet = Instantiate(_bulletPrefab, _fireposition, Quaternion.identity);
            //mermi yönünü ayarlama
            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            if (bulletRB != null)
            {
                bulletRB.linearVelocity = ShootDirection * 100; //mermi hýzý
            }
        }
    }


    public Vector2 GetVelocity() => _currentVelocity;
    public bool IsFacingRight() => _facingRight;
    public Vector2 GetInputDirection() => _inputVector;
    private void OnDrawGizmos()
    {
        //hareket yönünü göster
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
