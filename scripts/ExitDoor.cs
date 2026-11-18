
using Godot;

public partial class ExitDoor : Area2D
{
	[Export] public int RequiredKeys = 2;

	[Export(PropertyHint.File, "*.tscn")]
	public string NextScene = "";

	[Export] public float FadeTime = 1.0f;
	[Export] public float TextHoldTime = 1.5f;

	private Player _player;
	private CanvasLayer _overlay;
	private bool _completingLevel = false;

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
		if (_player == null || _completingLevel)
			return;

		if (!@event.IsActionPressed("interact") || @event.IsEcho())
			return;

		GetViewport().SetInputAsHandled();

		// Comprobar llaves
		if (_player.KeysCollected < RequiredKeys)
		{
			ShowMissingKeysMessage();
			return;
		}

		CompleteLevel();
	}

	// Mostrar mensaje cuando faltan llaves
	private void ShowMissingKeysMessage()
	{
		int missingKeys = RequiredKeys - _player.KeysCollected;

		GD.Print(
			$"Puerta bloqueada: {_player.KeysCollected}/{RequiredKeys} llaves"
		);

		string subtitle = missingKeys == 1
			? "NECESITAS 1 LLAVE"
			: $"NECESITAS {missingKeys} LLAVES";

		string description = missingKeys == 1
			? "Debes encontrar la llave restante para abrir esta puerta y continuar."
			: "Debes encontrar las llaves restantes para abrir esta puerta y continuar.";

		Node scene = GetTree().CurrentScene;

		if (scene == null)
			return;

		// Buscar controlador del tutorial
		TutorialController tutorial = FindTutorialController(scene);

		if (tutorial != null)
		{
			tutorial.ShowMessage(
				"PUERTA BLOQUEADA",
				subtitle,
				description
			);

			return;
		}

		// Buscar controlador de los otros niveles
		LevelController level = FindLevelController(scene);

		if (level != null)
		{
			level.ShowMessage(
				"PUERTA BLOQUEADA",
				subtitle,
				description
			);

			return;
		}

		GD.PushWarning(
            "ExitDoor: no se encontró TutorialController ni LevelController."
		);
	}

	// Buscar TutorialController en cualquier parte de la escena
	private TutorialController FindTutorialController(Node node)
	{
		if (node is TutorialController controller)
			return controller;

		foreach (Node child in node.GetChildren())
		{
			TutorialController found = FindTutorialController(child);

			if (found != null)
				return found;
		}

		return null;
	}

	// Buscar LevelController en cualquier parte de la escena
	private LevelController FindLevelController(Node node)
	{
		if (node is LevelController controller)
			return controller;

		foreach (Node child in node.GetChildren())
		{
			LevelController found = FindLevelController(child);

			if (found != null)
				return found;
		}

		return null;
	}

	// Completar nivel
	private void CompleteLevel()
	{
		if (_completingLevel)
			return;

		_completingLevel = true;

		GD.Print("¡Nivel completado!");

		GetTree().Paused = true;

		_overlay = new CanvasLayer
		{
			Layer = 100,
			ProcessMode = ProcessModeEnum.Always
		};

		AddChild(_overlay);

		// Fondo negro
		var black = new ColorRect
		{
			Color = Colors.Black,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};

		_overlay.AddChild(black);

		black.SetAnchorsAndOffsetsPreset(
			Control.LayoutPreset.FullRect
		);

		black.Modulate = new Color(1, 1, 1, 0);

		// Texto de nivel completado
		var label = new Label
		{
			Text = "Nivel completado",
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};

		label.AddThemeFontSizeOverride("font_size", 48);

		_overlay.AddChild(label);

		label.SetAnchorsAndOffsetsPreset(
			Control.LayoutPreset.FullRect
		);

		label.Modulate = new Color(1, 1, 1, 0);

		// Animación
		Tween tween = _overlay.CreateTween();

		tween.TweenProperty(
			black,
			"modulate:a",
			1.0f,
			FadeTime
		);

		tween.TweenProperty(
			label,
			"modulate:a",
			1.0f,
			0.4f
		);

		tween.TweenInterval(TextHoldTime);

		tween.TweenCallback(
			Callable.From(OnFadeFinished)
		);
	}

	// Cambiar al siguiente nivel
	private void OnFadeFinished()
	{
		GetTree().Paused = false;

		if (!string.IsNullOrEmpty(NextScene))
		{
			if (ResourceLoader.Exists(NextScene))
			{
				GetTree().ChangeSceneToFile(NextScene);
				return;
			}

			GD.PushError(
				$"ExitDoor: no existe la escena '{NextScene}'."
			);
		}

		if (GodotObject.IsInstanceValid(_overlay))
			_overlay.QueueFree();

		_overlay = null;
		_completingLevel = false;
	}

	// Crear acción interact con tecla E
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
