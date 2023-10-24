using UnityEngine;
using System.Collections;
using TMPro;

public class ScoreManager : Singleton<ScoreManager> {
    [SerializeField] TextMeshProUGUI ScoreText;
    public int m_currentScore = 0;
    private int m_countValue = 0;
    private int m_increment = 5;

    public void UpdateScorText(int scoreValue) {
        if(ScoreText != null) {
            ScoreText.text = scoreValue.ToString();
        }
    }
    public void AddScores(int value) {
        m_currentScore += value;
        StartCoroutine(CountScoreRoutine());
    }

    IEnumerator CountScoreRoutine() {
        int interations = 0; // 만약의 무한루프를 막음

        while(m_currentScore > m_countValue && interations < 100000) {
            m_countValue += m_increment;
            UpdateScorText(m_countValue);
            interations++;

            yield return new WaitForSeconds(0.1f);
        }
        m_countValue = m_currentScore;
        UpdateScorText(m_currentScore);
    }
}