using Godot;

// Componente de FÍSICA: mueve el cuerpo del jugador, aplica la lentitud de las telarañas y teletransporta.
public partial class PhysicsController : Node
{
	[Export] public float Speed = 300.0f;

	public Vector2 LastDirection { get; private set; } = Vector2.Right;
	public float SlowFactor { get; private set; } = 1f;      // 1 = velocidad normal
	public bool IsMoving => _body.Velocity != Vector2.Zero;

	private CharacterBody2D _body;
	private int _slowZones;  // cuántas telarañas te están frenando a la vez

	public override void _Ready()
	{
		_body = GetParent<CharacterBody2D>();
	}

	// Calcula la velocidad según la dirección pedida
	public void SetDirection(Vector2 direction)
	{
		if (direction != Vector2.Zero)
		{
			_body.Velocity = direction * Speed * SlowFactor;
			LastDirection = direction;
		}
		else
		{
			_body.Velocity = Vector2.Zero;
		}
	}

	// Mueve el cuerpo y lo desliza contra paredes
	public void Move()
	{
		_body.MoveAndSlide();
	}

	public void EnterSlow(float factor)
	{
		_slowZones++;
		SlowFactor = factor;
	}

	public void ExitSlow()
	{
		_slowZones = Mathf.Max(0, _slowZones - 1);
		if (_slowZones == 0) SlowFactor = 1f;
	}

	public void TeleportTo(Vector2 globalPos)
	{
		_body.GlobalPosition = globalPos;
		_body.Velocity = Vector2.Zero;
		_body.GetNodeOrNull<Camera2D>("Camera2D")?.ResetSmoothing();  // que la cámara no "viaje" hasta ahí
	}
}
