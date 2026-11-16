
using Godot;

public partial class ExitCompass : Node2D
{
	[Export] public float DistanceFromPlayer = 45f;
	[Export] public float MovementSpeed = 5f;
	[Export] public float PulseSpeed = 4f;
	[Export] public float MinOpacity = 0.10f;
	[Export] public float MaxOpacity = 1f;

	private Sprite2D _glow;
	private Player _player;
	private ExitDoor _exitDoor;
	private PointLight2D _doorGlow;

	[Export] public float DoorGlowDistance = 65f;

	private float _time = 0f;
	private bool _activated = false;

	public override void _Ready()
	{
		_glow = GetNode<Sprite2D>("GlowSprite");
		_player = GetParent<Player>();

		Position = Vector2.Zero;

		// Mantener la luz dentro del sistema del jugador
		_glow.TopLevel = false;
		_glow.ZIndex = 100;
		_glow.Hide();

		GD.Print("ExitCompass inicializada");
	}

	public override void _Process(double delta)
	{
		if (_player == null)
			return;

		// Buscar la salida del nivel
		if (_exitDoor == null ||
			!GodotObject.IsInstanceValid(_exitDoor))
		{
			_exitDoor = FindExitDoor();
		}

		if (_exitDoor == null)
		{
			_glow.Hide();
			return;
		}

		// Mostrar solo al conseguir todas las llaves
		if (_player.KeysCollected < _exitDoor.RequiredKeys)
		{
			_glow.Hide();
			return;
		}
		
		// Detectar cuándo el jugador está cerca de la salida
		float distanceToExit = _player.GlobalPosition.DistanceTo(
			_exitDoor.GlobalPosition
		);

		_doorGlow ??= _exitDoor.GetNodeOrNull<PointLight2D>("DoorGlow");

		if (distanceToExit <= DoorGlowDistance)
		{
			_glow.Hide();

			if (_doorGlow != null)
			{
				_doorGlow.Enabled = true;

				
				// Brillo suave alrededor de la puerta
				_time += (float)delta;

				float doorPulse =
					(Mathf.Sin(_time * 3f) + 1f) / 2f;

				_doorGlow.Energy = Mathf.Lerp(
					0.35f,
					0.85f,
					doorPulse
				);

			}

			return;
		}

		// Al alejarse, volver a la brújula
		if (_doorGlow != null)
			_doorGlow.Enabled = false;

		_glow.Show();

		// Dirección hacia la salida
		Vector2 direction =
			_exitDoor.GlobalPosition -
			_player.GlobalPosition;

		if (direction.LengthSquared() > 0.001f)
			direction = direction.Normalized();
		else
			direction = Vector2.Up;

		// Mantener la luz a 45 unidades del jugador
		Vector2 targetPosition =
			direction * DistanceFromPlayer;

		// Primera activación
		if (!_activated)
		{
			_activated = true;

			_glow.Position = targetPosition;
			_glow.Show();

			GD.Print(
				"COMPASS ACTIVADA | Llaves: ",
				_player.KeysCollected,
				"/",
				_exitDoor.RequiredKeys
			);
		}

		// Movimiento suave siguiendo la salida
		float weight =
			1f - Mathf.Exp(
				-MovementSpeed * (float)delta
			);

		_glow.Position = _glow.Position.Lerp(
			targetPosition,
			weight
		);

		// Destello dorado intermitente
		_time += (float)delta;

		float pulse =
			(Mathf.Sin(_time * PulseSpeed) + 1f) / 2f;

		// Destellos breves en lugar de luz constante
		pulse = Mathf.Pow(pulse, 4f);

		float opacity = Mathf.Lerp(
			MinOpacity,
			MaxOpacity,
			pulse
		);

		// Color dorado
		_glow.Modulate = new Color(
			1f,
			0.78f,
			0.38f,
			opacity
		);

		// La luz crece un poco al destellar
		float scale = Mathf.Lerp(
			0.06f,
			0.12f,
			pulse
		);

		_glow.Scale = new Vector2(scale, scale);
	}

	private ExitDoor FindExitDoor()
	{
		Node scene = GetTree().CurrentScene;

		if (scene == null)
			return null;

		return FindExitDoorRecursive(scene);
	}

	private ExitDoor FindExitDoorRecursive(Node node)
	{
		if (node is ExitDoor exitDoor)
			return exitDoor;

		foreach (Node child in node.GetChildren())
		{
			ExitDoor found =
				FindExitDoorRecursive(child);

			if (found != null)
				return found;
		}

		return null;
	}
}
