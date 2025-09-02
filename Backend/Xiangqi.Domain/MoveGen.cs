namespace Xiangqi.Domain;

/// <summary>
/// This is good enough to play real games; you can refine evaluation/search later.
/// reasonably complete + “flying general” + legality check
/// GenerateLegal = pseudo moves filtered by IsKingInCheck after applying (also covers flying general).
/// Pawns: forward + sideways after crossing river; attack pattern equals move pattern in Xiangqi.
/// This is enough for real legal play. You can refine draw rules/repetition later.
/// </summary>
public interface IMoveGenerator
{
    IEnumerable<Move> GenerateLegal(Position pos);
    Position Apply(Position pos, Move m);
}

public sealed class MoveGenerator : IMoveGenerator
{
    public IEnumerable<Move> GenerateLegal(Position pos)
    {
        foreach (var m in GeneratePseudo(pos))
        {
            var after = Apply(pos, m);
            if (!IsKingInCheck(after, Opposite(after.SideToMove)))
                yield return m;
        }
    }

    public Position Apply(Position pos, Move m)
    {
        var np = pos.Clone();
        var piece = np.Board[m.From];
        np.Board[m.To] = piece;
        np.Board[m.From] = Piece.Empty;
        np.SideToMove = Opposite(np.SideToMove);
        return np;
    }

    static Side Opposite(Side s) => s == Side.Red ? Side.Black : Side.Red;

    IEnumerable<Move> GeneratePseudo(Position pos)
    {
        for (int i = 0; i < Position.Squares; i++)
        {
            var p = pos.Board[i];
            if (p.IsEmpty()) continue;
            if ((p.IsRed() && pos.SideToMove != Side.Red) ||
                (p.IsBlack() && pos.SideToMove != Side.Black)) continue;

            switch (p)
            {
                case Piece.RedRook or Piece.BlackRook:
                    foreach (var m in RookMoves(pos, i)) yield return m; break;
                case Piece.RedCannon or Piece.BlackCannon:
                    foreach (var m in CannonMoves(pos, i)) yield return m; break;
                case Piece.RedHorse or Piece.BlackHorse:
                    foreach (var m in HorseMoves(pos, i)) yield return m; break;
                case Piece.RedElephant or Piece.BlackElephant:
                    foreach (var m in ElephantMoves(pos, i)) yield return m; break;
                case Piece.RedAdvisor or Piece.BlackAdvisor:
                    foreach (var m in AdvisorMoves(pos, i)) yield return m; break;
                case Piece.RedKing or Piece.BlackKing:
                    foreach (var m in KingMoves(pos, i)) yield return m; break;
                case Piece.RedPawn or Piece.BlackPawn:
                    foreach (var m in PawnMoves(pos, i)) yield return m; break;
            }
        }
    }

    IEnumerable<Move> RookMoves(Position pos, int from)
    {
        var (f, r) = Position.FR(from);
        var side = pos.Board[from].SideOf();

        foreach (var (df, dr) in new (int,int)[]{ (1,0), (-1,0), (0,1), (0,-1) })
        {
            int nf = f + df, nr = r + dr;
            while (Position.OnBoard(nf, nr))
            {
                var to = Position.Index(nf, nr);
                var target = pos.Board[to];
                if (target.IsEmpty()) { yield return new Move(from, to); }
                else { if (target.SideOf() != side) yield return new Move(from, to); break; }
                nf += df; nr += dr;
            }
        }
    }

    IEnumerable<Move> CannonMoves(Position pos, int from)
    {
        var (f, r) = Position.FR(from);
        var side = pos.Board[from].SideOf();

        foreach (var (df, dr) in new (int,int)[]{ (1,0), (-1,0), (0,1), (0,-1) })
        {
            int nf = f + df, nr = r + dr;
            bool jumped = false;
            while (Position.OnBoard(nf, nr))
            {
                var to = Position.Index(nf, nr);
                var t = pos.Board[to];

                if (!jumped)
                {
                    if (t.IsEmpty()) yield return new Move(from, to);
                    else jumped = true;
                }
                else
                {
                    if (!t.IsEmpty())
                    {
                        if (t.SideOf() != side) yield return new Move(from, to);
                        break;
                    }
                }
                nf += df; nr += dr;
            }
        }
    }

