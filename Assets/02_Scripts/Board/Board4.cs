using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Board : MonoBehaviour {

    [Header("Prefabs")]
    [SerializeField] private GameObject tileNormalPrefabs;
    [SerializeField] private GameObject tileNObstclePrefabs;

    public StartingObject[] startingTiles;

    Tile[,] m_allTiles;

    Tile m_clickedTile;
    Tile m_targetTile;


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
            if(clickedTile != null && targetPiece != null) { // clickedPiece.matchValue != MatchValue.None
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, 0.5f);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, 0.5f);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                List<GamePiece> colorMatches = new List<GamePiece>();

                if(IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece)) {
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorMatches = FindAllMatchValue(clickedPiece.matchValue);
                }
                else if(!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece)) {
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                }
                else if(IsColorBomb(clickedPiece) && IsColorBomb(targetPiece)) {
                    foreach(GamePiece piece in m_allGamePieces) {
                        if(!colorMatches.Contains(piece)) {
                            colorMatches.Add(piece);
                        }
                    }
                }

                if(clickPieceMatches.Count == 0 && targetPieceMatches.Count == 0 && colorMatches.Count == 0) { // 매치된 값이 없을 떄
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, 0.5f);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, 0.5f);
                }
                else {
                    GameManager.Instance.DiminishScore();
                    yield return new WaitForSeconds(swapTime);

                    Vector2 swipDiection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);
                    m_clickTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swipDiection, clickPieceMatches);
                    m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swipDiection, targetPieceMatches);

                    if(m_clickTileBomb != null && targetPiece != null) {
                        GamePiece clickedBombPiece = m_clickTileBomb.GetComponent<GamePiece>();
                        if(!IsColorBomb(clickedBombPiece)) {
                            clickedBombPiece.ChangeColor(targetPiece);
                        }
                    }
                    if(m_targetTileBomb != null && clickedPiece != null) {
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                        if(!IsColorBomb(targetBombPiece)) {
                            targetBombPiece.ChangeColor(clickedPiece);
                        }
                    }

                    ClearAndRefillBoard(clickPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList());
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
    private void BreakTileAt(int x, int y) {
        Tile tileToBreak = m_allTiles[x, y];

        if(tileToBreak != null && tileToBreak.tileType == TileType.breakable) {
            if(particleManager != null) {
                particleManager.BreakTileFXAt(tileToBreak.breakbleValse, x, y, 0); // 아이템 particle
            }
            tileToBreak.BreakTile();
        }
    }
    private void BreakTileAt(List<GamePiece> gamePieces) {
        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }
}
