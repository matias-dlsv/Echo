using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;

	[Export] public float[] LightBoostMultipliers = { 1.5f, 2.5f, 4.0f };
	[Export] public float BoostDuration = 1.0f;
	[Export] public float TransitionTime = 0.15f;
	[Export] public int LoudEchoIndex = 2;  // cuál echo hace ruido (0 = primero, 2 = tercero)

	private AnimatedSprite2D _animatedSprite2D;
	private PointLight2D _visionLight;
	private AudioStreamPlayer _echoSound;

	private Vector2 _lastDirection = Vector2.Right;
	private float _baseLightScale = 1.0f;
	private int _chargeIndex = 0;
	private Tween _lightTween;
	private Vector2 _respawnPosition;
	private int _slowZones;      // cuántas telarañas te están frenando a la vez
	private float _slowFactor = 1f;  // 1 = velocidad normal

	// ---- Lo que lee el perro (Dog.cs) ----
	// Radio aproximado (px globales) donde tu luz realmente alumbra, por cada 1.0 de TextureScale.
	// Con tu luz actual (escala 1.5, personaje x5) da ~185 px. Ajústalo a ojo si hace falta.
	[Export] public float LightRadiusPerScale = 123f;

	// true mientras dura un echo que hace ruido (solo el nivel LoudEchoIndex). Solo ese lo oye el perro.
	public bool IsEchoLoud { get; private set; }

	// Radio actual de tu luz en px globales (crece con el eco)
	public float LightRadius => _visionLight == null ? 0f : _visionLight.TextureScale * LightRadiusPerScale;

	public override void _Ready()
	{
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_visionLight = GetNode<PointLight2D>("PointLight2D");
		_echoSound = GetNodeOrNull<AudioStreamPlayer>("AudioController/EchoSound");

		_baseLightScale = _visionLight.TextureScale;

		AddToGroup("player");
		_respawnPosition = GlobalPosition;  // el punto de inicio es tu primer checkpoint
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("vision_boost"))
		{
			BoostVision();
		}
	}

	private void BoostVision()
	{
		if (_lightTween != null && _lightTween.IsValid())
		{
			_lightTween.Kill();
		}

		bool isLoud = _chargeIndex == LoudEchoIndex;
		IsEchoLoud = isLoud;
		if (isLoud) _echoSound?.Play();

		float targetScale = _baseLightScale * LightBoostMultipliers[_chargeIndex];

		_lightTween = CreateTween();
		_lightTween.TweenProperty(_visionLight, "texture_scale", targetScale, TransitionTime);
		_lightTween.TweenInterval(BoostDuration);
		_lightTween.TweenProperty(_visionLight, "texture_scale", _baseLightScale, TransitionTime * 2);
		_lightTween.TweenCallback(Callable.From(() => { IsEchoLoud = false; }));

		_chargeIndex = (_chargeIndex + 1) % LightBoostMultipliers.Length;
	}

	public override void _PhysicsProcess(double delta)
	{
		ProcessMovement();
		ProcessAnimation(_lastDirection);
		MoveAndSlide();
	}

	private void ProcessMovement()
	{
		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		if (direction != Vector2.Zero)
		{
			Velocity = direction * Speed * _slowFactor;
			_lastDirection = direction;
		}
		else
		{
			Velocity = Vector2.Zero;
		}
	}

	private void ProcessAnimation(Vector2 direction)
	{
		_animatedSprite2D.SpeedScale = _slowFactor;

		if (Velocity != Vector2.Zero)
		{
			PlayAnimation("run", direction);
		}
		else
		{
			PlayAnimation("idle", direction);
		}
	}

	private void PlayAnimation(string prefix, Vector2 dir)
	{
		if (dir.X != 0)
		{
			_animatedSprite2D.FlipH = dir.X < 0;
			_animatedSprite2D.Play(prefix + "_right");
		}
		else if (dir.Y < 0)
		{
			_animatedSprite2D.Play(prefix + "_up");
		}
		else if (dir.Y > 0)
		{
			_animatedSprite2D.Play(prefix + "_down");
		}
	}

	// El perro (o una trampa) llama a esto cuando te alcanza
	public void TakeHit()
	{
		Die();
	}

	// Un Checkpoint llama a esto para guardar dónde reapareces
	public void SetCheckpoint(Vector2 globalPos)
	{
		_respawnPosition = globalPos;
	}

	// Las llaves recogidas se conservan aunque mueras
	public int KeysCollected { get; private set; }

	// Una KeyPickup llama a esto al recogerse
	public void AddKey()
	{
		KeysCollected++;
		GD.Print($"Llaves: {KeysCollected}");
	}

	// Una Cobweb llama a esto al entrar / salir de su zona
	public void EnterSlow(float factor)
	{
		_slowZones++;
		_slowFactor = factor;
	}

	public void ExitSlow()
	{
		_slowZones = Mathf.Max(0, _slowZones - 1);
		if (_slowZones == 0) _slowFactor = 1f;
	}

	// Una Door llama a esto para moverte al otro lado
	public void TeleportTo(Vector2 globalPos)
	{
		GlobalPosition = globalPos;
		Velocity = Vector2.Zero;
		GetNodeOrNull<Camera2D>("Camera2D")?.ResetSmoothing();  // que la cámara no "viaje" hasta ahí
	}

	private void Die()
	{
		// Reinicia el estado del eco
		if (_lightTween != null && _lightTween.IsValid())
		{
			_lightTween.Kill();
		}
		_visionLight.TextureScale = _baseLightScale;
		_echoSound?.Stop();
		IsEchoLoud = false;
		_chargeIndex = 0;

		// Todo lo que esté en el grupo "resettable" (enemigos, trampas) vuelve a su estado inicial
		GetTree().CallGroup("resettable", "ResetToDefault");

		TeleportTo(_respawnPosition);
	}
}