    IEnumerable<Move> HorseMoves(Position pos, int from)
    {
        var (f, r) = Position.FR(from);
        var side = pos.Board[from].SideOf();

        var cands = new (int legF,int legR,int toF,int toR)[]
        {
            (f+0,r-1,f+1,r-2),(f+0,r-1,f-1,r-2),
            (f+1,r+0,f+2,r-1),(f+1,r+0,f+2,r+1),
            (f+0,r+1,f+1,r+2),(f+0,r+1,f-1,r+2),
            (f-1,r+0,f-2,r-1),(f-1,r+0,f-2,r+1),
        };

        foreach (var c in cands)
        {
            if (!Position.OnBoard(c.legF, c.legR) || !Position.OnBoard(c.toF, c.toR)) continue;
            if (!pos.Board[Position.Index(c.legF, c.legR)].IsEmpty()) continue; // leg blocked
            var to = Position.Index(c.toF, c.toR);
            var t = pos.Board[to];
            if (t.IsEmpty() || t.SideOf() != side) yield return new Move(from, to);
        }
    }

    IEnumerable<Move> ElephantMoves(Position pos, int from)
    {
        var (f, r) = Position.FR(from);
        var side = pos.Board[from].SideOf();

        foreach (var (df, dr) in new (int,int)[]{ (2,2), (2,-2), (-2,2), (-2,-2) })
        {
            int mf = f + df/2, mr = r + dr/2;
            int tf = f + df, tr = r + dr;
            if (!Position.OnBoard(tf, tr) || !Position.OnBoard(mf, mr)) continue;
            if (!pos.Board[Position.Index(mf, mr)].IsEmpty()) continue;
            if (side == Side.Red && tr <= 4) continue;  // red cannot go to ranks 0..4
            if (side == Side.Black && tr >= 5) continue;

            var to = Position.Index(tf, tr);
            var t = pos.Board[to];
            if (t.IsEmpty() || t.SideOf() != side) yield return new Move(from, to);
        }
    }

    static bool InPalace(Side s, int f, int r)
    {
        if (f < 3 || f > 5) return false;
        return s == Side.Black ? r <= 2 : r >= 7;
    }

    IEnumerable<Move> AdvisorMoves(Position pos, int from)
    {
        var (f, r) = Position.FR(from);
        var side = pos.Board[from].SideOf();
        foreach (var (df, dr) in new (int,int)[]{ (1,1), (1,-1), (-1,1), (-1,-1) })
        {
            int tf = f + df, tr = r + dr;
            if (!Position.OnBoard(tf, tr)) continue;
            if (!InPalace(side, tf, tr)) continue;
            var to = Position.Index(tf, tr);
            var t = pos.Board[to];
            if (t.IsEmpty() || t.SideOf() != side) yield return new Move(from, to);
        }
    }

    IEnumerable<Move> KingMoves(Position pos, int from)
    {
        var (f, r) = Position.FR(from);
        var side = pos.Board[from].SideOf();
        foreach (var (df, dr) in new (int,int)[]{ (1,0), (-1,0), (0,1), (0,-1) })
        {
            int tf = f + df, tr = r + dr;
            if (!Position.OnBoard(tf, tr)) continue;
            if (!InPalace(side, tf, tr)) continue;
            var to = Position.Index(tf, tr);
            var t = pos.Board[to];
            if (t.IsEmpty() || t.SideOf() != side) yield return new Move(from, to);
        }
    }

    IEnumerable<Move> PawnMoves(Position pos, int from)
    {
        var (f, r) = Position.FR(from);
        var side = pos.Board[from].SideOf();

        int fr = side == Side.Red ? r - 1 : r + 1;
        if (Position.OnBoard(f, fr))
        {
            var to = Position.Index(f, fr);
            var t = pos.Board[to];
            if (t.IsEmpty() || t.SideOf() != side) yield return new Move(from, to);
        }

        bool crossed = side == Side.Red ? r <= 4 : r >= 5;
        if (crossed)
        {
            foreach (var nf in new[] { f - 1, f + 1 })
            {
                if (!Position.OnBoard(nf, r)) continue;
                var to = Position.Index(nf, r);
                var t = pos.Board[to];
                if (t.IsEmpty() || t.SideOf() != side) yield return new Move(from, to);
            }
        }
    }

