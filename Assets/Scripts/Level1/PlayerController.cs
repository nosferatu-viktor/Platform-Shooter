using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2.5f;
    [SerializeField] private float _smoothTime = 0.1f;
    [SerializeField] private float _zSpeedMultiplier = 0.8f; //z ekseninde daha yavaþ hareket etmesini istiyorum

    [Header("Sprite Direction")]
    [SerializeField] private float _directionThreshHold = 0.1f; //yön deðiþtirmek için gereken minimum hareket miktarý

    [Header("Shooting Settings")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;


    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private TextMeshProUGUI _reFullText;
    Vector3 _fireposition;
    public static bool isShoot = false;
    private static bool isHurt = false;
    public bool _isAlive = true;
    public bool _recharge = false;
    private bool _facingRight = true;
    public bool _isMoving = false;
    private Rigidbody2D _playerRB;
    [SerializeField]private SpriteRenderer _spriteRenderer;
    public Vector2 _currentVelocity;
    private Vector2 _velocitySmoothing;
    private Vector2 _inputVector;
    private BoxCollider2D _boxCollider;
    private int _maxBullet = 20;
    private int _clip;
    private bool _isGamePaused;
    public int _reFullHealth;
    [SerializeField] private PlayerAnimationController _playerAnimationController;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _reCharge;
    [SerializeField] private AudioClip _bullet;

    public float Health = 100f;
    //-1.1 / -4.0
    private void Start()
    {
        Health = 100f;
        _playerRB = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        instance = this;
        _recharge = false;
        isHurt= false;
        _isAlive = true;
        _facingRight = true;
        isShoot = false;
        _isMoving = false;
        _playerRB.gravityScale = 0f;
        _clip = _maxBullet;
        _textMeshPro.text = _clip.ToString();
        _isGamePaused = false;
        _reFullHealth = GameData._reFullHealth;
    }
    private void Update() 
    {
        SetInput();
        SetShooting();
        UpdateSpriteDirection();
        SetClip();
        SetReFull();
        _textMeshPro.text = _clip.ToString();
        _reFullText.text = _reFullHealth.ToString();
        _isGamePaused = GameManager.Instance._isGamePaused;
    }
    private void FixedUpdate()
    {
        SetMovement();
    }
    private void SetInput()
    {
        if (isHurt==false && _isGamePaused == false)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            //Input vektörünü kaydet
            _inputVector = new Vector2(horizontal, vertical).normalized;
        }
    }
    private void SetMovement()
    {
        Vector2 moveVelocity = new Vector2(_inputVector.x * _moveSpeed, _inputVector.y * _moveSpeed * _zSpeedMultiplier); //z ekseni için farklý hýz
        _currentVelocity = Vector2.SmoothDamp(_currentVelocity, moveVelocity, ref _velocitySmoothing, _smoothTime); //yumuþak hareket geçiþi
        if (Health <= 0)
        {
            if (_isAlive)
            {
                _playerRB.linearVelocity = Vector2.zero;
                _playerAnimationController.UpdateAnimations(AnimationState.Dead);
                _isAlive = false;
            }
            else { return; }
        }
        else
        {
            
                if (isShoot == false && isHurt == false && _recharge == false)
                {
                    _playerRB.linearVelocity = _currentVelocity;
                    _playerAnimationController.UpdateAnimations(_playerRB.linearVelocity.magnitude > 0.1f ? AnimationState.Walk : AnimationState.Idle);
                if (_playerRB.linearVelocity.magnitude > 0.1f)
                {
                    _isMoving = true;
                }
                else
                {
                    _isMoving = false;
                }
                }
                else if(isHurt== true)
                {
                _playerAnimationController.UpdateAnimations(AnimationState.Hurt);
                _playerRB.linearVelocity = Vector2.zero;
                }
                else
                {
                    _playerRB.linearVelocity = Vector2.zero;
                }
        }
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
    {
        if (Input.GetMouseButtonDown(0) && isShoot==false && _clip > 0 && _isGamePaused == false && isHurt == false && _recharge==false)
        {
            _playerAnimationController.StartShooting();
            _playerAnimationController.UpdateAnimations(AnimationState.Shoot);
            Invoke(nameof(Shoot),0.4f);
        }
    }
    private void Shoot()
    {
        if (_bulletPrefab != null && _firePoint != null )
        {
            if(_facingRight) _fireposition = new Vector3(_firePoint.transform.position.x + 0.2f, _firePoint.transform.position.y + 0.7f, 0);
            else _fireposition = new Vector3(_firePoint.transform.position.x - 0.2f, _firePoint.transform.position.y + 0.7f, 0);
            Vector3 ShootDirection = _facingRight ? Vector3.right : Vector3.left;
            GameObject bullet = Instantiate(_bulletPrefab, _fireposition, Quaternion.identity);
            //mermi yönünü ayarlama
            Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
            _audioSource.clip = _bullet;
            _audioSource.Play();
            if (bulletRB != null)
            {
                bulletRB.linearVelocity = ShootDirection * 20; //mermi hýzý
                _clip--;
            }
        }
    }
    private void SetClip()
    {
        if (Input.GetKeyDown(KeyCode.R) && _recharge ==false)
        {
            _recharge = true;
            _playerAnimationController.UpdateAnimations(AnimationState.Recharge);
            _audioSource.clip = _reCharge;
            _audioSource.Play();
            Invoke(nameof(Recharge),1f);
        }
    }
    private void Recharge()
    {
        _clip = _maxBullet;
        _recharge = false;
        _playerAnimationController._animator.SetBool("IsRecharging", false);
    }
    private void SetReFull()
    {
        if (Input.GetKeyDown(KeyCode.E) && _reFullHealth>0)
        {
            EventManager.Instance.HealthReFullTrigger();
            _reFullHealth--;
        }
    }
    public static void IsShoot()
    {
        isShoot = !isShoot;      
    }

    public int SortingOrder()
    {
        return _spriteRenderer.sortingOrder;
    }
    public  void IsHurt()
    {
        isHurt = !isHurt;
        if(isHurt == true)
        Invoke(nameof(IsHurt),1.4f);
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
