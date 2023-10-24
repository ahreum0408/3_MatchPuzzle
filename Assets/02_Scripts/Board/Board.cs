using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Board : MonoBehaviour { // partial : 클래스를 분리한다
    ParticleManager particleManager;
    GamePiece[,] m_allGamePieces;

    [Header("Board Setting")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int borderSize;
    [Space]
    [SerializeField] private GameObject[] gamePiecePrefabs;

    GameObject m_clickTileBomb;
    GameObject m_targetTileBomb;
    [Space]
    [SerializeField] private float swapTime = 0.5f;
    [SerializeField] int falseYoffset = 10;
    [SerializeField] float moveTime = 0.5f;

    [Header("Starting GamePiece")]
    public bool m_playerInputEnabled = true;
    int m_scoreMultiplier = 0;
    public Transform dotsParent;
    public StartingObject[] startingGamePiece;

    [System.Serializable]
    public class StartingObject {
        public GameObject prefabs;
        public int x;
        public int y;
        public int z;
    }

    private void Start() {
        particleManager = FindObjectOfType<ParticleManager>();

        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
    }

    public void SetupBoard() {
        SetUpTiles();
        SetUpGamePiece();
        SetUpCamera();
        FillBoard(falseYoffset, moveTime);
    }

    GameObject GetRandomObject(GameObject[] objectArray) {
        int randIndex = Random.Range(0, objectArray.Length);
        if(objectArray[randIndex] == null) {
            Debug.Log("void!");
        }
        return objectArray[randIndex];
    }
    GameObject GetRandomCollectable() {
        return GetRandomObject(collectablePrefabs);
    }
    GameObject GetRandomGamePiece() {
        return GetRandomObject(gamePiecePrefabs);
    }
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y) {
        if(gamePiece == null) {
            Debug.LogWarning("waring");
            return;
        }
        gamePiece.transform.position = new Vector3(x, y, 0);
        if(IsWithInBounds(x, y)) {
            m_allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x, y);
    } // Dot의 위치를 지정해줌
    private bool IsWithInBounds(int x, int y) {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
    private void FillBoard(int falseYoffset = 0, float moveTime = 0.1f) {
        int maxInteractions = 100;
        int interactions = 0;

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                GamePiece piece = null;

                if(m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle) {
                    if(j == height - 1 && CanAddCollectable()) {
                        piece = FillRandomCollectablePieceAt(i, j, falseYoffset, moveTime);
                        collectableCount++;
                    }
                    else {
                        piece = FillRandomGamePieceAt(i, j, falseYoffset, moveTime);
                        while(HasMatchOnFill(i, j)) {
                            ClearPieceAt(i, j);
                            piece = FillRandomGamePieceAt(i, j, falseYoffset, moveTime);
                            interactions++;

                            if(interactions >= maxInteractions) {
                                break;
                            }
                        }
                    }
                }
            }
        }
    } // Dots를 생성함

    private GamePiece FillRandomGamePieceAt(int i, int j, int falseYoffset = 0, float moveTime = 0.1f) { // piece를 생성
        if(!IsWithInBounds(i, j)) {
            return null;
        }

        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity, dotsParent);
        if(randomPiece != null) {
            MakeGamePiece(randomPiece, i, j, falseYoffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }
    private GamePiece FillRandomCollectablePieceAt(int i, int j, int falseYoffset = 0, float moveTime = 0.1f) {
        if(!IsWithInBounds(i, j)) {
            return null;
        }

        GameObject randomPiece = Instantiate(GetRandomCollectable(), Vector3.zero, Quaternion.identity, dotsParent);
        if(randomPiece != null) {
            MakeGamePiece(randomPiece, i, j, falseYoffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }
    private void ReplaceWithRandom(List<GamePiece> gamePieces, int x, int y) {
        foreach(GamePiece piece in gamePieces) {
            ClearPieceAt(piece.xIndex, piece.yIndex);
            if(falseYoffset == 0) {
                FillRandomGamePieceAt(piece.xIndex, piece.yIndex);
            }
            else {
                FillRandomGamePieceAt(piece.xIndex, piece.yIndex, falseYoffset, moveTime);
            }
        }
    }
    private void MakeGamePiece(GameObject piece, int i, int j, int falseYoffset = 0, float moveTime = 0.1f) {
        if(piece != null && IsWithInBounds(i, j)) {
            piece.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(piece.GetComponent<GamePiece>(), i, j);

            if(falseYoffset != 0) { // 내려오는 중이다
                piece.transform.position = new Vector3(i, j + falseYoffset, 0);
                piece.GetComponent<GamePiece>().Move(i, j, moveTime);
            }
        }

        piece.transform.parent = transform;
    }
    private GameObject MakeBomb(GameObject piece, int i, int j) {
        if(piece != null && IsWithInBounds(i, j)) {
            GameObject bomb = Instantiate(piece, new Vector3(i, j, 0), Quaternion.identity);
            bomb.GetComponent<Bomb>().Init(this);
            bomb.GetComponent<Bomb>().SetCoord(i, j);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3) { // 처음 시작 될 떄 매치가 있냐?
        // match 있음 true. 없어 false;
        List<GamePiece> downPiece = FindVerticalMatches(x, y);
        List<GamePiece> liftPiece = FindHorizontalMatches(x, y);
        if(downPiece == null) {
            downPiece = new List<GamePiece>();
        }
        if(liftPiece == null) {
            liftPiece = new List<GamePiece>();
        }

        return liftPiece.Count > 0 || downPiece.Count > 0;
    }
    private void SetUpGamePiece() {
        foreach(StartingObject sPiece in startingGamePiece) {
            if(sPiece != null) {
                GameObject piece = Instantiate(sPiece.prefabs, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity);
                MakeGamePiece(piece, sPiece.x, sPiece.y, falseYoffset, moveTime);
            }
        }
    }
    private void SetUpCamera() {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2, (float)(height - 1) / 2, -10f);

        float aspecRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)height / 2f + (float)borderSize;
        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspecRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize; // 둘중에 더 큰애로 사이즈를 설정하겠다
    }// 타일에 따른 카메라 사이즈 및 위치
    private void SetUpTiles() {
        foreach(var sTiles in startingTiles) {
            if(sTiles != null) {
                MakeTile(sTiles.prefabs, sTiles.x, sTiles.y, sTiles.z);
            }
        }

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(m_allTiles[i, j] == null) {
                    MakeTile(tileNormalPrefabs, i, j);
                }
            }
        }
    } // 타일배치

    private void ClearPieceAt(int x, int y) {
        GamePiece pieceToClear = m_allGamePieces[x, y];
        if(pieceToClear != null) {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        //HighlightTilesOff(x, y);
    }
    private void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombPieces) { // gamePiece : 없앨 모든 piece를 다 들고옴 // bombPieces : 에 있는 애만 bombFX를 불러줌
        int bonus = 0;

        foreach(var piece in gamePieces) {
            if(piece != null) {
                ClearPieceAt(piece.xIndex, piece.yIndex);
                if(gamePieces.Count >= 4) {
                    bonus = 20;
                }
                piece.ScorePoints(bonus, m_scoreMultiplier);

                if(particleManager != null) {
                    if(bombPieces.Contains(piece)) {
                        particleManager.BombClearFXAt(piece.xIndex, piece.yIndex);
                    }
                    else {
                        particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex, 0); // 기본 particle
                    }
                }
            }
        }
    }
    private void ClearBoard() {
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                ClearPieceAt(i, j);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f) {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for(int i = 0; i < height - 1; i++) {
            if(m_allGamePieces[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle) {
                for(int j = i + 1; j < height; j++) {
                    if(m_allGamePieces[column, j] != null) {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

                        if(!movingPieces.Contains(m_allGamePieces[column, i])) {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }
                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }
    List<int> GetColumns(List<GamePiece> gamePieces) {
        List<int> columns = new List<int>();
        foreach(GamePiece piece in gamePieces) {
            if(!columns.Contains(piece.xIndex)) {
                columns.Add(piece.xIndex);
            }
        }
        return columns;
    }
    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces) {
        List<GamePiece> movingPiece = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach(int column in columnsToCollapse) {
            movingPiece = movingPiece.Union(CollapseColumn(column)).ToList();
        }
        return movingPiece;
    }


    void ClearAndRefillBoard(List<GamePiece> gamePieces) {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }
    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces) {
        m_playerInputEnabled = false;
        List<GamePiece> maches = gamePieces;
        m_scoreMultiplier = 0;

        do {
            // clear and collapse
            m_scoreMultiplier++;
            yield return StartCoroutine(ClearAndCollapseRoutine(maches));
            yield return null;

            // refill
            yield return StartCoroutine(RefillRoutine());
            maches = FindAllMatches();
            yield return new WaitForSeconds(0.5f);

        } while(maches.Count != 0);

        m_playerInputEnabled = true;
    }
    IEnumerator RefillRoutine() {
        FillBoard(falseYoffset, moveTime);

        yield return null;
    }
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces) {
        List<GamePiece> movingPiece = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlihtMatchesAt(gamePieces);

        yield return new WaitForSeconds(0.25f);
        bool isFinished = false;

        while(!isFinished) {
            List<GamePiece> bombPieces = GetBombedPrieces(gamePieces);
            gamePieces = gamePieces.Union(bombPieces).ToList();

            yield return new WaitForSeconds(0.2f);

            bombPieces = GetBombedPrieces(gamePieces); // bomb으로 없어지는 piece
            gamePieces = gamePieces.Union(bombPieces).ToList(); // 없어지는 일반 piece

            List<GamePiece> collectablePieces = FindCollectablesAt(0, true);
            List<GamePiece> allCollectablePieces = FindAllCollectableAt();
            List<GamePiece> bolockers = gamePieces.Intersect(allCollectablePieces).ToList();

            collectablePieces = collectablePieces.Union(bolockers).ToList();
            collectableCount -= collectablePieces.Count;

            gamePieces = gamePieces.Union(collectablePieces).ToList();

            // 삭제할 목록 추가하기
            ClearPieceAt(gamePieces, bombPieces);
            BreakTileAt(gamePieces);


            yield return new WaitForSeconds(0.25f);

            if(m_clickTileBomb != null) {
                ActivateBomb(m_clickTileBomb);
                m_clickTileBomb = null;
            }
            if(m_targetTileBomb != null) {
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(0.2f);

            movingPiece = CollapseColumn(gamePieces);

            while(!IsCollapsed(movingPiece)) {
                yield return null;
            }

            yield return new WaitForSeconds(0.25f);

            collectablePieces = FindCollectablesAt(0, true);

            matches = FindMatchesAt(movingPiece);
            matches = matches.Union(collectablePieces).ToList();

            if(matches.Count == 0) {
                isFinished = true;
                break;
            }
            else {
                m_scoreMultiplier++;
                if(SoundManager.Instance != null) {
                    SoundManager.Instance.PlayBounsSound();
                }
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    private bool IsCollapsed(List<GamePiece> gamePieces) {
        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                if(piece.transform.position.y - (float)piece.yIndex > 0.001f) {
                    return false;
                }
            }
        }
        return true;
    }
}