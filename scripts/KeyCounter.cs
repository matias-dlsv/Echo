using Godot;

public partial class KeyCounter : Label
{
	[Export] public Player Player;
	[Export] public int TotalKeys = 3;

	public override void _Process(double delta)
	{
		if (Player == null) return;

		Text = $"Llaves: {Player.KeysCollected}/{TotalKeys}";
	}
}
