using Godot;

public partial class EchoCounter : Label
{
	[Export] public Player Player;

	public override void _Process(double delta)
	{
		if (Player == null) return;

		int nextEcho = Player.EchoCount;

		Text = $"ECHO: {nextEcho} / 3";

		// El tercer ECHO hace ruido y alerta a los enemigos
		if (nextEcho == 3)
		{
			AddThemeColorOverride("font_color", new Color("#FF6B6B"));
		}
		else
		{
			AddThemeColorOverride("font_color", Colors.White);
		}
	}
}
