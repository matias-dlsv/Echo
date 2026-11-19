
using Godot;

public partial class RuneDoor : Area2D
{
	[Export] public Node2D Destination;

	[Export]
	public PackedScene PuzzleScene;

	private Player _player;
	private RunePuzzle _puzzle;

	private bool _unlocked = false;
	private bool _puzzleOpen = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
			_player = player;
	}

	private void OnBodyExited(Node2D body)
	{
		if (body == _player)
			_player = null;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_player == null || _puzzleOpen)
			return;

		if (!@event.IsActionPressed("interact") || @event.IsEcho())
			return;

		GetViewport().SetInputAsHandled();

		// Si ya resolvió el puzle, funciona como una puerta normal.
		if (_unlocked)
		{
			if (Destination != null)
				_player.TeleportTo(Destination.GlobalPosition);

			return;
		}

		OpenPuzzle();
	}

	private void OpenPuzzle()
	{
		if (PuzzleScene == null)
		{
			GD.PushError("RuneDoor: falta asignar PuzzleScene.");
			return;
		}

		// Crear el popup una sola vez para conservar el progreso.
		if (_puzzle == null)
		{
			_puzzle = PuzzleScene.Instantiate<RunePuzzle>();

			GetTree().CurrentScene.AddChild(_puzzle);

			_puzzle.PuzzleSolved += OnPuzzleSolved;
			_puzzle.PuzzleClosed += ClosePuzzle;
		}

		_puzzleOpen = true;
		_puzzle.Show();

		// Congelar jugador, enemigos y resto del nivel.
		GetTree().Paused = true;
	}

	private void OnPuzzleSolved()
	{
		_unlocked = true;

		GD.Print("RuneDoor: sello resuelto, puerta desbloqueada.");

		// Teletransportar al jugador al destino de Door10.
		if (_player != null && Destination != null)
		{
			_player.TeleportTo(Destination.GlobalPosition);
		}
	}

	private void ClosePuzzle()
	{
		if (_puzzle != null)
			_puzzle.Hide();

		_puzzleOpen = false;

		// Reanudar el nivel.
		GetTree().Paused = false;
	}

	public override void _ExitTree()
	{
		if (_puzzle != null && GodotObject.IsInstanceValid(_puzzle))
		{
			_puzzle.PuzzleSolved -= OnPuzzleSolved;
			_puzzle.PuzzleClosed -= ClosePuzzle;
		}

		if (_puzzleOpen)
			GetTree().Paused = false;
	}
}
