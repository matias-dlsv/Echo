using Godot;

// Checkpoint: al pasar por encima, este punto pasa a ser donde reapareces si mueres.
public partial class Checkpoint : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player p)
		{
			p.SetCheckpoint(GlobalPosition);
			GD.Print("Checkpoint guardado");
		}
	}
}
