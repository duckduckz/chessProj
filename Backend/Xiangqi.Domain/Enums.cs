namespace Xiangqi.Domain;

public enum Side : byte { Red = 0, Black = 1 }

public enum Piece : byte
{
    Empty = 0,
    RedRook, RedHorse, RedElephant, RedAdvisor, RedKing, RedCannon, RedPawn,
    BlackRook, BlackHorse, BlackElephant, BlackAdvisor, BlackKing, BlackCannon, BlackPawn
}

public static class PieceExt
{
    public static bool IsEmpty(this Piece p) => p == Piece.Empty;
    public static bool IsRed(this Piece p) => p >= Piece.RedRook && p <= Piece.RedPawn;
    public static bool IsBlack(this Piece p) => p >= Piece.BlackRook && p <= Piece.BlackPawn;
    public static Side SideOf(this Piece p) => p.IsRed() ? Side.Red : Side.Black;
}