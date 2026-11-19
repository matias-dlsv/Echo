
using Godot;

public partial class LevelController : Node
{
	[Export] public int RequiredKeys = 2;
	[Export] public bool IsFinalLevel = false;
	[Export] public float MessageDuration = 4.0f;

	// Mensaje inicial personalizado para cada nivel
	[Export(PropertyHint.MultilineText)]
	public string WelcomeDescription = "";

	private CanvasLayer _levelUI;
	private Control _messageContainer;
	private Label _titleLabel;
	private Label _keysLabel;
	private Label _descriptionLabel;

	private Tween _messageTween;

	public override void _Ready()
	{
		_levelUI = GetNode<CanvasLayer>("../LevelUI");

		_messageContainer = GetNode<Control>(
            "../LevelUI/CenterContainer"
		);

		_titleLabel = GetNode<Label>(
            "../LevelUI/CenterContainer/ObjectivePanel/VBoxContainer/TitleLabel"
		);

		_keysLabel = GetNode<Label>(
            "../LevelUI/CenterContainer/ObjectivePanel/VBoxContainer/KeysLabel"
		);

		_descriptionLabel = GetNode<Label>(
            "../LevelUI/CenterContainer/ObjectivePanel/VBoxContainer/DescriptionLabel"
		);

		_levelUI.Hide();

		CallDeferred(nameof(ShowWelcome));
	}

	private void ShowWelcome()
	{
		_keysLabel.Text = $"Encuentra las {RequiredKeys} llaves";

		if (!string.IsNullOrWhiteSpace(WelcomeDescription))
		{
			_descriptionLabel.Text = WelcomeDescription;
		}
		else
		{
			_descriptionLabel.Text = IsFinalLevel
				? "Reúne todas las llaves para poder escapar."
				: "Reúne todas las llaves para avanzar al siguiente nivel.";
		}

		ShowCurrentMessage();
	}

	public void ShowMessage(string title, string subtitle, string description)
	{
		_titleLabel.Text = title;
		_keysLabel.Text = subtitle;
		_descriptionLabel.Text = description;

		ShowCurrentMessage();
	}

	private void ShowCurrentMessage()
	{
		if (_messageTween != null && _messageTween.IsValid())
			_messageTween.Kill();

		_levelUI.Show();
		_messageContainer.Modulate = Colors.White;

		_messageTween = CreateTween();
		_messageTween.TweenInterval(MessageDuration);
		_messageTween.TweenProperty(
			_messageContainer,
			"modulate:a",
			0.0f,
			0.5f
		);

		_messageTween.TweenCallback(
			Callable.From(() => _levelUI.Hide())
		);
	}
}
