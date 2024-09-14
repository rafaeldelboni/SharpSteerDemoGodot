namespace SharpSteerDemo.scripts;

public partial class MainScene : Node
{
	public override void _Input(InputEvent input)
	{
		if (input.IsActionPressed("Quit"))
			GetTree().Quit();
	}
}
