using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
public class ZombieAI : MonoBehaviour
{   //max health
    public static ZombieAI Instance;
    public enum ZombieState
    {
        Idle,
        Chasing,
        Attacking,
        Hurt,
        Dead
    }
    public ZombieState _currentState = ZombieState.Idle;
    [SerializeField] private Image _healthImage;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private float _moveSpeed = 0.5f;
    [SerializeField] private float _detectionRange = 15f;
    private float _attackRange = 0.5f;
    [SerializeField] private float _attackCoolDown = 3.5f;
    [SerializeField] private int _attackDamage = 25;
    private float _maxHealth;
    private Transform _targetTransform;
    private Rigidbody2D _zombieRigidBody;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private BoxCollider2D _boxCollider;
    private bool Attack=false;
    private float distanceToPlayer;
    private float _currentHealth;
    private bool _facingRight=false;
    private bool _isHit=false;
    private float _attackTimer=0f;
    private float _soundTimer = 0f;
    private int _rnd = 0;
    [SerializeField] public AudioSource _audioSource;
    [SerializeField] public AudioClip _scream1;
    [SerializeField] public AudioClip _scream2;
    [SerializeField] public AudioClip _scream3;
    [SerializeField] public AudioClip _dead;
    [SerializeField] public AudioClip _bite;
    private void Start()
    {
        _rnd = UnityEngine.Random.Range(1, 4);
        switch (_rnd)
        {
            case 1:
                _audioSource.clip = _scream1;
                _audioSource.Play();
                break;
            case 2:
                _audioSource.clip = _scream2;
                _audioSource.Play();
                break;
            case 3:
                _audioSource.clip = _scream3;
                _audioSource.Play();
                break;
        }
        _soundTimer = 0f;
        Instance = this;
        InitializeComponents();
        InitializeStats();
        UpdateDepthSorting();
    }
    private void Update()
    {
        _soundTimer += Time.deltaTime;
        if(_soundTimer > 10)
        {
            _soundTimer = 0;
            _rnd = UnityEngine.Random.Range(1,4); 
            switch(_rnd)
            {
                case 1:
                    _audioSource.clip = _scream1;
                    _audioSource.Play();
                    break;
                case 2:
                    _audioSource.clip = _scream2;
                    _audioSource.Play();
                    break;
                case 3:
                    _audioSource.clip = _scream3;
                    _audioSource.Play();
                    break;
            }
        }

        _targetTransform = PlayerController.instance.transform;
        distanceToPlayer = Vector3.Distance(transform.position, _targetTransform.position);
        if (_currentState == ZombieState.Dead)
        {
            return;
        }
        if (Attack)
        {
            _attackTimer += Time.deltaTime;
            
            if (_attackTimer == 0.1f)
            {
                PlayerController.instance.IsHurt();
                
            }
            if (_attackTimer > _attackCoolDown)
            {
                SetAttackBool();
                _attackTimer = 0f;
            }
        }
        UpdateAI();
        SetMoving();
    }
    private void ChangeAnimation(ZombieState state)
    {
        _currentState = state;
        switch (_currentState)
        {
            case ZombieState.Idle:
                _animator.SetBool("IsMoving",false);
                _animator.Play("Idle");
                break;
            case ZombieState.Chasing:
                _animator.SetBool("IsMoving", true);
                _animator.Play("Walk");
                break;
            case ZombieState.Attacking:
                _animator.SetTrigger("IsBite");
                Attack = true;
                EventManager.Instance.ZombieAttackTrigger();
                _animator.SetBool("IsMoving", false);
                break;
            case ZombieState.Hurt:
                _animator.SetTrigger("IsHurt");
                Invoke(nameof(IsHit),0.5f);
                    break;
            case ZombieState.Dead:
                _animator.SetTrigger("IsDead");
                _audioSource.clip = _dead;
                _audioSource.Play();
                Destroy(_zombieRigidBody);
                Destroy(_boxCollider);
                Invoke(nameof(IsDead), 1.5f);
                break;
        }
    }
    private void IsHit()
    {
        _isHit = !_isHit;
    }
    private void SetAttackBool()
    {
        Attack = !Attack;
    }
    private void InitializeComponents()
    {
        _zombieRigidBody = GetComponent<Rigidbody2D>();
        _zombieRigidBody.gravityScale = 0f;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _targetTransform = PlayerController.instance.transform;
        Attack = false;
    }
    private void InitializeStats()
    {
        _maxHealth = GameData._zombiemaxHealth;
        _currentHealth = _maxHealth;
    }
    
    private void UpdateAI()
    {
        
            if (_currentState != ZombieState.Idle)
            {
                _currentState = ZombieState.Idle;
            }
            
        
        
        if (Attack == false && _isHit == false)
        {
            ZombieState newState = _currentState switch
            {
                _ when _currentHealth <= 0 => ZombieState.Dead,
                _ when distanceToPlayer >= _detectionRange => ZombieState.Idle,
                _ when distanceToPlayer <= _detectionRange && distanceToPlayer <= _attackRange => ZombieState.Attacking,
                _ when distanceToPlayer <= _detectionRange && distanceToPlayer >= _attackRange => ZombieState.Chasing,
                _ => ZombieState.Idle
            };
            ChangeAnimation(newState);
        }
    }
    private void UpdateDepthSorting()
    {
        Vector3 pos = transform.position;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = (int)MathF.Round(-pos.y);
            _canvas.sortingOrder = _spriteRenderer.sortingOrder;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("bullet") && _spriteRenderer.sortingOrder == PlayerController.instance.SortingOrder())
        {
            Hit();
            Destroy(collision.gameObject);
        }
    }
    private void Hit()
    {
        ChangeAnimation(ZombieState.Hurt);
        _currentHealth -= _attackDamage;
        ZombieHit();
        IsHit();
    }
    private void IsDead()
    {
            ZombieSpawner.IsDead(this.gameObject);
            ZombieSpawner.Instance._zombieKill++;
            Destroy(gameObject);
    }
    private void SetMoving()
    {
        if (!Attack && _currentState == ZombieState.Chasing)
        {
            Vector2 move;
            if (_targetTransform.position.y < transform.position.y)
            {
                move.y = -0.1f;
            }
            else if (_targetTransform.position.y > transform.position.y)
            {
                move.y = 0.1f;
            }
            else
            {
                move.y = 0.0f;
            }
            if (_targetTransform.position.x < transform.position.x && distanceToPlayer>_attackRange)
            {
                move.x = -_moveSpeed;
                FlipSprite();
            }
            else if (_targetTransform.position.x > transform.position.x && distanceToPlayer >_attackRange)
            {
                move.x = _moveSpeed;
                FlipSprite();
            }
            else
            {
                move.x = 0;
            }
            _zombieRigidBody.linearVelocity = move;
            
        }
        else
        {
            _zombieRigidBody.linearVelocity = Vector2.zero;
        }
            UpdateDepthSorting();
    }
    private void FlipSprite()
    {
        if (_targetTransform.position.x < transform.position.x && _facingRight)
        {
            _facingRight = !_facingRight;
            _spriteRenderer.flipX = !_facingRight;
        }
        else if (_targetTransform.position.x > transform.position.x && !_facingRight)
        {
            _facingRight = !_facingRight;
            _spriteRenderer.flipX = !_facingRight;
        }
    }

    private void ZombieHit()
    {
        if (_currentHealth<26)
        {
            _healthImage.color = Color.red;
        }
        _healthImage.fillAmount = _currentHealth / _maxHealth;
    }
    public  int SortingOrder()
    {
        return _spriteRenderer.sortingOrder;
    }
}