using Godot;

// Puerta de salida: necesitas RequiredKeys llaves. Con "interact" (E), si las tienes, termina el nivel.
public partial class ExitDoor : Area2D
{
	[Export] public int RequiredKeys = 2;

	// Siguiente nivel (elige el .tscn en el Inspector). Si lo dejas vacío, solo avisa en la consola.
	[Export(PropertyHint.File, "*.tscn")] public string NextScene = "";

	[Export] public float FadeTime = 1.0f;       // cuánto tarda en ponerse negra la pantalla
	[Export] public float TextHoldTime = 1.5f;   // cuánto se queda el texto antes de cambiar de nivel

	private Player _player;  // el jugador, mientras esté dentro de la zona
	private CanvasLayer _overlay;

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
		if (_player == null) return;
		if (!@event.IsActionPressed("interact")) return;

		GetViewport().SetInputAsHandled();

		if (_player.KeysCollected < RequiredKeys)
		{
			GD.Print($"Puerta cerrada: te faltan llaves ({_player.KeysCollected}/{RequiredKeys})");
			return;
		}

		CompleteLevel();
	}

	// Fundido a negro con "Nivel completado". Se pausa el juego para que nada se mueva.
	private void CompleteLevel()
	{
		GD.Print("¡Nivel completado!");
		GetTree().Paused = true;

		// Capa que se dibuja sobre todo y sigue funcionando aunque el juego esté en pausa
		_overlay = new CanvasLayer { Layer = 100, ProcessMode = ProcessModeEnum.Always };
		AddChild(_overlay);

		var black = new ColorRect { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
		_overlay.AddChild(black);
		black.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		black.Modulate = new Color(1, 1, 1, 0);

		var label = new Label
		{
			Text = "Nivel completado",
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		label.AddThemeFontSizeOverride("font_size", 48);
		_overlay.AddChild(label);
		label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		label.Modulate = new Color(1, 1, 1, 0);

		// 1) pantalla a negro  2) aparece el texto  3) espera  4) siguiente nivel
		Tween tween = _overlay.CreateTween();
		tween.TweenProperty(black, "modulate:a", 1.0f, FadeTime);
		tween.TweenProperty(label, "modulate:a", 1.0f, 0.4f);
		tween.TweenInterval(TextHoldTime);
		tween.TweenCallback(Callable.From(() => OnFadeFinished()));
	}

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
			GD.PushError($"ExitDoor: no existe la escena '{NextScene}'. Vuelve a elegirla en Next Scene.");
		}

		// Sin siguiente nivel (o ruta inválida): quita el fundido para no quedarte con la pantalla negra
		_overlay.QueueFree();
	}

	// Crea la acción "interact" con la tecla E si no existe todavía
	private static void EnsureInteractAction()
	{
		if (InputMap.HasAction("interact")) return;

		InputMap.AddAction("interact");
		InputMap.ActionAddEvent("interact", new InputEventKey { PhysicalKeycode = Key.E });
	}
}
