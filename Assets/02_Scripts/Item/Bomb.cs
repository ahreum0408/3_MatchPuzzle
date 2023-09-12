using UnityEngine;

public enum BombType {
    None,
    Column, // 세로
    Row, // 가로
    Adjacent,
    Color
}

public class Bomb : GamePiece {
    public BombType bombType; // commit
}
