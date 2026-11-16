using Godot;

// Jugador = COORDINADOR. No hace el trabajo: conecta sus componentes (nodos hijos) y expone
// la API que usan el resto de scripts (perro, puertas, checkpoints, llaves, telarañas).
//
//   InputController   -> lee las teclas
//   PhysicsController -> mueve el cuerpo, lentitud, teletransporte
//   StateController   -> animaciones, llaves, checkpoint
//   AudioController   -> sonidos
//   PointLight2D      -> (script EchoController) poder echo y su luz
public partial class Player : CharacterBody2D
{
	private InputController _input;
	private PhysicsController _physics;
	private StateController _state;
	private AudioController _audio;
	private EchoController _echo;
	public int EchoCount => _echo?.EchoCount ?? 0;
	public bool CompletedEchoTraining =>
		_echo != null && _echo.CompletedEchoTraining;
	private CanvasLayer _deathUI;
	private ColorRect _deathBackground;
	private Label _deathLabel;
	private bool _isDying = false;
	private bool _tutorialDeathTipShown = false;

	// ---- Lo que leen otros scripts ----
	public bool IsEchoLoud => _echo != null && _echo.IsEchoLoud;
	public float LightRadius => _echo?.LightRadius ?? 0f;
	public int KeysCollected => _state?.KeysCollected ?? 0;

	public override void _Ready()
	{
		_input = Find<InputController>("InputController");
		_physics = Find<PhysicsController>("PhysicsController");
		_state = Find<StateController>("StateController");
		_audio = Find<AudioController>("AudioController");
		_echo = Find<EchoController>("PointLight2D");
		_deathUI = Find<CanvasLayer>("DeathUI");
		_deathBackground = Find<ColorRect>("DeathUI/ColorRect");
		_deathLabel = Find<Label>("DeathUI/DeathLabel");

		if (_deathUI == null || _deathBackground == null || _deathLabel == null)
		{
			SetPhysicsProcess(false);
			return;
		}

		// Inicializar la interfaz de muerte
		_deathUI.Show();

		_deathBackground.Show();
		_deathBackground.Modulate = new Color(1, 1, 1, 0);

		_deathLabel.Hide();

		if (_input == null || _physics == null || _state == null || _audio == null || _echo == null)
		{
			SetPhysicsProcess(false);  // faltan componentes: ya se avisó del error arriba
			return;
		}

		_input.EchoPressed += OnEchoPressed;

		AddToGroup("player");
		_state.SetCheckpoint(GlobalPosition);  // el punto de inicio es tu primer checkpoint
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDying)
		{
			Velocity = Vector2.Zero;
			return;
		}
		_physics.SetDirection(_input.Direction);
		_state.UpdateAnimation(_physics.IsMoving, _physics.LastDirection, _physics.SlowFactor);
		_physics.Move();
	}

	private void OnEchoPressed()
	{
		if (_isDying)
			return;

		bool loud = _echo.Activate();
		if (loud) _audio.PlayEcho();
	}

	// ---- API pública (otros scripts llaman esto) ----

	// El perro (o una trampa) llama a esto cuando te alcanza
	public void TakeHit() => Die();

	// Un Checkpoint guarda dónde reapareces
	public void SetCheckpoint(Vector2 globalPos) => _state.SetCheckpoint(globalPos);

	// Una KeyPickup suma una llave
	public void AddKey() => _state.AddKey();

	// Una Cobweb te frena mientras estás dentro
	public void EnterSlow(float factor) => _physics.EnterSlow(factor);
	public void ExitSlow() => _physics.ExitSlow();

	// Una Door te mueve al otro lado
	public void TeleportTo(Vector2 globalPos) => _physics.TeleportTo(globalPos);

	private async void Die()
	{
		if (_isDying)
			return;

		_isDying = true;

		_echo.ResetEcho();
		_audio.StopAll();

		_deathBackground.Show();
		_deathLabel.Hide();

		// Oscurecer gradualmente la pantalla
		Tween fadeOut = CreateTween();
		fadeOut.TweenProperty(
			_deathBackground,
			"modulate:a",
			1.0f,
			0.5f
		);

		await ToSignal(fadeOut, Tween.SignalName.Finished);

		// Mostrar mensaje de muerte
		_deathLabel.Show();

		await ToSignal(
			GetTree().CreateTimer(1.0),
			SceneTreeTimer.SignalName.Timeout
		);

		// Reiniciar enemigos y trampas
		GetTree().CallGroup("resettable", "ResetToDefault");

		// Volver al último checkpoint
		TeleportTo(_state.RespawnPosition);

		// Ocultar mensaje
		_deathLabel.Hide();

		// Volver a iluminar la pantalla
		Tween fadeIn = CreateTween();
		fadeIn.TweenProperty(
			_deathBackground,
			"modulate:a",
			0.0f,
			0.5f
		);

		await ToSignal(fadeIn, Tween.SignalName.Finished);

		_deathBackground.Hide();
		_isDying = false;

		// Mostrar consejo después de la primera muerte del tutorial
		if (!_tutorialDeathTipShown)
		{
			TutorialController tutorial = GetTree()
				.CurrentScene.GetNodeOrNull<TutorialController>("TutorialController");

			if (tutorial != null)
			{
				_tutorialDeathTipShown = true;

				tutorial.ShowMessage(
					"USA EL ECHO A TU FAVOR",
					"CONSEJO DE SUPERVIVENCIA",
		            "El tercer ECHO genera un sonido que atrae a las criaturas. Úsalo para alejarlas de tu camino y escapar."
				);
			}
		}
	}

	// Busca un componente por nombre y avisa exactamente qué falta
	private T Find<T>(string path) where T : Node
	{
		Node raw = GetNodeOrNull(path);

		if (raw == null)
		{
			GD.PushError($"Player: no existe un nodo hijo llamado '{path}'.");
			return null;
		}
		if (raw is not T node)
		{
			GD.PushError($"Player: el nodo '{path}' existe, pero le falta asignarle el script {typeof(T).Name}.");
			return null;
		}
		return node;
	}
}
