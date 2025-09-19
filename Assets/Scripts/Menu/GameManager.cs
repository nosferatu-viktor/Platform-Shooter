using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManagerMenu : MonoBehaviour
{
    [SerializeField] private GameObject _menuStartPanel;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private GameObject _difficultPanel;
    [SerializeField] private Button _easyButton;
    [SerializeField] private Button _normalButton;
    [SerializeField] private Button _hardButton;
    [SerializeField] private Button _returnButton;
    void Start()
    {
        _menuStartPanel.SetActive(true);
        _difficultPanel.SetActive(false);
        _playButton.onClick.AddListener(PlayGame);
        _quitButton.onClick.AddListener(QuitGame);
        _easyButton.onClick.AddListener(Easy);
        _normalButton.onClick.AddListener(Normal);
        _hardButton.onClick.AddListener(Hard);
        _returnButton.onClick.AddListener(Return);
    }
    private void PlayGame()
    {
        _difficultPanel.SetActive(true);
        _menuStartPanel.SetActive(false);
    }
    private void QuitGame()
    {
        #if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

        #else
                    Application.Quit();
        #endif
    }
    private void Easy()
    {
        GameData._reFullHealth = 5;
        GameData._totalZombie= 60;
        GameData._maxZombiesAlive = 8;
        GameData._spawnInterval = 4f;
        GameData._zombiemaxHealth = 100f;
        GameData._leveldifficult = "Easy";
        SceneManager.LoadScene(1);
    }
    private void Normal()
    {
        GameData._reFullHealth = 4;
        GameData._totalZombie = 90;
        GameData._maxZombiesAlive = 12;
        GameData._spawnInterval = 3f;
        GameData._zombiemaxHealth = 125f;
        GameData._leveldifficult = "Normal";
        SceneManager.LoadScene(1);
    }
    private void Hard()
    {
        GameData._reFullHealth = 3;
        GameData._totalZombie = 120;
        GameData._maxZombiesAlive = 20;
        GameData._spawnInterval = 1f;
        GameData._zombiemaxHealth = 150f;
        GameData._leveldifficult = "Hard";
        SceneManager.LoadScene(1);
    }
    private void Return()
    {
        _difficultPanel.SetActive(false);
        _menuStartPanel.SetActive(true);
    }
}
