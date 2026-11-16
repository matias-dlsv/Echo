
using Godot;

public partial class TutorialController : Node
{
	[Export] public float MessageDuration = 4.0f;

	private CanvasLayer _tutorialUI;
	private Control _messageContainer;
	private Label _titleLabel;
	private Label _keysLabel;
	private Label _descriptionLabel;

	private Tween _messageTween;

	public override void _Ready()
	{
		_tutorialUI = GetNode<CanvasLayer>("../TutorialUI");
		_messageContainer = GetNode<Control>("../TutorialUI/CenterContainer");

		_titleLabel = GetNode<Label>(
            "../TutorialUI/CenterContainer/ObjectivePanel/VBoxContainer/TitleLabel"
		);

		_keysLabel = GetNode<Label>(
            "../TutorialUI/CenterContainer/ObjectivePanel/VBoxContainer/KeysLabel"
		);

		_descriptionLabel = GetNode<Label>(
            "../TutorialUI/CenterContainer/ObjectivePanel/VBoxContainer/DescriptionLabel"
		);

		_tutorialUI.Hide();

		CallDeferred(nameof(ShowWelcome));
	}

	private void ShowWelcome()
	{
		ShowMessage(
			"BIENVENIDO A ECHO",
			"TUTORIAL",
	        "Estás atrapado en un templo oscuro. Aprende a utilizar tus poderes para escapar."
		);
	}

	public void ShowMessage(string title, string subtitle, string description)
	{
		if (_messageTween != null && _messageTween.IsValid())
			_messageTween.Kill();

		_titleLabel.Text = title;
		_keysLabel.Text = subtitle;
		_descriptionLabel.Text = description;

		_tutorialUI.Show();
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
			Callable.From(() => _tutorialUI.Hide())
		);
	}
}
