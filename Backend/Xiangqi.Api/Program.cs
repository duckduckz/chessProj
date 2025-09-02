var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// In-memory repos
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepo>();
builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepo>();
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepo>();
builder.Services.AddScoped<IRoomQueryService, RoomQueryService>();


// Services & domain
builder.Services.AddSingleton<IMoveGenerator, MoveGenerator>();
builder.Services.AddSingleton<IRobot, RandomRobot>();
builder.Services.AddSingleton<IClockService, ClockService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();
app.UseSwagger().UseSwaggerUI();

app.MapHub<GameHub>("/hub");

// ----------------- AUTH -----------------

// (guest)
app.MapPost("/auth/guest", async (string username, IUserRepository users) =>
{
    if (string.IsNullOrWhiteSpace(username))
        return Results.BadRequest(new
        {
            error = "username required"
        });

    var existing = await users.FindByNameAsync(username);
    if (existing is not null)
        return Results.BadRequest(new { error = "Username already taken" });

    var u = await users.AddAsync(username);
    return Results.Ok(new { u.Id, u.Username, token = u.Id.ToString() }); // token=Id (prototype)
});

// guest
app.MapPost("/auth/register", async (RegisterDto dto, IUserRepository users) =>
{
    if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest(new
        {
            error = "username and password required."
        });
    try
    {
        var u = await users.AddAsync(dto.Username, dto.DisplayName, dto.Password);
        return Results.Ok(new
        {
            u.Id,
            u.Username,
            u.DisplayName,
            token = u.Id.ToString()
        });
    }
    catch
    {
        return Results.BadRequest(new
        {
            error = "username already taken"
        });
    }

});

// login
app.MapPost("/auth/login", async (LoginDto dto, IUserRepository users) =>
{
    var u = await users.FindByNameAsync(dto.Username);
    if (u is null || u.PasswordHash is null || !PasswordHash.Verify(dto.Password, u.PasswordHash))
        return Results.BadRequest(new { error = "invalid username or password" });

    u.LastActivateUtc = DateTime.UtcNow;
    return Results.Ok(new
    {
        u.Id,
        u.Username,
        u.DisplayName,
        token = u.Id.ToString()
    });

});

// update progile (username/displayname)
app.MapPatch("/users/{userId:guid}", async (Guid userId, UpdateProfileDto dto, IUserRepository users) =>
{
    var u = await users.GetAsync(userId);
    if (u is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(dto.Username))
    {
        var ok = await users.ChangeUsernameAsync(userId, dto.Username);
        if (!ok)
            return Results.BadRequest(new
            {
                error = "username already taken"
            });
    }

    if (!string.IsNullOrWhiteSpace(dto.DisplayName))
        u.DisplayName = dto.DisplayName!;
    return Results.Ok(new
    {
        u.Id,
        u.Username,
        u.DisplayName
    });
});

// change password
app.MapPut("/users/{userId:guid}/password", async (Guid userId, ChangePasswordDto dto, IUserRepository users) =>
{
    if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        return Results.BadRequest(new { error = "Both old and new password required." });

    var u = await users.GetAsync(userId);
    if (u is null || u.PasswordHash is null || !PasswordHash.Verify(dto.OldPassword, u.PasswordHash))
        return Results.BadRequest(new { error = "Old password is incorrect." });

    var ok = await users.SetPasswordAsync(userId, dto.NewPassword);
    return ok ? Results.Ok() : Results.BadRequest(new { error = "Failed to set password." });
});

// dashboard (win, lose, draw, game, 180 heatmap)
app.MapGet("/users/{userId:guid}/dashboard", async (Guid userId, IUserRepository users) =>
{
    var u = await users.GetAsync(userId);
    if (u is null) return Results.NotFound();

    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var start = today.AddDays(-179);

    var days = new List<object>(180);
    for (var d = start; d <= today; d = d.AddDays(1))
    {
        u.Stats.Activity.TryGetValue(d, out var count);
        days.Add(new { date = d.ToString("yyyy-MM-dd"), count });
    }

    // current daily activity streak
    int streak = 0;
    for (var d = today; streak < 365; d = d.AddDays(-1))
    {
        if (u.Stats.Activity.TryGetValue(d, out var c) && c > 0) streak++;
        else break;
    }

    return Results.Ok(new
    {
        user = new { u.Id, u.Username, u.DisplayName, u.CreatedUtc, u.LastActiveUtc, active = u.LastActiveUtc.HasValue && (DateTime.UtcNow - u.LastActiveUtc.Value) < TimeSpan.FromMinutes(5) },
        stats = new { u.Stats.Games, u.Stats.Wins, u.Stats.Losses, u.Stats.Draws, activeDays = u.Stats.Activity.Count, currentStreakDays = streak },
        heatmap = days
    });
});

// update profile photo
app.MapPut("/users/{userId:guid}/photo", async (Guid userId, UpdatePhotoDto dto, IUserRepository users) =>
{
    var u = await users.GetAsync(userId);
    if (u is null) return Results.NotFound();

    if (dto.Base64Image.Length > 1000000)
        return Results.BadRequest(new
        {
            ERROR = "IMAGE TOO LARGE"
        });

    u.ProfilePhotoBase64 = dto.Base64Image;
    return Results.Ok(new
    {
        message = "profile photo update"
    });
});

