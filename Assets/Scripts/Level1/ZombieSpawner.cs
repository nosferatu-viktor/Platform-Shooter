using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{   //Total Zombie/maxzombies alive/_spawninternal
    public static ZombieSpawner Instance;
    [Tooltip("Kaç Saniyede bir zombi spawn edilecek")]
    public float _spawnInterval;

    [Tooltip("Maks Kaç zombi hayatta")]
    public int _maxZombiesAlive;

    [Header("Zombie prefab")]
    [SerializeField] private GameObject _zombieMan;
    [SerializeField] private GameObject _zombieWoman;

    [Header("Preferences")]
    [Tooltip("Player Referansý")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private TextMeshProUGUI _zombieKillText;
    [SerializeField] private TextMeshProUGUI _countText;

    static List<GameObject> _activeZombies;
    private float _lastSpawnTime;
    private Vector3 _lastPlayerPosition;
    private int _spawnZombieCount = 0;
    public int _totalZombie;
    public int _zombieKill = 0;
    private void Start()
    {
        Instance = this;
        _activeZombies = new List<GameObject>();
        if(_playerTransform == null)
        {
            GameObject _playerobj = GameObject.FindGameObjectWithTag("Player");
            if(_playerobj != null)
            {
                _playerTransform = _playerobj.transform;
            }
            else
            {
                Debug.Log("player objesi bulunamadý");
            }
        }
        _lastSpawnTime = Time.time;
        _lastPlayerPosition = _playerTransform.position;
        _totalZombie = GameData._totalZombie;
        _maxZombiesAlive = GameData._maxZombiesAlive;
        _spawnInterval = GameData._spawnInterval;
    }
    private void Update()
    {
        if (_playerTransform == null || _mainCamera == null || _zombieMan == null || _zombieWoman == null) return;
        CleanupDeadZombies();
        CheckSpawnTiming();
        _zombieKillText.text = _zombieKill.ToString();
        _countText.text = _totalZombie.ToString();
    }
    private void CleanupDeadZombies()
    {
        for (int i = _activeZombies.Count - 1; i >= 0; i--)
        {
            if (_activeZombies[i] == null)
            {
                _activeZombies.RemoveAt(i);
            }
        }
    }
    private void CheckSpawnTiming()
    {
        bool _enoughTimePassed = (Time.time - _lastSpawnTime) >= _spawnInterval;
        bool _underZombieLimit = _activeZombies.Count < _maxZombiesAlive;
        if(_enoughTimePassed&&_underZombieLimit && _spawnZombieCount<=_totalZombie)
        {
            SpawnZombie();
            _lastSpawnTime = Time.time;
        }
    }
    private void SpawnZombie()
    {
        //Vector3 _cameraPosition = _mainCamera.transform.position;
        //Vector3 _cameraForward = _mainCamera.transform.forward;

        //Vector3 _baseSpawnPosition = _cameraPosition + (_cameraForward*_spawnDistanceAhead);
        //float _randomXoffset = Random.Range(-_spawnAreaWidth / 2f, _spawnAreaWidth / 2f);
        //Vector3 _cameraRight = Vector3.Cross(_cameraForward,Vector3.up).normalized;
        Vector3 _spawnPosition = new Vector3(this.transform.position.x + Random.Range(0, 5), Random.Range(-1.1f, -3.9f), 0);
        GameObject newZombie;
        int rnd = Random.Range(-2,2);
        if (rnd <= 0)
        {
             newZombie = Instantiate(_zombieMan, _spawnPosition, Quaternion.identity);
        }
        else
        {
            newZombie = Instantiate(_zombieWoman, _spawnPosition, Quaternion.identity);
        }


            AssignPlayerZombie(newZombie);
        _activeZombies.Add(newZombie);
        newZombie.name = $"{_activeZombies.Count}_{Time.time:F0}";
        _spawnZombieCount++;
        //Debug.Log(newZombie);

    }

    private void AssignPlayerZombie(GameObject Zombie)
    {
        ZombieAI zombieAI = Zombie.GetComponent<ZombieAI>();
        if (zombieAI != null) 
        {
            
        }
    }

    public static void IsDead(GameObject zombi) => NotifyZombieDeath(zombi);
    public static void NotifyZombieDeath(GameObject deadZombie)
    {
        if (_activeZombies.Contains(deadZombie))
        {
            _activeZombies.Remove(deadZombie);
        }
    }
    public void ClearAllZombies()
    {
        foreach (GameObject zombie in _activeZombies)
        {
            if(zombie != null)
            {
                Destroy(zombie);
            }
        }
        _activeZombies.Clear();
    }
}
