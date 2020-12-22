using UnityEngine.UI;
using UnityEngine;

public class Ui : MonoBehaviour
{
    public static Ui instance;
    public Text textScore, textMove;
    private int score=0;
    private int moves=20;

    public GameObject PanelGameOver;
    public Text gTextScore, gTextBestScore;
    private void Awake()
    {
        instance = this;
    }
    public void Score(int value)
    {
        score += value;
        textScore.text = "Score: " + score.ToString();

    }
    public void Move( int value)
    {
        moves -= value;
        if (moves<=0)
        {
            GameOver();
        }
        textMove.text = "Move: " + moves.ToString();
    }
    private void GameOver()
    {
        if (score>PlayerPrefs.GetInt("Score"))
        {
            PlayerPrefs.SetInt("Score", score);
            gTextBestScore.text = "New Best: " + score.ToString();

        }
        else
        {
            gTextBestScore.text = "Best: " + PlayerPrefs.GetInt("Score");
        }
        gTextScore.text = "Score: " + score.ToString();
        PanelGameOver.SetActive(true);
    }
}
