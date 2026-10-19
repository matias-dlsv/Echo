using Godot;

// Telaraña: mientras estés dentro de su zona, te mueves mucho más lento.
public partial class Cobweb : Area2D
{
	[Export] public float SlowFactor = 0.1f;  // 0.3 = te mueves al 30% de tu velocidad

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player p) p.EnterSlow(SlowFactor);
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Player p) p.ExitSlow();
	}
}
