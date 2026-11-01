using Godot;

// Componente de ENTRADA: lee las teclas. No mueve nada ni decide qué hacer con ellas.
public partial class InputController : Node
{
	// Se emite cuando aprietas el botón del echo
	[Signal] public delegate void EchoPressedEventHandler();

	// Dirección que estás pidiendo (izquierda/derecha/arriba/abajo)
	public Vector2 Direction => Input.GetVector("left", "right", "up", "down");

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("vision_boost"))
			EmitSignal(SignalName.EchoPressed);
	}
}
