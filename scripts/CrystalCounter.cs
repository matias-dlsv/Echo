
using Godot;

public partial class CrystalCounter : Label
{
	[Export] public Crystal CrystalBlue;
	[Export] public Crystal CrystalYellow;
	[Export] public Crystal CrystalPurple;

	public override void _Process(double delta)
	{
		int activated = 0;

		if (CrystalBlue != null && CrystalBlue.IsActivated)
			activated++;

		if (CrystalYellow != null && CrystalYellow.IsActivated)
			activated++;

		if (CrystalPurple != null && CrystalPurple.IsActivated)
			activated++;

		Text = $"CRYSTALS: {activated}/3";
	}
}
