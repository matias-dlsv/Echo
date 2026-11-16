
using Godot;

public partial class Door : Area2D
{
	[Export] public Node2D Destination;

	// Activar solamente en la primera puerta del tutorial
	[Export] public bool RequiresEchoTraining = false;

	private Player _player;

	public override void _Ready()
	{
		EnsureInteractAction();
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player p)
			_player = p;
	}

	private void OnBodyExited(Node2D body)
	{
		if (body == _player)
			_player = null;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_player == null || Destination == null)
			return;

		if (!@event.IsActionPressed("interact") || @event.IsEcho())
			return;

		// Bloqueo especial del tutorial
		if (RequiresEchoTraining && !_player.CompletedEchoTraining)
		{
			TutorialController tutorial = GetTree()
				.CurrentScene.GetNodeOrNull<TutorialController>("TutorialController");

			tutorial?.ShowMessage(
				"DESCUBRE TU PODER",
				"UTILIZA ECHO",
		        "Utiliza tus 3 ECHOS para desbloquear esta puerta."
			);

			GetViewport().SetInputAsHandled();
			return;
		}

		_player.TeleportTo(Destination.GlobalPosition);
		GetViewport().SetInputAsHandled();
	}

	private static void EnsureInteractAction()
	{
		if (InputMap.HasAction("interact"))
			return;

		InputMap.AddAction("interact");
		InputMap.ActionAddEvent(
			"interact",
			new InputEventKey { PhysicalKeycode = Key.E }
		);
	}
}
