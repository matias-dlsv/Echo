
using Godot;

public partial class SpikeTrap : Area2D
{
	[Export] public int FirstDangerousFrame = 2;
	[Export] public int LastDangerousFrame = 3;

	private AnimatedSprite2D _sprite;
	private Player _playerInside;
	private bool _hasTriggered = false;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		_sprite.FrameChanged += CheckTrap;

		AddToGroup("resettable");

		_sprite.Play("default");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			_playerInside = player;
			CheckTrap();
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body == _playerInside)
		{
			_playerInside = null;
			_hasTriggered = false;
		}
	}

	private void CheckTrap()
	{
		if (_playerInside == null || _hasTriggered)
			return;

		int frame = _sprite.Frame;

		bool isDangerous =
			frame >= FirstDangerousFrame &&
			frame <= LastDangerousFrame;

		if (!isDangerous)
			return;

		_hasTriggered = true;

		GD.Print("¡El jugador pisó los pinchos!");

		_playerInside.TakeHit();
	}

	public void ResetToDefault()
	{
		_playerInside = null;
		_hasTriggered = false;

		_sprite.Stop();
		_sprite.Frame = 0;
		_sprite.Play("default");
	}
}
