using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _baseOffset = new Vector3(0, 2f, -10f);
    [SerializeField] private float _followSmoothing = 2f;

    [Header("Cinematic Effects")]
    [SerializeField] private float _lookAheadDistance = 3f;
    [SerializeField] private float _lookAheadSmoothing = 3f;
    [SerializeField] private bool _enableScreenShake = true;
    [SerializeField] private bool _enableDynamicZoom = true;

    [Header("Zoom Settings")]
    [SerializeField] private float _baseZoom = 5f;
    [SerializeField] private float _actionZoom = 4f;
    [SerializeField] private float _dangerZoom = 6f;
    [SerializeField] private float _zoomSpeed = 2f;

    [Header("Shake Settings")]
    [SerializeField] private float _shakeIntensity = 0.1f;
    [SerializeField] private float _shakeDuration = 0.2f;

    [Header("Movement Prediction")]
    [SerializeField] private float _velocityInfluence = 1.5f;
    [SerializeField] private float _maxVelocityOffset = 4f;

    private Camera _camera;
    [SerializeField] private PlayerController _playerController;
    private Vector3 _currentVelocity;
    private Vector3 _lookAheadPosition;
    private float _currentLookAhead;
    private float _lookAheadVelocity;

    private float _shakeTimeRemaining;
    private Vector3 _shakeOffset;

    private float _targetZoom;
    private float _currentZoomVelocity;

    public CameraStates currentState = CameraStates.Following;

    private void Start()
    {
        _camera = GetComponent<Camera>();

        if(_target!=null)
        {
            _playerController = GetComponent<PlayerController>();
        }

        _targetZoom = _baseZoom;
        _camera.orthographicSize = _baseZoom;
    }
    private void LateUpdate()
    {
        if (_target == null) return;
        UpdateCameraState();
        UpdateLookAhead();
        UpdatePosition();
        UpdateZoom();
        UpdateShake();
    }
    private void UpdateCameraState()
    {
        if (_playerController == null)
        { return; }

        if (_playerController.IsMoving())
        {
            currentState = CameraStates.Following;
        }

        if (Input.GetMouseButtonDown(0))
        {
            currentState = CameraStates.Action;
        }
    }
    private void UpdateLookAhead()
    {
        if (_playerController == null) { return; }
        Vector2 inputDir = _playerController.GetInputDirection();
        Vector2 velocity = _playerController.GetVelocity();

        //look ahead direction hesaplama
        Vector3 targetLookAhead = Vector3.zero;
        switch(currentState)
        {
            case CameraStates.Following:
                //hareket yönünde kamerayý ileri hareket ettir
                targetLookAhead = new Vector3(inputDir.x * _lookAheadDistance, inputDir.y * _lookAheadDistance * 0.5f,0);
                //hýz bazlý offset ekliyoruz
                Vector3 velocityOffset = new Vector3(velocity.x * _velocityInfluence,velocity.y*_velocityInfluence*0.5f,0);
                velocityOffset = Vector3.ClampMagnitude(velocityOffset, _maxVelocityOffset);
                targetLookAhead += velocityOffset;
                break;
            case CameraStates.Action:
                //ateþ ederken minimal look ahead
                targetLookAhead = Vector3.zero;
                break;
            case CameraStates.Danger:
                //tehlike anýnda geri çekil
                targetLookAhead =new Vector3(0,1f,0);
                break;
        }
        //smooth þekilde look ahead yapma
        _lookAheadPosition = Vector3.Lerp(_lookAheadPosition, targetLookAhead, _lookAheadSmoothing * Time.deltaTime);
    }
    private void UpdatePosition()
    {
        //target position hesapla
        Vector3 targetPosition = _target.position + _baseOffset + _lookAheadPosition + _shakeOffset;

        //smooth camera movement
        transform.position = Vector3.SmoothDamp(transform.position,targetPosition,ref _currentVelocity,1f/_followSmoothing);

        //z pozisyonu fixleme
        Vector3 pos = transform.position;
        pos.z = _baseOffset.z;
        transform.position = pos;
    }
    private void UpdateZoom()
    {
        if (!_enableDynamicZoom) return;

        switch(currentState)
        {
            case CameraStates.Following:
                _targetZoom = _baseZoom;
                break;
            case CameraStates.Action:
                _targetZoom = _actionZoom;
                break;
            case CameraStates.Danger:
                _targetZoom = _dangerZoom;
                break;
        }
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _targetZoom, ref _currentZoomVelocity, 1f / _zoomSpeed);
    }
    private void UpdateShake()
    {
        if (_shakeTimeRemaining > 0)
        {
            _shakeTimeRemaining -= Time.deltaTime;


            _shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * _shakeIntensity;

            float shakeStrength = _shakeTimeRemaining / _shakeDuration;
            _shakeOffset *= shakeStrength;
        }
        else
        {
            _shakeOffset = Vector3.Lerp(_shakeOffset, Vector3.zero,Time.deltaTime * 5f);
        }
    }
    //public methods
    public void TriggerShake(float intensity = -1f,float duration = -1f)
    {
        if(!_enableScreenShake) { return; }
        _shakeIntensity = intensity > 0 ? intensity : this._shakeIntensity;
        _shakeTimeRemaining = duration > 0 ? duration : this._shakeDuration;
    }    
    public void SetCameraState (CameraStates newstate)
    {
        currentState = newstate;
    }
    public void FocusOnTarget (Transform newtarget, float duration = 2f)
    {
        StartCoroutine(FocusCoroutine(newtarget,duration));
    }
    System.Collections.IEnumerator FocusCoroutine(Transform focustarget,float duration)
    {
        Transform originnaltarget = _target;
        _target = focustarget;
        yield return new WaitForSeconds(duration);
        _target = originnaltarget;
    }
    private void OnDrawGizmos()
    {
        //look ahead direction gösterir
        if (_target==null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_target.position + _lookAheadPosition, 0.5f);

        //camera bounds gösterir
        if (_camera != null)
        {
            Gizmos.color = Color.cyan;
            float height = _camera.orthographicSize * 2f;
            float width = height * _camera.aspect;
            Vector3 pos = transform.position;
            pos.z = 0;
            Gizmos.DrawWireCube(pos, new Vector3(width, height, 0));
        }

    }
}
