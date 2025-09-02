namespace Xiangqi.Infrastructure.Models;

public sealed class RoomSettings
{
    public Visibility Visibility { get; set; } = Visibility.Public;
    public bool AllowUndo { get; set; } = false;
    public bool Rated { get; set; } = false;
    public int SpectatorLimit { get; set; } = 50;
    public string Password { get; set; }
    public bool VsRobot { get; set; } = false;
    public string RobotSide { get; set; }  // red or black
    public int BaseSeconds { get; set; } = 300;
    public int IncrementSeconds { get; set; } = 5;
}