// Get profile photo
app.MapGet("/users/{userId:guid}/photo", async (Guid userId, IUserRepository users) =>
{
    var u = await users.GetAsync(userId);
    if (u is null) return Results.NotFound();

    return string.IsNullOrEmpty(u.ProfilePhotoBase64)
        ? Results.NotFound(new { error = "No photo set." })
        : Results.Ok(new { userId = u.Id, base64 = u.ProfilePhotoBase64 });
});

// ----------------------- rooms ------------------------

// create -> return id + code
app.MapPost("/rooms", async (CreateRoomDto dto, IRoomService rooms) =>
{
    if (dto.Settings.Visibility == Visibility.Private && string.IsNullOrWhiteSpace(dto.Settings.Password))
        return Results.BadRequest(new { error = "Private room requires a password." });

    var room = await rooms.CreateAsync(
        dto.OwnerId,
        dto.Name,
        dto.Settings
    );
    return Results.Ok(new
    {
        room.Id,
        room.Code,
        room.Name,
        room.Settings
    });
});

// list public rooms
app.MapGet("/rooms", async (
    IRoomQueryService svc,
    string visibility,   // public|private|unlisted|all
    string full,         // all|onlyFull|onlyOpen
    Guid? ownerId,
    Guid? participantId,
    string q,            // search
    int page = 1,
    int pageSize = 50
) =>
{
    Visibility? vis = visibility?.ToLowerInvariant() switch
    {
        "public" => Visibility.Public,
        "private" => Visibility.Private,
        "unlisted" => Visibility.Unlisted,
        "all" or null or "" => null,
        _ => Visibility.Public
    };

    var fullFilter = full?.ToLowerInvariant() switch
    {
        "onlyfull" => FullFilter.OnlyFull,
        "onlyopen" => FullFilter.OnlyOpen,
        _ => FullFilter.All
    };

    var result = await svc.ListAsync(new RoomListQuery(
        Visibility: vis ?? Visibility.Public,
        Full: fullFilter,
        OwnerId: ownerId,
        ParticipantId: participantId,
        Search: q,
        Page: page,
        PageSize: pageSize
    ));

    return Results.Ok(result);
});



// get by guid
app.MapGet("/rooms/{roomId:guid}", async (Guid roomId, IRoomRepository repo) =>
{
    var r = await repo.GetAsync(roomId);
    return r is null ? Results.NotFound() : Results.Ok(new { r.Id, r.Code, r.Name, r.Playing, r.Settings, r.RedPlayerId, r.BlackPlayerId });
});

// join by guid
app.MapPost("/rooms/{roomId:guid}/join", async (Guid roomId, Guid userId, string role, string password, IRoomRepository repo, IRoomService rooms) =>
{
    var r = await repo.GetAsync(roomId);
    if (r is null) return Results.NotFound();

    if (r.Settings.Visibility == Visibility.Private && r.Settings.Password != password)
        return Results.BadRequest(new { error = "Invalid password for private room." });

    if (r.RedPlayerId is not null && r.BlackPlayerId is not null)
        return Results.BadRequest(new { error = "Room is full." });

    var ok = await rooms.JoinAsync(roomId, userId, role ?? "spectator", password);
    return ok ? Results.Ok() : Results.BadRequest(new { error = "Join failed" });
});


// join by code
app.MapPost("/rooms/{code}/join", async (string code, Guid userId, string role, string password, IRoomRepository repo, IRoomService rooms) =>
{
    var r = await repo.GetByCodeAsync(code);
    if (r is null) return Results.NotFound();
    var ok = await rooms.JoinAsync(r.Id, userId, role ?? "spectator", password);
    return ok ? Results.Ok() : Results.BadRequest(new { error = "Join failed" });
});

// leave by guid
app.MapPost("/rooms/{roomId:guid}/leave", async (Guid roomId, Guid userId, IRoomService rooms) =>
{
    await rooms.LeaveAsync(roomId, userId);
    return Results.Ok();
});

// ----------------------- lobby / waiting mode ------------------
app.MapPost("/rooms/{roomId:guid}/wait",
    async (Guid roomId, Guid userId, string password,
           IRoomRepository repo, IRoomService rooms, IHubContext<GameHub> hub) =>
{
    var r = await repo.GetAsync(roomId);
    if (r is null) return Results.NotFound();

    // explicit full check
    if (r.RedPlayerId is not null && r.BlackPlayerId is not null)
        return Results.BadRequest(new { error = "Room is full" });

    // (optional) private room password check before service (for precise errors)
    if (r.Settings.Visibility == Visibility.Private && r.Settings.Password != password)
        return Results.BadRequest(new { error = "Wrong password" });

    var ok = await rooms.JoinWaitingAsync(roomId, userId, password);
    if (!ok) return Results.BadRequest(new { error = "Join waiting failed" });

    await hub.Clients.Group($"room:{roomId}").SendAsync("LobbyUpdated", new { roomId });
    return Results.Ok();
});
// Owner assigns a waiting/spectator user to a seat ("red" or "black")
app.MapPost("/rooms/{roomId:guid}/seat", async (Guid roomId, SeatDto dto, IRoomService rooms, IHubContext<GameHub> hub) =>
{
    var ok = await rooms.AssignSeatAsync(roomId, dto.OwnerId, dto.UserId, dto.Role);
    if (!ok) return Results.BadRequest(new { error = "Seat assignment failed" });

    await hub.Clients.Group($"room:{roomId}").SendAsync("SeatsUpdated", new { roomId });
    return Results.Ok();
});

