
using Godot;

public partial class KeyTutorialTrigger : Area2D
{
	private bool _messageShown = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		// Solo se activa cuando entra el jugador
		if (body is not Player || _messageShown)
			return;

		TutorialController tutorial = GetTree()
			.CurrentScene.GetNodeOrNull<TutorialController>("TutorialController");

		if (tutorial == null)
			return;

		_messageShown = true;

		tutorial.ShowMessage(
			"ENCUENTRA LA LLAVE",
			"TU SIGUIENTE OBJETIVO",
		    "Busca la llave y presiona E para recogerla. Necesitarás reunir todas las llaves para abrir la puerta de salida."
		);
	}
}
