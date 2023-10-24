using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Board : MonoBehaviour {
    [Header("Bombs")]
    [SerializeField] private GameObject adjacentBombPrefab;
    [SerializeField] private GameObject columnBombPrefab;
    [SerializeField] private GameObject rowBombPrefab;
    [SerializeField] private GameObject colorBombPrefab;

    [Header("Collectable")]
    [SerializeField] int maxCollectable = 3;
    [SerializeField] int collectableCount = 0;
    [Range(0, 1)] public float chanceForCollectable = 0.1f;
    [SerializeField] GameObject[] collectablePrefabs;

    #region Bomb
    List<GamePiece> GetRowPieces(int row) {
        List<GamePiece> gamePiece = new List<GamePiece>();

        for(int i = 0; i < width; i++) {
            if(m_allGamePieces[i, row] != null) {
                gamePiece.Add(m_allGamePieces[i, row]);
            }
        }

        return gamePiece;
    }
    List<GamePiece> GetColumnPieces(int column) {
        List<GamePiece> gamePiece = new List<GamePiece>();

        for(int i = 0; i < height; i++) {
            if(m_allGamePieces[column, i] != null) {
                gamePiece.Add(m_allGamePieces[column, i]);
            }
        }

        return gamePiece;
    }
    List<GamePiece> GetAdjacentPiece(int x, int y, int offset = 1) {
        List<GamePiece> gamePiece = new List<GamePiece>();

        for(int i = x - offset; i <= x + offset; i++) {
            for(int j = y - offset; j <= y + offset; j++) {
                if(IsWithInBounds(i, j)) {
                    if(m_allGamePieces[i, j] != null) {
                        gamePiece.Add(m_allGamePieces[i, j]);
                    }
                }
            }
        }

        return gamePiece;
    }
    List<GamePiece> GetBombedPrieces(List<GamePiece> gamePieces) {
        List<GamePiece> allPricesToClear = new List<GamePiece>();

        foreach(var piece in gamePieces) {
            if(piece != null) {
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
                            break;
                    }
                    allPricesToClear = allPricesToClear.Union(pieceToClear).ToList();
                    allPricesToClear = RemoveCollectable(allPricesToClear);
                }
            }
        }
        return allPricesToClear;
    }
    bool IsCornerMatch(List<GamePiece> gamePieces) {
        bool vertical = false;
        bool horizontal = false;

        int startX = -1;
        int startY = -1;

        foreach(GamePiece piece in gamePieces) {
            if(piece != null) {
                if(startX == -1 || startY == -1) {
                    startX = piece.xIndex;
                    startY = piece.yIndex;
                    continue;
                }
                if(piece.xIndex != startX && piece.yIndex == startY) {
                    horizontal = true;
                }
                if(piece.xIndex == startX && piece.yIndex != startY) {
                    vertical = true;
                }
            }
        }
        return (horizontal && vertical);
    }
    GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces) {
        GameObject bomb = null;
        if(gamePieces.Count >= 4) {
            if(IsCornerMatch(gamePieces)) {
                if(adjacentBombPrefab != null) {
                    bomb = MakeBomb(adjacentBombPrefab, x, y);
                }
            }
            else {
                if(gamePieces.Count >= 5) {
                    if(colorBombPrefab != null) {
                        bomb = MakeBomb(colorBombPrefab, x, y);
                    }
                }
                else {
                    if(swapDirection.x != 0) {
                        if(rowBombPrefab != null) {
                            bomb = MakeBomb(columnBombPrefab, x, y);
                        }
                    }
                    else {
                        if(columnBombPrefab != null) {
                            bomb = MakeBomb(rowBombPrefab, x, y);
                        }
                    }
                }
            }
        }

        return bomb;
    }

    private void ActivateBomb(GameObject bomb) {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if(IsWithInBounds(x, y)) {
            m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
        }
    }
    bool IsColorBomb(GamePiece gamePiece) {
        Bomb bomb = gamePiece.GetComponent<Bomb>();

        if(bomb != null) {
            return (bomb.bombType == BombType.Color);
        }

        return false;
    }
    #endregion

    #region Collectable
    List<GamePiece> FindCollectablesAt(int row, bool clearByBotton = false) {
        List<GamePiece> foundCollectables = new List<GamePiece>();

        for(int i = 0; i < width; i++) {
            if(m_allGamePieces[i, row] != null) {
                Collectable collectableComponent = m_allGamePieces[i, row].GetComponent<Collectable>();
                if(collectableComponent != null) {
                    if((clearByBotton && collectableComponent.clearByBotton) || !clearByBotton) {
                        foundCollectables.Add(m_allGamePieces[i, row]);
                    }
                }
            }
        }

        return foundCollectables;
    }
    List<GamePiece> FindAllCollectableAt() {
        List<GamePiece> foundCollectables = new List<GamePiece>();

        for(int i = 0; i < height; i++) {
            List<GamePiece> collectableRow = FindCollectablesAt(i);
            foundCollectables = foundCollectables.Union(collectableRow).ToList();
        }
        return foundCollectables;
    }
    // Union : 합집합
    // Except : 차집합
    // Intersect : 교집합
    List<GamePiece> RemoveCollectable(List<GamePiece> bombPiece) {
        List<GamePiece> collectablePieces = FindAllCollectableAt();
        List<GamePiece> pieceToRomove = new List<GamePiece>();

        foreach(GamePiece piece in collectablePieces) {
            Collectable collectableComponent = piece.GetComponent<Collectable>();
            if(collectableComponent != null && !collectableComponent.clearByBomb) {
                pieceToRomove.Add(piece);
            }
        }

        return bombPiece.Except(pieceToRomove).ToList();
    }
    bool CanAddCollectable() {
        return (Random.Range(0f, 1f) <= chanceForCollectable && collectablePrefabs.Length > 0 && collectableCount < maxCollectable); // chanceForCollectable값이 Random.Range(0f,1f)안에 있나?
    }
    #endregion
}
