using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;

	[Export] public float[] LightBoostMultipliers = { 1.5f, 2.5f, 4.0f };
	[Export] public float BoostDuration = 1.0f;
	[Export] public float TransitionTime = 0.15f;

	private AnimatedSprite2D _animatedSprite2D;
	private PointLight2D _visionLight;

	private Vector2 _lastDirection = Vector2.Right;
	private float _baseLightScale = 1.0f;
	private int _chargeIndex = 0;
	private Tween _lightTween;

	// ---- Lo que lee el perro (Dog.cs) ----
	// Radio aproximado (px globales) donde tu luz realmente alumbra, por cada 1.0 de TextureScale.
	// Con tu luz actual (escala 1.5, personaje x5) da ~185 px. Ajústalo a ojo si hace falta.
	[Export] public float LightRadiusPerScale = 123f;

	// true mientras el eco está activo (la luz está más grande que la base)
	public bool IsEchoActive => _visionLight != null && _visionLight.TextureScale > _baseLightScale * 1.01f;

	// Radio actual de tu luz en px globales (crece con el eco)
	public float LightRadius => _visionLight == null ? 0f : _visionLight.TextureScale * LightRadiusPerScale;

	public override void _Ready()
	{
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_visionLight = GetNode<PointLight2D>("PointLight2D");

		_baseLightScale = _visionLight.TextureScale;

		AddToGroup("player");
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

		float targetScale = _baseLightScale * LightBoostMultipliers[_chargeIndex];

		_lightTween = CreateTween();
		_lightTween.TweenProperty(_visionLight, "texture_scale", targetScale, TransitionTime);
		_lightTween.TweenInterval(BoostDuration);
		_lightTween.TweenProperty(_visionLight, "texture_scale", _baseLightScale, TransitionTime * 2);

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
			Velocity = direction * Speed;
			_lastDirection = direction;
		}
		else
		{
			Velocity = Vector2.Zero;
		}
	}

	private void ProcessAnimation(Vector2 direction)
	{
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

	// El perro llama a esto cuando te alcanza. Por ahora solo avisa; aquí irá la vida / game over.
	public void TakeHit()
	{
		GD.Print("¡Te golpearon!");
	}
}
