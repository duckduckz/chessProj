namespace Xiangqi.Infrastructure.Models;

public sealed class UserStates
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int Games { get; set; }

    // activity count per day
    public Dictionary<DateOnly, int> Activity { get; } = new();
    public void BumpActivity(DateOnly day) =>
        Activity[day] = Activity.TryGetValue(day, out var c) ? c + 1 : 1;

}