using System.Collections;
using UnityEngine;

public enum InterpType {
    Linear,
    EsaeOut,
    EaseIn,
    SmoothStep,
    SmootherStep
};
public enum MatchValue {
    Blue,
    Magenta,
    Indigo,
    Green,
    Teal,
    Red,
    Cyan,
    Yellow,
    WildKey
};
public class GamePiece : MonoBehaviour {
    public int xIndex;
    public int yIndex;

    Board m_board;

    public InterpType interploation = InterpType.SmootherStep;
    public MatchValue matchValue;

    private bool isMoveing = false;

    public void Init(Board board) {
        m_board = board;
    }
    public void SetCoord(int x, int y) {
        xIndex = x;
        yIndex = y;
    }
    private void Update() {
        if(Input.GetKeyDown(KeyCode.W)) {
            Move((int)transform.position.x, (int)transform.position.y + 1, 0.5f);
        }
        else if(Input.GetKeyDown(KeyCode.A)) {
            Move((int)transform.position.x - 1, (int)transform.position.y, 0.5f);
        }
        else if(Input.GetKeyDown(KeyCode.S)) {
            Move((int)transform.position.x, (int)transform.position.y - 1, 0.5f);
        }
        else if(Input.GetKeyDown(KeyCode.D)) {
            Move((int)transform.position.x + 1, (int)transform.position.y, 0.5f);
        }
    }
    public void Move(int destX, int destY, float timeToMove) {
        if(!isMoveing)
            StartCoroutine(MovePoutine(new Vector3(destX, destY, 0), timeToMove));
    }
    IEnumerator MovePoutine(Vector3 destination, float timeToMove) {
        Vector3 startPosition = transform.position;
        bool reachedDestination = false;
        float elapsedTime = 0f;
        isMoveing = true;
        while(!reachedDestination) {
            if(Vector3.Distance(transform.position, destination) < 0.01f) {
                reachedDestination = true;
                if(m_board != null) {
                    m_board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                }
                break;
            }
            elapsedTime += Time.deltaTime; // 현재시간을 측정
            float t = elapsedTime / timeToMove;
            switch(interploation) { // 움직이는 속도를 조절
                case InterpType.Linear:
                    break;

                case InterpType.EsaeOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;

                case InterpType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;

                case InterpType.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;

                case InterpType.SmootherStep:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;
            }
            transform.position = Vector3.Lerp(startPosition, destination, t);

            yield return null;
        }
        isMoveing = false;
    }
}
