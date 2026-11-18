
using Godot;

public partial class EndGame : Control
{
	[Export(PropertyHint.File, "*.tscn")]
	public string RestartScene = "res://scene/tutorial.tscn";

	public override void _Ready()
	{
		GetTree().Paused = false;

		Button restartButton = GetNode<Button>(
            "CenterContainer/PanelContainer/VBoxContainer/RestartButton"
		);

		restartButton.Pressed += OnRestartPressed;
	}

	private void OnRestartPressed()
	{
		if (!ResourceLoader.Exists(RestartScene))
		{
			GD.PushError($"No existe la escena: {RestartScene}");
			return;
		}

		GetTree().ChangeSceneToFile(RestartScene);
	}
}