    static bool IsKingInCheck(Position pos, Side kingSide)
    {
        int ksq = -1;
        for (int i = 0; i < Position.Squares; i++)
        {
            var p = pos.Board[i];
            if (kingSide == Side.Red && p == Piece.RedKing) { ksq = i; break; }
            if (kingSide == Side.Black && p == Piece.BlackKing) { ksq = i; break; }
        }
        if (ksq == -1) return true;

        var (kf, kr) = Position.FR(ksq);
        var opp = kingSide == Side.Red ? Side.Black : Side.Red;

        // Rook/Cannon/Flying-General along ranks/files
        foreach (var (df, dr) in new (int,int)[]{ (1,0), (-1,0), (0,1), (0,-1) })
        {
            int nf = kf + df, nr = kr + dr;
            bool seenScreen = false;
            while (Position.OnBoard(nf, nr))
            {
                var sq = Position.Index(nf, nr);
                var p = pos.Board[sq];
                if (p.IsEmpty()) { nf += df; nr += dr; continue; }

                if (!seenScreen)
                {
                    if (p == Opp(opp, Piece.RedKing)) return true; // flying general
                    if (p == Opp(opp, Piece.RedRook)) return true; // rook attack
                    // first blocker becomes screen for cannon
                    seenScreen = true;
                    nf += df; nr += dr;
                    continue;
                }
                else
                {
                    if (p == Opp(opp, Piece.RedCannon)) return true; // cannon capture over screen
                    break;
                }
            }
        }

        // Horse checks (with leg logic)
        foreach (var (legF, legR, toF, toR) in new (int,int,int,int)[]{
            (kf+0,kr-1,kf+1,kr-2),(kf+0,kr-1,kf-1,kr-2),
            (kf+1,kr+0,kf+2,kr-1),(kf+1,kr+0,kf+2,kr+1),
            (kf+0,kr+1,kf+1,kr+2),(kf+0,kr+1,kf-1,kr+2),
            (kf-1,kr+0,kf-2,kr-1),(kf-1,kr+0,kf-2,kr+1),
        })
        {
            if (!Position.OnBoard(legF, legR) || !Position.OnBoard(toF, toR)) continue;
            if (!pos.Board[Position.Index(legF, legR)].IsEmpty()) continue;
            var t = pos.Board[Position.Index(toF, toR)];
            if (!t.IsEmpty() && t == Opp(opp, Piece.RedHorse)) return true;
        }

        // Pawn checks (their moves = attacks)
        var pawnDirs = opp == Side.Red ? new (int, int)[] { (0,-1),(1,0),(-1,0) } : new[] { (0,1),(1,0),(-1,0) };
        foreach (var (df, dr) in pawnDirs)
        {
            int tf = kf + df, tr = kr + dr;
            if (!Position.OnBoard(tf, tr)) continue;
            var t = pos.Board[Position.Index(tf, tr)];
            if (t == Opp(opp, Piece.RedPawn))
            {
                if (df == 0) return true; // forward
                bool crossed = opp == Side.Red ? tr <= 4 : tr >= 5;
                if (crossed) return true;
            }
        }

        return false;
    }

    static Piece Opp(Side side, Piece redType) => side == Side.Red ? redType : redType switch
    {
        Piece.RedRook => Piece.BlackRook,
        Piece.RedHorse => Piece.BlackHorse,
        Piece.RedElephant => Piece.BlackElephant,
        Piece.RedAdvisor => Piece.BlackAdvisor,
        Piece.RedKing => Piece.BlackKing,
        Piece.RedCannon => Piece.BlackCannon,
        Piece.RedPawn => Piece.BlackPawn,
        _ => Piece.Empty
    };
}