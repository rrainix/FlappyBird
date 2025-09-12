using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static bool hasStarted;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private GameObject mainUI;

    public static Action OnGameStart;
    public static Action OnRetry;

    private void OnEnable()
    {
        BirdController.OnGameOver += OnDeath;
    }

    private void OnDisable()
    {
        BirdController.OnGameOver -= OnDeath;
    }

    private void Awake()
    {
        hasStarted = false;
        LoadSceneManager.Check();
        startScreen.SetActive(true);
    }

    private void Update()
    {
        if (!hasStarted &&Input.anyKeyDown)
        {
            AudioManager.PlayClick();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            hasStarted = true;
            OnGameStart?.Invoke();
            startScreen.SetActive(false);
            mainUI.SetActive(true);
        }
    }

    public void OnDeath()
    {
        gameoverScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClickRetry()
    {
        Retry();
    }

    private async void Retry()
    {
        await SceneManager.UnloadSceneAsync(LoadSceneManager.GAME_SCENE);
        hasStarted = false;
        await SceneManager.LoadSceneAsync(LoadSceneManager.GAME_SCENE, LoadSceneMode.Additive);
        WorldGenerator.instance.OnRetry();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
    public void OnClickMenu()
    {
        LoadSceneManager.LoadMenuScene_FromGame();
    }
}
