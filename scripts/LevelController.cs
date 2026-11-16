
using Godot;

public partial class LevelController : Node
{
	[Export] public int RequiredKeys = 2;
	[Export] public bool IsFinalLevel = false;
	[Export] public float MessageDuration = 4.0f;

	private CanvasLayer _levelUI;
	private Label _keysLabel;
	private Label _descriptionLabel;

	public override async void _Ready()
	{
		_levelUI = GetNode<CanvasLayer>("../LevelUI");

		_keysLabel = GetNode<Label>(
		    "../LevelUI/CenterContainer/ObjectivePanel/VBoxContainer/KeysLabel"
		);

		_descriptionLabel = GetNode<Label>(
		    "../LevelUI/CenterContainer/ObjectivePanel/VBoxContainer/DescriptionLabel"
		);

		_keysLabel.Text = $"Encuentra las {RequiredKeys} llaves";

		_descriptionLabel.Text = IsFinalLevel
			? "Reúne todas las llaves para poder escapar."
			: "Reúne todas las llaves para avanzar al siguiente nivel.";

		_levelUI.Show();

		await ToSignal(
			GetTree().CreateTimer(MessageDuration),
			SceneTreeTimer.SignalName.Timeout
		);

		if (!IsInsideTree())
			return;

		_levelUI.Hide();
	}
}