// Owner unassigns a seat back to waiting (optional)
app.MapPost("/rooms/{roomId:guid}/unseat", async (Guid roomId, UnseatDto dto, IRoomService rooms, IHubContext<GameHub> hub) =>
{
    var ok = await rooms.UnassignSeatAsync(roomId, dto.OwnerId, dto.Role);
    if (!ok) return Results.BadRequest(new { error = "Unseat failed" });

    await hub.Clients.Group($"room:{roomId}").SendAsync("SeatsUpdated", new { roomId });
    return Results.Ok();
});

// Get lobby status (seats + waiting list)
app.MapGet("/rooms/{roomId:guid}/lobby", async (Guid roomId, IRoomRepository repo) =>
{
    var r = await repo.GetAsync(roomId);
    if (r is null) return Results.NotFound();
    return Results.Ok(new
    {
        r.Id, r.Code, r.Name,
        seats = new { red = r.RedPlayerId, black = r.BlackPlayerId },
        waiting = r.Waiting.ToArray(),
        spectators = r.Spectators.ToArray(),
        isFull = r.RedPlayerId is not null && r.BlackPlayerId is not null,
        canStart = r.RedPlayerId is not null && r.BlackPlayerId is not null && !r.Playing
    });
});



// --------------------- game --------------------
// start by guid
app.MapPost("/rooms/{roomId:guid}/game/start", async (Guid roomId, IGameService games, IHubContext<GameHub> hub) =>
{
    var g = await games.StartAsync(roomId);
    await hub.Clients.Group($"room:{roomId}").SendAsync("GameStarted", new { roomId, fen = g.CurrentFen, g.Clock });
    return Results.Ok(new { g.Id, g.CurrentFen, g.Clock });
});

// start by code
app.MapPost("/rooms/{code}/game/start", async (string code, IRoomRepository repo, IGameService games, IHubContext<GameHub> hub) =>
{
    var room = await repo.GetByCodeAsync(code);
    if (room is null) return Results.NotFound();
    var g = await games.StartAsync(room.Id);
    await hub.Clients.Group($"room:{room.Id}").SendAsync("GameStarted", new { roomId = room.Id, roomCode = room.Code, fen = g.CurrentFen, g.Clock });
    return Results.Ok(new { g.Id, roomCode = room.Code, g.CurrentFen, g.Clock });
});

// snapshot by guid
app.MapGet("/rooms/{roomId:guid}/game", async (Guid roomId, IGameService games) =>
{
    var g = await games.SnapshotAsync(roomId);
    return g is null ? Results.NotFound() : Results.Ok(g);
});

// move by guid
app.MapPost("/rooms/{roomId:guid}/move", async (Guid roomId, MoveDto dto, IGameService games, IHubContext<GameHub> hub) =>
{
    var (ok, err, g) = await games.TryMoveAsync(roomId, dto.ByUserId, dto.From, dto.To);
    if (!ok) return Results.BadRequest(new { error = err });
    await hub.Clients.Group($"room:{roomId}").SendAsync("MoveApplied", new { roomId, fen = g!.CurrentFen, g.Clock, last = g.Moves.LastOrDefault() });
    return Results.Ok(new { g!.CurrentFen, g.Clock, g.Moves });
});

// resign by guid
app.MapPost("/rooms/{roomId:guid}/resign", async (Guid roomId, Guid byUserId, IGameService games, IHubContext<GameHub> hub) =>
{
    var (ok, err, g) = await games.ResignAsync(roomId, byUserId);
    if (!ok) return Results.BadRequest(new { error = err });
    await hub.Clients.Group($"room:{roomId}").SendAsync("GameEnded", new { roomId, g!.Result, g.ResultReason, g.CurrentFen });
    return Results.Ok(new { g!.Result, g.ResultReason });
});

// resign by code
app.MapPost("/rooms/{code}/resign", async (string code, Guid byUserId, IRoomRepository repo, IGameService games, IHubContext<GameHub> hub) =>
{
    var room = await repo.GetByCodeAsync(code);
    if (room is null) return Results.NotFound();
    var (ok, err, g) = await games.ResignAsync(room.Id, byUserId);
    if (!ok) return Results.BadRequest(new { error = err });
    await hub.Clients.Group($"room:{room.Id}").SendAsync("GameEnded", new { roomId = room.Id, roomCode = room.Code, g!.Result, g.ResultReason, g.CurrentFen });
    return Results.Ok(new { g!.Result, g.ResultReason });
});

app.Run();