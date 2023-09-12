using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour {
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int borderSize;

    [SerializeField] private GameObject tileNormalPrefabs;
    [SerializeField] private GameObject tileNObstclePrefabs;

    [SerializeField] private GameObject[] gamePiecePrefabs;

    [SerializeField] private float swapTime = 0.5f;

    public Transform dotsParent;

    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    Tile m_clickedTile;
    Tile m_targetTile;

    ParticleManager particleManager;

    bool m_playerInputEnabled = true;
    int falseYoffset = 10;
    float moveTime = 0.5f;

    public StartingObject[] startingTiles;
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

        SetUpTiles();
        SetUpGamePiece();
        SetUpCamera();
        FillBoard(falseYoffset, moveTime);
    }
    GameObject GetRandomGamePiece() {
        int rand = Random.Range(0, gamePiecePrefabs.Length);

        if(gamePiecePrefabs[rand] == null) {
            Debug.LogWarning("warring");
        }
        return gamePiecePrefabs[rand];
    } // 랜덤한 Dots를 얻음
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
                if(m_allGamePieces[i,j] == null && m_allTiles[i,j].tileType != TileType.Obstacle) {
                   GamePiece piece  = FillRandomAt(i, j, falseYoffset, moveTime);
                    while(HasMatchOnFill(i, j)) {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j, falseYoffset, moveTime);
                        interactions++;

                        if(interactions >= maxInteractions) {
                            break;
                        }
                    }
                }
            }
        }
    } // Dots를 생성함

    private GamePiece FillRandomAt(int i, int j, int falseYoffset = 0, float moveTime = 0.1f) {
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
    private void ReplaceWithRandom(List<GamePiece> gamePieces, int x, int y) {
        foreach(GamePiece piece in gamePieces) {
            ClearPieceAt(piece.xIndex, piece.yIndex);
            if(falseYoffset == 0) {
                FillRandomAt(piece.xIndex, piece.yIndex);
            }
            else {
                FillRandomAt(piece.xIndex, piece.yIndex, falseYoffset, moveTime);
            }
        }
    }
    private void MakeGamePiece(GameObject piece, int i, int j, int falseYoffset =0, float moveTime = 0.1f) {
        piece.GetComponent<GamePiece>().Init(this);
        PlaceGamePiece(piece.GetComponent<GamePiece>(), i, j);

        if(falseYoffset != 0) { // 내려오는 중이다
            piece.transform.position = new Vector3(i, j + falseYoffset, 0);
            piece.GetComponent<GamePiece>().Move(i, j, moveTime);
        }

        piece.transform.parent = transform;
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3) { // 처음 시작 될 떄 매치가 있냐?
        // match 있음 true. 없어 false;
        List<GamePiece> downPiece = FindMatches(x, y, new Vector2(0, -1));
        List<GamePiece> liftPiece = FindMatches(x, y, new Vector2(-1, 0));
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
                if(m_allTiles[i,j] == null) {
                    MakeTile(tileNormalPrefabs, i, j);
                }
            }
        }
    } // 타일배치
    private void MakeTile(GameObject prefabs, int i, int j, int z = 0) {
        if(prefabs == null) {
            return;
        }
        GameObject tile = Instantiate(prefabs, new Vector3(i, j), Quaternion.identity, transform); // (,,,transform) : 부모 오브젝트 지정
        tile.name = $"Tile ({i}, {j})";
        m_allTiles[i, j] = tile.GetComponent<Tile>();
        m_allTiles[i, j].Init(i, j, this);
    }


    public void ClickTile(Tile tile) {
        if(m_clickedTile == null) {
            m_clickedTile = tile;
        }
    }
    public void DragToTile(Tile tile) {
        if(m_clickedTile != null) {
            m_targetTile = tile;
        }
    }
    public void ReleaseTile() {
        if(m_clickedTile != null && m_targetTile != null && IsNextTo(m_clickedTile, m_targetTile)) {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }
    private void SwitchTiles(Tile clickedTile, Tile targetTile) {
        StartCoroutine(SwitchTileRoutine(clickedTile, targetTile));
    }
    IEnumerator SwitchTileRoutine(Tile clickedTile, Tile targetTile) {
        if(m_playerInputEnabled) {
            // 타일 교환
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

            // 교환
            if(clickedTile != null && targetPiece != null) {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, 0.5f);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, 0.5f);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

                if(clickPieceMatches.Count == 0 && targetPieceMatches.Count == 0) {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, 0.5f);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, 0.5f);
                }
                else {
                    yield return new WaitForSeconds(swapTime);
                    ClearAndRefillBoard(clickPieceMatches.Union(targetPieceMatches).ToList());
                }
            }
        }
    }
    private bool IsNextTo(Tile start, Tile end) {
        /*if(Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex) {
            return true;
        }
        if(start.xIndex == end.xIndex && Mathf.Abs(start.yIndex - end.yIndex) == 1) {
            return false;
        }*/

        float distance = Vector3.Distance(start.transform.position, end.transform.position);
        if(distance <= 1) {
            return true;
        }
        else {
            return false;
        }
    }


    // 매치된 타일 확인
    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3){
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if(IsWithInBounds(startX, startY)) {
            startPiece = m_allGamePieces[startX, startY];
        }

        if(startPiece != null) {
            matches.Add(startPiece);
        }
        else {
            return null;
        }

        int nextX, nextY;
        int maxValue = (width > height) ? width : height;

        for(int i = 1; i < maxValue -1; i++) {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if(!IsWithInBounds(nextX, nextY)) {
                break;
            }
            GamePiece nextPiece = m_allGamePieces[nextX, nextY];
            if(nextPiece == null) {
                break;
            }
            else {
                if(nextPiece.matchValue == startPiece.matchValue) {
                    matches.Add(nextPiece);
                }
                else {
                    break;
                }
            }
        }
        if(matches.Count >= minLength) {
            return matches;
        }
        return null;    
    } 
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenth = 3) {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if(upwardMatches == null) {
            upwardMatches = new List<GamePiece>();
        }
        if(downwardMatches == null) {
            downwardMatches = new List<GamePiece>();
        }

        foreach(GamePiece piece in downwardMatches) {
            if(!upwardMatches.Contains(piece)) {
                upwardMatches.Add(piece);
            }
        }
        var comdinedMatches = upwardMatches.Union(downwardMatches).ToList(); // 합집합 : Union

        return (upwardMatches.Count >= minLenth) ? upwardMatches : null;
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenth = 3) {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if(rightMatches == null) {
            rightMatches = new List<GamePiece>();
        }
        if(leftMatches == null) {
            leftMatches = new List<GamePiece>();
        }

        foreach(GamePiece piece in leftMatches) {
            if(!rightMatches.Contains(piece)) {
                rightMatches.Add(piece);
            }
        }
        var comdinedMatches = rightMatches.Union(leftMatches).ToList();

        return (rightMatches.Count >= minLenth) ? rightMatches : null;
    }
    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3) {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, 3);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, 3);

        if(horizMatches == null) {
            horizMatches = new List<GamePiece>();
        }
        if(vertMatches == null) {
            vertMatches = new List<GamePiece>();
        }

        var combineMatches = horizMatches.Union(vertMatches).ToList();
        return combineMatches;
    }
    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLegth = 3) {
        List<GamePiece> matches = new List<GamePiece>();
        // match된 list구하기
        foreach(var piece in gamePieces) {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLegth)).ToList();
        }
        return matches;
    }
    List<GamePiece> FindAllMatches() {
        List<GamePiece> combinMatches = new List<GamePiece>();

        for(int i = 0; i < width; i++) {
            for(int j =0; j < height; j++) {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinMatches = combinMatches.Union(matches).ToList();
            }
        }
        return combinMatches;
    }


    // 하이라이트
    private void HighlightTilesOff(int x, int y) {
        if(m_allTiles[x, y].tileType != TileType.breakable) {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    } 
    private void HighlightTilesOn(int x, int y, Color col) {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }
    private void HighlihtMatchesAt(int x, int y) {
        HighlightTilesOff(x, y);
        var combineMatches = FindMatchesAt(x, y);
        if(combineMatches.Count > 0) {
            foreach(GamePiece piece in combineMatches) {
                HighlightTilesOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    private void HighlihtMatchesAt(List<GamePiece> gamePieces) { 
        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                HighlihtMatchesAt(piece.xIndex, piece.yIndex);
            }
        }
    }
    private void HighlightMatches() {
        for(int i = 0; i < width; i++) {
            for(int j  = 0; j < height; j++) {
                HighlihtMatchesAt(i, j);
                
            }
        }
    }


    private void ClearPieceAt(int x, int y) {
        GamePiece pieceToClear = m_allGamePieces[x, y];
        if(pieceToClear != null) {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        //HighlightTilesOff(x, y);
    }
    private void ClearPieceAt(List<GamePiece> gamePiece) {
        foreach(GamePiece piece in gamePiece) {
            if(piece != null) {
                ClearPieceAt(piece.xIndex, piece.yIndex);
                if(particleManager != null) {
                    particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex, 0);
                }
            }
        }
    }
    private void BreakTileAt(int x, int y) {
        Tile tileToBreak = m_allTiles[x, y];

        if(tileToBreak != null && tileToBreak.tileType == TileType.breakable) {
            if(particleManager != null) {
                particleManager.BreakTileFXAt(tileToBreak.breakbleValse,x, y, 0);
            }
            tileToBreak.BreakTile();
        }
    }
    private void BreakTileAt(List<GamePiece> gamePieces) {
        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                BreakTileAt(piece.xIndex, piece.yIndex);
                /*if(m_allTiles[piece.xIndex, piece.yIndex].tileType == TileType.breakable) {
                    m_allTiles[piece.xIndex, piece.yIndex].BreakTile();
                }*/
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
            if(m_allGamePieces[column, i] == null && m_allTiles[column,i].tileType != TileType.Obstacle) {
                for(int j = i + 1;  j < height; j++) {
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
    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)  {
        m_playerInputEnabled = false;
        List<GamePiece> maches = gamePieces;

        do {
            // clear and collapse
            yield return StartCoroutine(ClearAndCollapseRoutine(maches));
            yield return null;

            // refill
            yield return StartCoroutine(RefillRoutine());
            maches = FindAllMatches();
            yield return new WaitForSeconds(0.5f);

        } while(maches.Count != 0);

        m_playerInputEnabled = true;
    }
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces) {
        List<GamePiece> movingPiece = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlihtMatchesAt(gamePieces);

        yield return new WaitForSeconds(0.25f);
        bool isFinished = false;

        while(!isFinished){
            GetBombedPrieces(gamePieces);
            ClearPieceAt(gamePieces);
            BreakTileAt(gamePieces);
            yield return new WaitForSeconds(0.25f);

            movingPiece = CollapseColumn(gamePieces);

            while(!IsCollapsed(movingPiece)) {
                yield return null;
            }

            yield return new WaitForSeconds(0.25f);

            matches = FindMatchesAt(movingPiece);
            if(matches.Count == 0) {
                isFinished = true;
                break;
            }
            else {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }


    IEnumerator RefillRoutine() {
        FillBoard(falseYoffset, moveTime);

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


    List<GamePiece> GetRowPieces(int row) {
        List<GamePiece> gamePiece = new List<GamePiece>();

        for(int i =0; i < row; i++) {
            gamePiece.Add(m_allGamePieces[i, row]);
        }

        return gamePiece;
    }
    List<GamePiece> GetColumnPieces(int column) {
        List<GamePiece> gamePiece = new List<GamePiece>();

        for(int i = 0; i < column; i++) {
            gamePiece.Add(m_allGamePieces[column, i]);
        }

        return gamePiece;
    }
    List<GamePiece> GetAdjacentPiece(int x, int y, int offset = 1) {
        List<GamePiece> gamePiece = new List<GamePiece>();


        return gamePiece;
    }
    List<GamePiece> GetBombedPrieces(List<GamePiece> gamePieces) {
        List<GamePiece> allPricesToClear = new List<GamePiece>();

        foreach(var piece in gamePieces) {
            if(piece!= null) {
                List<GamePiece> pieceToClear = new List<GamePiece>();

                Bomb bomb = piece.GetComponent<Bomb>();
                if(bomb != null) {
                    switch(bomb.bombType) {
                        case BombType.Column:
                            pieceToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            pieceToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            pieceToClear = GetAdjacentPiece(bomb.xIndex, bomb.yIndex);
                            break;
                        case BombType.Color:
                            pieceToClear = GetColumnPieces(bomb.xIndex);
                            break;
                    }
                    allPricesToClear = allPricesToClear.Union(pieceToClear).ToList();
                }
            }
        }
        return allPricesToClear;
    }
}