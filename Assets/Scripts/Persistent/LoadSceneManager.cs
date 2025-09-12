using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public const string PERSISTENT_SCENE = "Persistent";
    public const string MENU_SCENE = "Menu";
    public const string GAME_SCENE = "Game";

    public static void Init()
    {
        if (!SceneLoaded(GAME_SCENE))
        {
            LoadSceneAsyncAdditive(GAME_SCENE);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (SceneLoaded(PERSISTENT_SCENE))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(PERSISTENT_SCENE));
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void LoadGameScene_FromMenu()
    {
        UnloadSceneAsync(MENU_SCENE);
        LoadSceneAsyncAdditive(GAME_SCENE);
    }
    public static void LoadMenuScene_FromGame()
    {
        UnloadSceneAsync(GAME_SCENE);
        LoadSceneAsyncAdditive(MENU_SCENE);
    }

    public static void UnloadSceneAsync(string name)
    {
        SceneManager.UnloadSceneAsync(name);
    }

    public static void LoadSceneAsyncAdditive(string name)
    {
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
    }

    public static void Check()
    {
        if (!SceneLoaded(PERSISTENT_SCENE))
        {
            SceneManager.LoadScene(PERSISTENT_SCENE);
        }
    }

    internal static bool SceneLoaded(string name)
    {
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name == name)
            {
                return true;
            }
        }
        return false;
    }
}
