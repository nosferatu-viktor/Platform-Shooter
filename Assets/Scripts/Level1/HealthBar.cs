using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public static HealthBar instance;

    [SerializeField] private float _health;
    [SerializeField] private Image _healthImage;
    private float _maxCan;
    private void Start()
    {
        instance = this;
        _maxCan = _health;

    }
    private void Update()
    {
        if (_health < 20)
        {
            _healthImage.color = Color.red;
        }

        if (_health > 20)
        {
            _healthImage.color = Color.green;
        }
    }
    public void HealthDown()
    {
        _health -= 18;
        _healthImage.fillAmount = _health/_maxCan;
    }
    public void HealthUp()
    {
        _health += 25;
        _healthImage.fillAmount = _health / _maxCan;
    }
    public void HealthReFull()
    {
        _health = 100;
        _healthImage.fillAmount = _health / _maxCan;
    }
}
