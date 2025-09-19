using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    public delegate void ZombieAttack();
    private ZombieAttack _zombieAttackFunction;

    public delegate void CharacterHealth();
    private CharacterHealth _healthUp;
    private CharacterHealth _healtReFull;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        _zombieAttackFunction = ZombieAttackDelegate;
        _healthUp += HealthUpCharacter;
        _healtReFull += HealthReFull;
    }
    private void ZombieAttackDelegate()
    {
        PlayerController.instance.Health -= 18;
        PlayerController.instance.IsHurt();
        HealthBar.instance.HealthDown();
        ZombieAI.Instance._audioSource.clip = ZombieAI.Instance._bite;
        ZombieAI.Instance._audioSource.Play();
    }
    private void HealthUpCharacter()
    {
        PlayerController.instance.Health += 25;
        HealthBar.instance.HealthUp();
    }
    private void HealthReFull()
    {
        PlayerController.instance.Health = 100f;
        HealthBar.instance.HealthReFull();
    }
    public void ZombieAttackTrigger()
    {
        _zombieAttackFunction();
    }
    public void HealthUpTrigger()
    {
        _healthUp();
    }
    public void HealthReFullTrigger()
    {
        _healtReFull();
    }
}
