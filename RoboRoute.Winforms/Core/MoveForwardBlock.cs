using System.Drawing;

namespace RoboRoute.Winforms.Core;

// Moves the robot one tile forward
public class MoveForwardBlock : Block
{
    public override BlockType Type => BlockType.MoveForward;
    public override string DisplayName => "Move Forward";
    public override Color FillColor => Color.FromArgb(46, 125, 80);

    public override bool Perform(Robot robot, Func<Point, bool> isWalkable)
    {
        return robot.MoveForward(isWalkable);   // false if there is a wall in the way
    }

    public override bool AppendSteps(List<Block> steps, int maxSteps)
    {
        steps.Add(this);
        return steps.Count <= maxSteps;
    }
}
