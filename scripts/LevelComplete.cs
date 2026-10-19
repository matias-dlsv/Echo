using Godot;

// Pantalla final: fondo negro con texto. Cualquier tecla lleva a NextScene.
public partial class LevelComplete : Control
{
	[Export(PropertyHint.File, "*.tscn")] public string NextScene = "";

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!@event.IsPressed() || @event.IsEcho()) return;

		if (string.IsNullOrEmpty(NextScene))
		{
			GD.Print("LevelComplete: elige Next Scene en el Inspector del nodo raíz.");
			return;
		}

		GetTree().ChangeSceneToFile(NextScene);
	}
}
