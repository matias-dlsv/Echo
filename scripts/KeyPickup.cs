using Godot;

// Llave: párate sobre ella y aprieta "interact" (E) para recogerla. Al recogerla desaparece.
public partial class KeyPickup : Area2D
{
	private Player _player;  // el jugador, mientras esté dentro de la zona
	private bool _collected;

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
		if (_collected || _player == null) return;
		if (!@event.IsActionPressed("interact")) return;

		_collected = true;
		_player.AddKey();
		GetViewport().SetInputAsHandled();
		QueueFree();  // desaparece
	}

	// Crea la acción "interact" con la tecla E si no existe todavía
	private static void EnsureInteractAction()
	{
		if (InputMap.HasAction("interact")) return;

		InputMap.AddAction("interact");
		InputMap.ActionAddEvent("interact", new InputEventKey { PhysicalKeycode = Key.E });
	}
}
