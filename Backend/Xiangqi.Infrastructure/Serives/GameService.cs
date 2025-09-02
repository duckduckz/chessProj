namespace Xiangqi.Infrastructure.Services;

public interface IGameService
{
    Task<Game> StartAsync(Guid roomId);
    Task<(bool ok, string err, Game game)> TryMoveAsync(Guid roomId, Guid byUser, int from, int to);
    Task<(bool ok, string err, Game game)> ResignAsync(Guid roomId, Guid byUser);
    Task<Game> SnapshotAsync(Guid roomId);

}

public sealed class GameService : IGameService
{
    private readonly IRoomRepository _roomRepo;
    private readonly IGameRepository _gameRepo;
    private readonly IMoveGenerator _gen;
    private readonly IRobot _robot;
    private readonly IClockService _clock;
    private readonly IUserRepository _users;

    public GameService(IRoomRepository roomRepo, IGameRepository gameRepo, IMoveGenerator gen, IRobot robot, IClockService clock, IUserRepository users)
    {
        _roomRepo = roomRepo;
        _gameRepo = gameRepo;
        _gen = gen;
        _robot = robot;
        _clock = clock;
        _users = users;
    }

    public async Task<Game> StartAsync(Guid roomId)
    {
        var r = await _roomRepo.GetAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (r.RedPlayerId is null || r.BlackPlayerId is null) throw new InvalidOperationException("Seats not filled");
        if (r.Playing) throw new InvalidOperationException("Game already running");

        var g = new Game
        {
            RoomId = roomId,
            CurrentFen = Fen.Start,
            Clock = new ClockState
            {
                RedMs = r.Settings.BaseSeconds * 1000,
                BlackMs = r.Settings.BaseSeconds * 1000,
                SideToMove = "red",
                TurnStartUtc = _clock.UtcNow()
            }
        };
        await _gameRepo.AddAsync(g);
        r.GameId = g.Id; r.Playing = true;
        return g;
    }

    public async Task<Game> SnapshotAsync(Guid roomId)
    {
        var r = await _roomRepo.GetAsync(roomId); if (r?.GameId == null) return null;
        return await _gameRepo.GetAsync(r.GameId.Value);
    }

