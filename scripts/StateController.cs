using Godot;

// Componente de ESTADO: qué está haciendo el jugador (animación idle/run) y qué lleva (llaves, checkpoint).
public partial class StateController : Node
{
	public int KeysCollected { get; private set; }          // se conservan aunque mueras
	public Vector2 RespawnPosition { get; private set; }    // último checkpoint

	private AnimatedSprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetParent().GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public void SetCheckpoint(Vector2 globalPos)
	{
		RespawnPosition = globalPos;
	}

	public void AddKey()
	{
		KeysCollected++;
		GD.Print($"Llaves: {KeysCollected}");
	}

	// Elige la animación según si te mueves y hacia dónde miras
	public void UpdateAnimation(bool moving, Vector2 direction, float speedScale)
	{
		_sprite.SpeedScale = speedScale;  // más lenta si una telaraña te frena
		PlayAnimation(moving ? "run" : "idle", direction);
	}

	private void PlayAnimation(string prefix, Vector2 dir)
	{
		if (dir.X != 0)
		{
			_sprite.FlipH = dir.X < 0;
			_sprite.Play(prefix + "_right");
		}
		else if (dir.Y < 0)
		{
			_sprite.Play(prefix + "_up");
		}
		else if (dir.Y > 0)
		{
			_sprite.Play(prefix + "_down");
		}
	}
}
