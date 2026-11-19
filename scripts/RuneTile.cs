
using Godot;

public partial class RuneTile : Area2D
{
	[Export] public Texture2D CarvedTexture;
	[Export] public Texture2D GlowTexture;
	[Export] public float GlowDuration = 2.0f;

	private Sprite2D _sprite;
	private EchoController _echo;
	private Tween _glowTween;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("RuneSprite");
		SetGlowing(false);

		_echo = GetTree().GetFirstNodeInGroup("player")
			?.GetNodeOrNull<EchoController>("PointLight2D");

		if (_echo == null)
		{
			GD.PushWarning("RuneTile: no se encontró EchoController.");
			return;
		}

		_echo.EchoActivated += OnEchoActivated;
	}

	private void OnEchoActivated(Vector2 origin, float radius)
	{
		if (GlobalPosition.DistanceTo(origin) > radius)
			return;

		SetGlowing(true);

		if (_glowTween != null && _glowTween.IsValid())
			_glowTween.Kill();

		_glowTween = CreateTween();
		_glowTween.TweenInterval(GlowDuration);
		_glowTween.TweenCallback(
			Callable.From(() => SetGlowing(false))
		);
	}

	public void SetGlowing(bool glowing)
	{
		if (_sprite == null)
			return;

		_sprite.Texture = glowing ? GlowTexture : CarvedTexture;
	}

	public override void _ExitTree()
	{
		if (_echo != null)
			_echo.EchoActivated -= OnEchoActivated;
	}
}