    public async Task<(bool ok, string err, Game game)> TryMoveAsync(Guid roomId, Guid byUser, int from, int to)
    {
        var r = await _roomRepo.GetAsync(roomId) ?? throw new InvalidOperationException("Room not found");
        if (r.GameId is null) return (false, "No game running.", null);

        var g = await _gameRepo.GetAsync(r.GameId.Value) ?? throw new InvalidOperationException("Game not found");
        if (g.Result != GameResult.Ongoing) return (false, "Game finished.", g);

        var pos = Fen.Parse(g.CurrentFen);
        var mv = new Move(from, to);
        var legal = _gen.GenerateLegal(pos).ToHashSet();
        if (!legal.Contains(mv)) return (false, "Illegal move.", g);

        // Activity + last active
        var today = DateOnly.FromDateTime(_clock.UtcNow());
        var user = await _users.GetAsync(byUser);
        if (user is not null) { user.Stats.BumpActivity(today); user.LastActiveUtc = _clock.UtcNow(); }

        // Clocks
        var now = _clock.UtcNow();
        if (g.Clock.TurnStartUtc is DateTime started)
        {
            var spent = (int)(now - started).TotalMilliseconds;
            if (g.Clock.SideToMove == "red") g.Clock.RedMs -= spent; else g.Clock.BlackMs -= spent;
            if (g.Clock.RedMs < 0 || g.Clock.BlackMs < 0)
            {
                g.Result = g.Clock.RedMs < 0 ? GameResult.BlackWin : GameResult.RedWin;
                g.ResultReason = "Time";
                await FinalizeResultAndBumpStats(r, g);
                await _gameRepo.SaveAsync(g);
                return (false, "Flagged on time.", g);
            }
        }

        // Apply move
        var after = _gen.Apply(pos, mv);
        g.CurrentFen = Fen.ToFen(after);
        g.Moves.Add(new MoveRecord { Ply = g.Moves.Count + 1, From = from, To = to, FenAfter = g.CurrentFen, ByUserId = byUser });

        // Flip + increment
        if (g.Clock.SideToMove == "red") g.Clock.RedMs += r.Settings.IncrementSeconds * 1000;
        else g.Clock.BlackMs += r.Settings.IncrementSeconds * 1000;
        g.Clock.SideToMove = (g.Clock.SideToMove == "red") ? "black" : "red";
        g.Clock.TurnStartUtc = now;

        await _gameRepo.SaveAsync(g);

        // Robot
        if (r.Settings.VsRobot &&
            ((r.Settings.RobotSide?.Equals("red", StringComparison.OrdinalIgnoreCase) == true && g.Clock.SideToMove == "red") ||
             (r.Settings.RobotSide?.Equals("black", StringComparison.OrdinalIgnoreCase) == true && g.Clock.SideToMove == "black")))
        {
            var bot = _robot.ChooseMove(g.CurrentFen);
            if (bot is Move rmv)
            {
                var p2 = Fen.Parse(g.CurrentFen);
                var after2 = _gen.Apply(p2, rmv);
                g.CurrentFen = Fen.ToFen(after2);
                g.Moves.Add(new MoveRecord { Ply = g.Moves.Count + 1, From = rmv.From, To = rmv.To, FenAfter = g.CurrentFen, ByUserId = Guid.Empty });

                if (g.Clock.SideToMove == "red") g.Clock.RedMs += r.Settings.IncrementSeconds * 1000;
                else g.Clock.BlackMs += r.Settings.IncrementSeconds * 1000;
                g.Clock.SideToMove = (g.Clock.SideToMove == "red") ? "black" : "red";
                g.Clock.TurnStartUtc = _clock.UtcNow();

                await _gameRepo.SaveAsync(g);
            }
        }

        return (true, null, g);
    }

    public async Task<(bool ok, string err, Game game)> ResignAsync(Guid roomId, Guid byUser)
    {
        var r = await _roomRepo.GetAsync(roomId) ?? throw new InvalidOperationException("room not found");
        if (r.GameId is null)
            return (false, "no game running", null);

        var g = await _gameRepo.GetAsync(r.GameId.Value) ?? throw new InvalidOperationException("game not found");
        if (g.Result != GameResult.Ongoing)
            return (false, "game finished", g);

        Guid? winner = null;
        if (r.RedPlayerId == byUser)
            winner = r.BlackPlayerId;
        else if (r.BlackPlayerId == byUser)
            winner = r.RedPlayerId;
        if (winner is null)
            return (false, "resign only by seated player", g);

        g.Result = winner == r.RedPlayerId ? GameResult.RedWin : GameResult.BlackWin;
        g.ResultReason = "resign";

        await FinalizeResultAndBumpStats(r, g);
        await _gameRepo.SaveAsync(g);
        return (true, null, g);

    }

    public async Task FinalizeResultAndBumpStats(Room r, Game g)
    {
        r.Playing = false;

        var red = r.RedPlayerId is Guid rid ? await _users.GetAsync(rid) : null;
        var black = r.BlackPlayerId is Guid bid ? await _users.GetAsync(bid) : null;

        if (red is not null) red.Stats.Games++;
        if (black is not null) black.Stats.Games++;

        switch (g.Result)
        {
            case GameResult.RedWin:
                if (red is not null) red.Stats.Wins++;
                if (black is not null) black.Stats.Losses++;
                break;
            case GameResult.BlackWin:
                if (red is not null) red.Stats.Wins++;
                if (black is not null) black.Stats.Losses++;
                break;
            case GameResult.Draw:
                if (red is not null) red.Stats.Draws++;
                if (black is not null) black.Stats.Draws++;
                break;            
        }
    }

}