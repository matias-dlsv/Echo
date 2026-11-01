using Godot;

// Componente de AUDIO: reproduce los sonidos del jugador.
// Espera un hijo AudioStreamPlayer llamado "EchoSound" con el sonido del echo en su Stream.
public partial class AudioController : Node
{
	private AudioStreamPlayer2D _echoSound;

	public override void _Ready()
	{
		_echoSound = GetNodeOrNull<AudioStreamPlayer2D>("EchoSound");
	}

	public void PlayEcho()
	{
		_echoSound?.Play();
	}

	public void StopAll()
	{
		_echoSound?.Stop();
	}
}
