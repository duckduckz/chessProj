namespace Xiangqi.Domain;

public sealed class Position
{
    public const int Files = 9;
    public const int Ranks = 10;
    public const int Squares = Files * Ranks;

    public Piece[] Board { get; } = new Piece[Squares];
    public Side SideToMove { get; set; } = Side.Red;

    public Position Clone()
    {
        var p = new Position { SideToMove = SideToMove };
        Array.Copy(Board, p.Board, Squares);
        return p;
    }

    public static int Index(int file, int rank) => rank * Files + file;
    public static (int file, int rank) FR(int index) => (index % Files, index / Files);
    public static bool OnBoard(int f, int r) => f >= 0 && f < Files && r >= 0 && r < Ranks;
}

public readonly record struct Move(int From, int To);

public static class Fen
{
    // Standard Xiangqi start (black on top, red on bottom). Side 'r' or 'b' at end.
    public const string Start = "rheakaehr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RHEAKAEHR r";

    public static Position Parse(string fen)
    {
        var parts = fen.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) throw new ArgumentException("Invalid FEN");

        var rankParts = parts[0].Split('/');
        if (rankParts.Length != Position.Ranks) throw new ArgumentException("Invalid FEN ranks");

        var pos = new Position();
        for (int r = 0; r < Position.Ranks; r++)
        {
            int f = 0;
            foreach (var ch in rankParts[r])
            {
                if (char.IsDigit(ch))
                {
                    f += ch - '0';
                    continue;
                }
                var p = CharToPiece(ch);
                if (f >= Position.Files) throw new ArgumentException("Too many files");
                pos.Board[Position.Index(f, r)] = p;
                f++;
            }
            if (f != Position.Files) throw new ArgumentException("Rank file count mismatch");
        }

        pos.SideToMove = (parts[1] == "r") ? Side.Red : Side.Black;
        return pos;
    }

    public static string ToFen(Position pos)
    {
        var sb = new StringBuilder();
        for (int r = 0; r < Position.Ranks; r++)
        {
            int empty = 0;
            for (int f = 0; f < Position.Files; f++)
            {
                var p = pos.Board[Position.Index(f, r)];
                if (p == Piece.Empty) { empty++; continue; }
                if (empty > 0) { sb.Append(empty); empty = 0; }
                sb.Append(PieceToChar(p));
            }
            if (empty > 0) sb.Append(empty);
            if (r < Position.Ranks - 1) sb.Append('/');
        }
        sb.Append(' ');
        sb.Append(pos.SideToMove == Side.Red ? 'r' : 'b');
        return sb.ToString();
    }
    
    static Piece CharToPiece(char c) => c switch
    {
        'r' => Piece.BlackRook,
        'h' => Piece.BlackHorse,
        'e' => Piece.BlackElephant,
        'a' => Piece.BlackAdvisor,
        'k' => Piece.BlackKing,
        'c' => Piece.BlackCannon,
        'p' => Piece.BlackPawn,
        'R' => Piece.RedRook,
        'H' => Piece.RedHorse,
        'E' => Piece.RedElephant,
        'A' => Piece.RedAdvisor,
        'K' => Piece.RedKing,
        'C' => Piece.RedCannon,
        'P' => Piece.RedPawn,
        _ => throw new ArgumentException($"Bad FEN char: {c}")
    };

    static char PieceToChar(Piece p) => p switch
    {
        Piece.BlackRook => 'r', Piece.BlackHorse => 'h', Piece.BlackElephant => 'e',
        Piece.BlackAdvisor => 'a', Piece.BlackKing => 'k', Piece.BlackCannon => 'c', Piece.BlackPawn => 'p',
        Piece.RedRook => 'R', Piece.RedHorse => 'H', Piece.RedElephant => 'E',
        Piece.RedAdvisor => 'A', Piece.RedKing => 'K', Piece.RedCannon => 'C', Piece.RedPawn => 'P',
        _ => '1'
    };
}
