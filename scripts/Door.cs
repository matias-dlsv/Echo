using Godot;

// Puerta: si estás dentro de su zona y aprietas "interact" (E), apareces en Destination.
// Pon un Marker2D al otro lado y arrástralo al campo Destination en el Inspector.
public partial class Door : Area2D
{
	[Export] public Node2D Destination;

	private Player _player;  // el jugador, mientras esté dentro de la zona

	public override void _Ready()
	{
		EnsureInteractAction();
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player p) _player = p;
	}

	private void OnBodyExited(Node2D body)
	{
		if (body == _player) _player = null;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_player == null || Destination == null) return;
		if (!@event.IsActionPressed("interact")) return;

		_player.TeleportTo(Destination.GlobalPosition);
		GetViewport().SetInputAsHandled();
	}

	// Crea la acción "interact" con la tecla E si no la agregaste en Project Settings > Input Map.
	// Si ya existe, esto no hace nada.
	private static void EnsureInteractAction()
	{
		if (InputMap.HasAction("interact")) return;

		InputMap.AddAction("interact");
		InputMap.ActionAddEvent("interact", new InputEventKey { PhysicalKeycode = Key.E });
	}
}
