using TMPro;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI scoreInfoTxt;
    [SerializeField] private TextMeshProUGUI highscoreInfoTxt;

    [SerializeField] private Animator newHighscoreAnimator;
    [SerializeField] private Animator addScoreAnimator;

    private int curScore;
    private bool gameOver;

    private Vector3 lastPipePos;


    private void OnEnable()
    {
        BirdController.OnGameOver += OnGameOver;
    }
    private void OnDisable()
    {
        BirdController.OnGameOver -= OnGameOver;
    }

    private void Update()
    {
        if (gameOver || !GameController.hasStarted) return;

        foreach (var pipe in WorldGenerator.pipes)
        {
            var pos = pipe.transform.position;

            if (pos.x <= lastPipePos.x) continue;

            if (pos.x < Camera.main.transform.position.x)
            {
                OnScoreAdd();
                lastPipePos = pos;
                break;
            }
        }
    }

    private void OnGameOver()
    {
        gameOver = true;
        scoreInfoTxt.text = $"Score {curScore}";

        int prefsHighScore = SaveManager.Load<int>("Highscore");
        int highScore = prefsHighScore > curScore ? prefsHighScore : curScore;
        SaveManager.Save("Highscore", highScore);

        newHighscoreAnimator.SetBool("NewHighscore", highScore != prefsHighScore);

        highscoreInfoTxt.text = $"Highscore {highScore}";
        scoreInfoTxt.text = $"Score {curScore}";
    }

    private void OnScoreAdd()
    {
        AudioManager.instance.pointAudioSource.Play();
        curScore++;
        scoreTxt.text = curScore.ToString();
        addScoreAnimator.SetTrigger("AddScore");
    }
}
