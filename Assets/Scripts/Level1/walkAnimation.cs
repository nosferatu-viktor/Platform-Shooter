using UnityEngine;

public class walkAnimation : MonoBehaviour
{
    private AudioSource _audioSource;
    private bool _isMoving;
    private bool _isShoot;
    private bool _isRecharge;
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        _isMoving = PlayerController.instance._isMoving;
        _isShoot = PlayerController.isShoot;
        _isRecharge = PlayerController.instance._recharge;
        if (_isMoving==true && _isShoot == false && _isRecharge == false)
        {
            _audioSource.enabled = true;
        }
        else
        {
            _audioSource.enabled=false;
        }
    }
}
