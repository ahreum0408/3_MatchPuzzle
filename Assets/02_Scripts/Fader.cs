using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour {
    [SerializeField] private Image fadeImage;
    [SerializeField] private float clearAlpha = 0;
    [SerializeField] private float solidAlpha = 1;
    [SerializeField] private float fadeTime = 1;

    public void FadeOn() {
        StartCoroutine(FadeRoutine(clearAlpha, solidAlpha, fadeTime));
    }
    public void FadeOff() {
        StartCoroutine(FadeRoutine(solidAlpha, clearAlpha, fadeTime));
    }
    IEnumerator FadeRoutine(float startValue, float endValue, float t) {
        float currentTime = 0; 

        while(currentTime < t) {
            currentTime += Time.deltaTime;

            var currentValue = Mathf.Lerp(startValue, endValue, currentTime / t); // Mathf.Lerp(시작값, 끝나는값, 걸리는 시간);
            fadeImage.color = new Color(0, 0, 0, currentValue);

            yield return null;
        }
    }
}
