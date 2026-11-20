
using Godot;

public partial class Crystal : Area2D
{
	[Export]
	public PackedScene PuzzleScene;

	[Export]
	public string[] CorrectSequence = new string[3];

	private Player _player;
	private RunePuzzle _puzzle;
	private PointLight2D _light;

	private bool _activated = false;
	private bool _puzzleOpen = false;

	public bool IsActivated => _activated;

	public override void _Ready()
	{
		_light = GetNode<PointLight2D>("PointLight2D");
		_light.Enabled = false;

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
		if (_player == null || _puzzleOpen || _activated)
			return;

		if (!@event.IsActionPressed("interact") || @event.IsEcho())
			return;

		GetViewport().SetInputAsHandled();

		OpenPuzzle();
	}

	private async void OpenPuzzle()
	{
		if (PuzzleScene == null)
		{
			GD.PushError($"{Name}: falta asignar PuzzleScene.");
			return;
		}

		// Evitar que se abra más de una vez.
		_puzzleOpen = true;

		// Buscar el controlador del nivel.
		LevelController level = GetTree()
			.CurrentScene
			.GetNodeOrNull<LevelController>("LevelController");

		// Mostrar la pista solo al interactuar
		// con el primer cristal del nivel.
		if (level != null && !level.CrystalHintShown)
		{
			level.ShowFirstCrystalHint();

			// Esperar a que termine el mensaje
			// y su animación de desaparición.
			await ToSignal(
				GetTree().CreateTimer(level.MessageDuration + 0.5),
				SceneTreeTimer.SignalName.Timeout
			);

			// Evitar continuar si el cristal fue eliminado.
			if (!IsInsideTree())
				return;
		}

		// Crear el puzzle una sola vez.
		if (_puzzle == null)
		{
			_puzzle = PuzzleScene.Instantiate<RunePuzzle>();

			// Cada cristal mantiene su propia combinación.
			_puzzle.CorrectSequence =
				(string[])CorrectSequence.Clone();

			GetTree().CurrentScene.AddChild(_puzzle);

			_puzzle.PuzzleSolved += OnPuzzleSolved;
			_puzzle.PuzzleClosed += ClosePuzzle;
		}

		// Mostrar el puzzle.
		_puzzleOpen = true;
		_puzzle.Show();

		// Pausar el nivel mientras se resuelve.
		GetTree().Paused = true;
	}

	private void OnPuzzleSolved()
	{
		Activate();
	}

	public void Activate()
	{
		if (_activated)
			return;

		_activated = true;

		// Encender la luz del cristal.
		_light.Enabled = true;

		GD.Print($"{Name}: ¡cristal activado!");
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
