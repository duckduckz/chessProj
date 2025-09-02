namespace Xiangqi.Infrastructure.Services;

public interface IClockService { DateTime UtcNow(); }
public sealed class ClockService : IClockService { public DateTime UtcNow() => DateTime.UtcNow; }

public interface IRobot
{
    Move? ChooseMove(string fen, int timeMs = 500);
}

/// very first robot: random legal move
public sealed class RandomRobot : IRobot
{
    private readonly IMoveGenerator _gen;
    private readonly Random _rng = new();
    public RandomRobot(IMoveGenerator gen) => _gen = gen;

    public Move? ChooseMove(string fen, int timeMs = 500)
    {
        var pos = Fen.Parse(fen);
        var moves = _gen.GenerateLegal(pos).ToList();
        return moves.Count == 0 ? null : moves[_rng.Next(moves.Count)];
    }
}
