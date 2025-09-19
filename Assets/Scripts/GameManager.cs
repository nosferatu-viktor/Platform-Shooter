using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private TextMeshProUGUI _pauseText;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _soundButton;

    [Header("Death Panel Referanslarý")]
    [SerializeField] private GameObject _deathPanel;
    [SerializeField] private TextMeshProUGUI _deathText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainmenu2Button;

    [Header("Win Panel Referanslarý")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private TextMeshProUGUI _winText;
    [SerializeField] private Button _nextLevel;
    [SerializeField] private Button _mainmenu3Button;

    public bool _isGamePaused = false;
    private bool _isSoundPaused = false;

    public static GameManager Instance;
    private void Start()
    {
        _pausePanel.SetActive(false);
        _deathPanel.SetActive(false);
        _winPanel.SetActive(false);
        _pauseButton.onClick.AddListener(PauseGame);
        _resumeButton.onClick.AddListener(ResumeGame);
        _mainMenuButton.onClick.AddListener(MainMenu);
        _soundButton.onClick.AddListener(SoundSet);
        _mainmenu2Button.onClick.AddListener(MainMenu);
        _restartButton.onClick.AddListener(RestartScene);
        Time.timeScale = 1f;
        _isGamePaused = false;
        _isSoundPaused = false;
        Instance = this;
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (PlayerController.instance._isAlive == false)
        {
            Invoke(nameof(Death),1.1f);
        }
        if (_isGamePaused)
        {
            Cursor.visible = true;
           Cursor.lockState = CursorLockMode.None;
        }
    }
    private void PauseGame()
    {
        Time.timeScale = 0f;
        _isGamePaused = true;
        _pausePanel.SetActive(true);
        _pauseText.text = "PAUSE";
        AudioListener.pause = true;
    }
    private void ResumeGame()
    {
        Time.timeScale = 1f;
        _isGamePaused = false;
        _pausePanel.SetActive(false);
        AudioListener.pause = false;
    }
    private void MainMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }
    private void TogglePause()
    {
        if (_isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    private void SoundSet()
    {
        AudioListener.pause = !_isSoundPaused;
        _isSoundPaused = !_isSoundPaused;
    }
    private void RestartScene()
    {
        SceneManager.LoadSceneAsync(1);
    }
    private void Death()
    {
        _deathPanel?.SetActive(true);
        Time.timeScale = 0f;
        _isGamePaused = true;
        _deathText.text = "GAME OVER";
    }
}