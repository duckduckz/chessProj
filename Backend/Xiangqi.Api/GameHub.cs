namespace Xiangqi.Api;

public class GameHub : Hub
{
    public Task JoinRoom(string roomId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");
}