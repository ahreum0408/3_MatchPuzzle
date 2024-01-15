using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : Singleton<GameManager> {
    public int moveLeft = 30; // 움직일 수 있는 횟수
    public int scoreGoal = 100000; // 골 점수
    [SerializeField] Fader screenFader;
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI moveLeftCount;
    [Space]
    [SerializeField] MessageWindow messageWindow;
    [SerializeField] Sprite loseIcon;
    [SerializeField] Sprite winIcon;
    [SerializeField] Sprite goalIcon;

    Board m_board;

    private bool m_isRealyToBegin;
    private bool m_isGameOver;
    private bool m_isWinner;
    private bool m_isReadyToReload;

    protected override void Awake() {
        base.Awake();
        SoundManager.Instance.Init();
        UpdateScoreText();

        m_board = FindObjectOfType<Board>();

        Scene scene = SceneManager.GetActiveScene();

        if(levelNameText != null) {
            levelNameText.text = scene.name;
        }
        StartCoroutine(ExeuteGameLoop());
    }

    IEnumerator ExeuteGameLoop() {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("EndGameRoutine");
    }
    public void BeginGame() {
        m_isRealyToBegin = true;
    }
    IEnumerator StartGameRoutine() {
        if(messageWindow != null) {
            messageWindow.GetComponent<RextXformMover>().MoveOn();
            messageWindow.ShowMeaagse(goalIcon, $"SCORE GOAL{scoreGoal}", "START");
        }
        while(!m_isRealyToBegin) {
            yield return null;
        }
        if(screenFader != null) {
            screenFader.FadeOff();
        }
        yield return new WaitForSeconds(0.8f);
        if(m_board != null) {
            m_board.SetupBoard();
        }
        m_isReadyToReload = false;
    }
    IEnumerator PlayGameRoutine() {
        while(!m_isGameOver) {
            yield return null;
            if(ScoreManager.Instance != null) {
                if(ScoreManager.Instance.m_currentScore > scoreGoal) {
                    m_isWinner = true;
                    m_isGameOver = true;
                }
            }
            if(moveLeft <= 0 && m_board.m_playerInputEnabled) {
                m_isWinner = false;
                m_isGameOver = true;
            }
        }
    }
    IEnumerator EndGameRoutine() {
        m_isReadyToReload = false;
        if(m_isWinner) {
            if(messageWindow != null) {
                messageWindow.ShowMeaagse(winIcon, "You Win!", "OK");
            }
            if(SoundManager.Instance != null) {
                SoundManager.Instance.Play("GameOver/Win");
            }
        }
        else {
            if(messageWindow != null) {
                messageWindow.ShowMeaagse(loseIcon, "You Lose..", "OK");
            }
            if(SoundManager.Instance != null) {
                SoundManager.Instance.Play("GameOver/Lose");
            }
        }
        messageWindow.GetComponent<RextXformMover>().MoveOn();
        if(screenFader != null) {
            screenFader.FadeOn();
        }
        while(!m_isReadyToReload) {
            yield return null;
        }
        SceneManager.LoadScene(0);
    }
    public void LoadScene() {
        m_isReadyToReload = true;
    }
    public void DiminishScore() {
        moveLeft -= 1;
        UpdateScoreText();
    }
    private void UpdateScoreText() {
        moveLeftCount.text = moveLeft.ToString();
    }
}
