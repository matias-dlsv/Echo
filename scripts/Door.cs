
using Godot;

public partial class Door : Area2D
{
	[Export] public Node2D Destination;

	// Condiciones configurables
	[Export] public bool RequiresEchoTraining = false;
	[Export] public bool RequiresKey = false;
	[Export] public bool RequiresCrystals = false;

	// Cristales asignados desde el Inspector
	[Export] public Crystal CrystalBlue;
	[Export] public Crystal CrystalYellow;
	[Export] public Crystal CrystalPurple;

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

		GetViewport().SetInputAsHandled();

		// 1. TUTORIAL: mantener comportamiento original
		if (RequiresEchoTraining && !_player.CompletedEchoTraining)
		{
			TutorialController tutorial = GetTree()
				.CurrentScene
				.GetNodeOrNull<TutorialController>("TutorialController");

			tutorial?.ShowMessage(
				"DESCUBRE TU PODER",
				"UTILIZA ECHO",
                "Utiliza tus 3 ECHOS para desbloquear esta puerta."
			);

			return;
		}

		// 2. NIVEL 3: comprobar cristales
		if (RequiresCrystals)
		{
			int activated = 0;

			if (CrystalBlue != null && CrystalBlue.IsActivated)
				activated++;

			if (CrystalYellow != null && CrystalYellow.IsActivated)
				activated++;

			if (CrystalPurple != null && CrystalPurple.IsActivated)
				activated++;

			if (activated < 3)
			{
				ShowLevelMessage(
					"EL PODER DE LOS CRISTALES",
					$"Esta puerta permanece sellada. Has despertado {activated} de 3 cristales.",
                    "Encuentra las runas correctas y despierta los tres cristales para continuar."
				);

				return;
			}
		}

		// 3. NIVELES: comprobar llave
		if (RequiresKey && _player.KeysCollected < 1)
		{
			ShowLevelMessage(
				"LA PUERTA ESTÁ CERRADA",
				"Necesitas una llave para abrir esta puerta.",
                "Explora el templo y encuentra la llave que te permitirá continuar."
			);

			return;
		}

		// 4. Abrir puerta normalmente
		_player.TeleportTo(Destination.GlobalPosition);
	}

	// Mensajes exclusivos de los niveles
	private void ShowLevelMessage(
		string title,
		string message,
		string hint
	)
	{
		LevelController level = GetTree()
			.CurrentScene
			.GetNodeOrNull<LevelController>("LevelController");

		if (level != null)
		{
			level.ShowMessage(title, message, hint);
		}
		else
		{
			GD.Print($"{title}: {message} {hint}");
		}
	}

	private static void EnsureInteractAction()
	{
		if (InputMap.HasAction("interact"))
			return;

		InputMap.AddAction("interact");

		InputMap.ActionAddEvent(
			"interact",
			new InputEventKey
			{
				PhysicalKeycode = Key.E
			}
		);
	}
}
