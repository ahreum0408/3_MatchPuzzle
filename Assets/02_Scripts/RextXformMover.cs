using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RextXformMover : MonoBehaviour {
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 onScreenPos;
    [SerializeField] private Vector3 endPos;
    [SerializeField] private float timeToMove = 1f;

    private bool isMoving=false;
    RectTransform m_rectTransform;
    private void Awake() {
        isMoving = false;
        m_rectTransform = GetComponent<RectTransform>();
        
    }
    private void Move(Vector3 startPos, Vector3 endPos, float timeToMove) {
        if(!isMoving) {
            StartCoroutine(MoveRoutine(startPos, endPos, timeToMove));
        }
    }

    IEnumerator MoveRoutine(Vector3 startPos, Vector3 endPos, float timeToMove) {
        if(m_rectTransform != null) {
            m_rectTransform.anchoredPosition = startPos;
        }
        bool reachedDestination = false;
        float elapsedTime = 0f;

        isMoving = true;
        while(!reachedDestination) {
            if(Vector3.Distance(m_rectTransform.anchoredPosition, endPos) < 0.01f) {
                reachedDestination = true;
                break;
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            t = t * t * t * (t * (t * 6 - 15) + 10);

            if(m_rectTransform != null) {
                m_rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            }
            yield return null;
        }
        isMoving = false;
    }
    public void MoveOn() { // 중앙으로
        Move(startPos, onScreenPos, timeToMove);
    }
    public void MoveOff() { // 밖으로
        Move(onScreenPos, endPos, timeToMove);
    }
}
