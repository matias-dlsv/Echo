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
		_physics.SetDirection(_input.Direction);
		_state.UpdateAnimation(_physics.IsMoving, _physics.LastDirection, _physics.SlowFactor);
		_physics.Move();
	}

	private void OnEchoPressed()
	{
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

	private void Die()
	{
		_echo.ResetEcho();
		_audio.StopAll();

		// Todo lo que esté en el grupo "resettable" (enemigos, trampas) vuelve a su estado inicial
		GetTree().CallGroup("resettable", "ResetToDefault");

		TeleportTo(_state.RespawnPosition);
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
