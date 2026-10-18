using Godot;

// Perro: Idle -> Windup (ladra, carga 1s y memoriza dónde oyó el sonido)
//        -> Airborne (salto en arco hacia ese punto, con distancia máxima)
//        -> Sliding (cae, se frena y pasa de largo) -> Lost (te pierde) -> Idle
public partial class Dog : CharacterBody2D
{
	private enum State { Idle, Windup, Airborne, Sliding, Lost }

	[ExportGroup("Detección (en píxeles globales)")]
	// Distancia a la que te detecta sin eco. Menor que tu luz base (~185) para que alcances a verlo primero.
	[Export] public float DetectionRadius = 150f;

	[ExportGroup("Ataque")]
	[Export] public float WindupTime = 1.0f;   // ladra y carga
	[Export] public float LostTime = 2.5f;     // tiempo que tarda en volver a poder detectarte

	[ExportGroup("Salto")]
	[Export] public float MaxJumpDistance = 600f;     // límite: si el sonido está más lejos, salta solo hasta aquí
	[Export] public float JumpSpeed = 1100f;          // velocidad en el aire
	[Export] public float JumpHeight = 16f;           // altura del arco (px LOCALES del perro; con escala x5 se ve x5)
	[Export] public float LandingSpeedFactor = 0.7f;  // al caer conserva este % de la velocidad
	[Export] public float SlideDeceleration = 2000f;  // qué tan rápido se frena (más bajo = pasa más de largo)

	private State _state = State.Idle;
	private Player _player;
	private float _timer;

	private Vector2 _soundPos;    // dónde ocurrió el sonido (donde estabas al detectarte)
	private Vector2 _jumpDir;
	private float _jumpDistance;
	private float _airTime;
	private float _airElapsed;
	private float _heightScale = 1f;

	// Opcionales: si no existen en la escena, simplemente se ignoran
	private AnimatedSprite2D _sprite;
	private AudioStreamPlayer2D _bark;
	private Node2D _visual;       // lo que se eleva durante el salto
	private float _visualBaseY;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_bark = GetNodeOrNull<AudioStreamPlayer2D>("Bark");

		_visual = GetNodeOrNull<Node2D>("AnimatedSprite2D") ?? GetNodeOrNull<Node2D>("Placeholder");
		_visualBaseY = _visual?.Position.Y ?? 0f;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		_player ??= GetTree().GetFirstNodeInGroup("player") as Player;

		switch (_state)
		{
			case State.Idle:     ProcessIdle();        break;
			case State.Windup:   ProcessWindup(dt);    break;
			case State.Airborne: ProcessAirborne(dt);  break;
			case State.Sliding:  ProcessSliding(dt);   break;
			case State.Lost:     ProcessLost(dt);      break;
		}
	}

	// ---------- IDLE ----------
	private void ProcessIdle()
	{
		Velocity = Vector2.Zero;
		if (_player == null) return;

		float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);

		bool tooClose = dist <= DetectionRadius;
		// Si usas el eco y el perro queda dentro de tu luz agrandada, te nota
		bool caughtInEcho = _player.IsEchoActive && dist <= _player.LightRadius;

		if (tooClose || caughtInEcho)
			StartWindup();
	}

	// ---------- WINDUP: ladra, carga y memoriza el punto del sonido ----------
	private void StartWindup()
	{
		_soundPos = _player.GlobalPosition;  // no te sigue: va a donde oyó el sonido
		_state = State.Windup;
		_timer = WindupTime;
		_bark?.Play();
		PlayAnim("bark");
	}

	private void ProcessWindup(float dt)
	{
		Velocity = Vector2.Zero;
		Face(_soundPos - GlobalPosition);

		_timer -= dt;
		if (_timer <= 0f) StartJump();
	}

	// ---------- AIRBORNE: salto en arco hacia el sonido ----------
	private void StartJump()
	{
		Vector2 toTarget = _soundPos - GlobalPosition;
		_jumpDir = toTarget.Length() > 1f ? toTarget.Normalized() : Vector2.Right;

		// Límite de distancia: si el sonido está más lejos, aterriza antes
		_jumpDistance = Mathf.Min(toTarget.Length(), MaxJumpDistance);
		_airTime = Mathf.Max(_jumpDistance / JumpSpeed, 0.1f);
		_airElapsed = 0f;
		_heightScale = Mathf.Clamp(_jumpDistance / MaxJumpDistance, 0.35f, 1f);  // saltos cortos = arco más bajo

		_state = State.Airborne;
		PlayAnim("jump");
	}

	private void ProcessAirborne(float dt)
	{
		_airElapsed += dt;
		float t = Mathf.Clamp(_airElapsed / _airTime, 0f, 1f);

		// Parábola: 0 al despegar, máxima a la mitad, 0 al aterrizar
		SetHeight(4f * JumpHeight * _heightScale * t * (1f - t));

		Velocity = _jumpDir * JumpSpeed;
		MoveAndSlide();

		if (CheckPlayerHit()) return;
		if (_airElapsed >= _airTime) Land();
	}

	// ---------- SLIDING: cae, se frena y pasa de largo ----------
	private void Land()
	{
		SetHeight(0f);
		Velocity = _jumpDir * JumpSpeed * LandingSpeedFactor;
		_state = State.Sliding;
		PlayAnim("land");
	}

	private void ProcessSliding(float dt)
	{
		Velocity = Velocity.MoveToward(Vector2.Zero, SlideDeceleration * dt);
		MoveAndSlide();

		if (CheckPlayerHit()) return;
		if (Velocity.Length() < 10f) EnterLost();
	}

	// ---------- LOST: te pierde ----------
	private void EnterLost()
	{
		SetHeight(0f);
		_state = State.Lost;
		_timer = LostTime;
		Velocity = Vector2.Zero;
		PlayAnim("idle");
	}

	private void ProcessLost(float dt)
	{
		Velocity = Vector2.Zero;
		_timer -= dt;
		if (_timer <= 0f) _state = State.Idle;
	}

	// ---------- Utilidades ----------
	// Si toca al jugador (en el aire o patinando): lo golpea una vez y se detiene
	private bool CheckPlayerHit()
	{
		int count = GetSlideCollisionCount();
		for (int i = 0; i < count; i++)
		{
			if (GetSlideCollision(i).GetCollider() is Player p)
			{
				p.TakeHit();
				EnterLost();
				return true;
			}
		}
		return false;
	}

	private void SetHeight(float h)
	{
		if (_visual != null)
			_visual.Position = new Vector2(_visual.Position.X, _visualBaseY - h);
	}

	private void Face(Vector2 dir)
	{
		if (_sprite != null && dir.X != 0f) _sprite.FlipH = dir.X < 0f;
	}

	private void PlayAnim(string name)
	{
		if (_sprite?.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(name))
			_sprite.Play(name);
	}
}
