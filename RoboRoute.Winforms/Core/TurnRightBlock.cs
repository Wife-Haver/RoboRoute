using System.Drawing;

namespace RoboRoute.Winforms.Core;

// Turns the robot 90 degrees to the right
public class TurnRightBlock : Block
{
    public override BlockType Type => BlockType.TurnRight;
    public override string DisplayName => "Turn Right";
    public override Color FillColor => Color.FromArgb(164, 101, 52);

    public override bool Perform(Robot robot, Func<Point, bool> isWalkable)
    {
        robot.TurnRight();
        return true;
    }

    public override bool AppendSteps(List<Block> steps, int maxSteps)
    {
        steps.Add(this);
        return steps.Count <= maxSteps;
    }
}
