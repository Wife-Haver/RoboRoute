using System.Drawing;

namespace RoboRoute.Winforms.Core;

// Turns the robot 90 degrees to the left
public class TurnLeftBlock : Block
{
    public override BlockType Type => BlockType.TurnLeft;
    public override string DisplayName => "Turn Left";
    public override Color FillColor => Color.FromArgb(52, 101, 164);

    public override bool Perform(Robot robot, Func<Point, bool> isWalkable)
    {
        robot.TurnLeft();
        return true;
    }

    public override bool AppendSteps(List<Block> steps, int maxSteps)
    {
        steps.Add(this);
        return steps.Count <= maxSteps;
    }
}
