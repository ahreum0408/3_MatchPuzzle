using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Board : MonoBehaviour {
    #region FindMatches
    // 매치된 타일 확인
    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3) {
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

        for(int i = 1; i < maxValue - 1; i++) {
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
                if(nextPiece.matchValue == startPiece.matchValue && nextPiece.matchValue != MatchValue.None && !matches.Contains(nextPiece)) {
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
    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLegth = 3) { // 매치되었나?
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
            for(int j = 0; j < height; j++) {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinMatches = combinMatches.Union(matches).ToList();
            }
        }
        return combinMatches;
    }
    List<GamePiece> FindAllMatchValue(MatchValue matchValue) {
        List<GamePiece> colorMatches = new List<GamePiece>();
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(m_allGamePieces[i, j] != null) {
                    if(m_allGamePieces[i, j].matchValue == matchValue) {
                        colorMatches.Add(m_allGamePieces[i, j]);
                    }
                }
            }
        }

        return colorMatches;
    }
    #endregion

    #region Highlight
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
            for(int j = 0; j < height; j++) {
                HighlihtMatchesAt(i, j);

            }
        }
    }
    #endregion
}
