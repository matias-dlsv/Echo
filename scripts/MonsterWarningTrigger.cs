
using Godot;

public partial class MonsterWarningTrigger : Area2D
{
	private bool _messageShown = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player || _messageShown)
			return;

		TutorialController tutorial = GetTree()
			.CurrentScene.GetNodeOrNull<TutorialController>("TutorialController");

		if (tutorial == null)
			return;

		_messageShown = true;

		tutorial.ShowMessage(
			"¡CUIDADO!",
			"NO ESTÁS SOLO",
            "Hay criaturas que pueden matarte. Si mueres, reaparecerás en tu último checkpoint."
		);
	}
}
