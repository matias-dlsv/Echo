using Godot;

// Componente del PODER ECHO: va en el nodo PointLight2D del jugador.
// Agranda la luz en 3 niveles y decide cuál de ellos hace ruido (el que oye el perro).
public partial class EchoController : PointLight2D
{
	[Export] public float[] LightBoostMultipliers = { 1.5f, 2.5f, 4.0f };
	[Export] public float BoostDuration = 1.0f;
	[Export] public float TransitionTime = 0.15f;
	[Export] public int LoudEchoIndex = 2;            // cuál echo hace ruido (0 = primero, 2 = tercero)
	[Export] public float LightRadiusPerScale = 123f; // px globales de luz por cada 1.0 de TextureScale (estimación)

	// true mientras dura un echo que hace ruido
	public bool IsEchoLoud { get; private set; }

	// Radio actual de la luz en px globales (crece con el echo)
	public float LightRadius => TextureScale * LightRadiusPerScale;

	private float _baseScale = 1f;
	private int _chargeIndex;
	private Tween _tween;

	public override void _Ready()
	{
		_baseScale = TextureScale;
	}

	// Lanza el siguiente echo. Devuelve true si este echo hace ruido.
	public bool Activate()
	{
		KillTween();

		bool loud = _chargeIndex == LoudEchoIndex;
		IsEchoLoud = loud;

		float target = _baseScale * LightBoostMultipliers[_chargeIndex];

		_tween = CreateTween();
		_tween.TweenProperty(this, "texture_scale", target, TransitionTime);
		_tween.TweenInterval(BoostDuration);
		_tween.TweenProperty(this, "texture_scale", _baseScale, TransitionTime * 2);
		_tween.TweenCallback(Callable.From(() => { IsEchoLoud = false; }));

		_chargeIndex = (_chargeIndex + 1) % LightBoostMultipliers.Length;
		return loud;
	}

	// Vuelve todo al estado inicial (al morir)
	public void ResetEcho()
	{
		KillTween();
		TextureScale = _baseScale;
		IsEchoLoud = false;
		_chargeIndex = 0;
	}

	private void KillTween()
	{
		if (_tween != null && _tween.IsValid())
		{
			_tween.Kill();
		}
	}
}
