
using Godot;

public partial class Checkpoint : Area2D
{
	[Export] public bool ShowFeedback = true;
	[Export] public float MessageDuration = 2.0f;

	private bool _activated = false;
	private Tween _glowTween;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player)
			return;

		// Siempre conservar el punto de reaparición
		player.SetCheckpoint(GlobalPosition);

		if (_activated)
			return;

		_activated = true;

		GD.Print("Checkpoint guardado");

		Node scene = GetTree().CurrentScene;

		// No mostrar feedback en el tutorial
		if (scene == null || FindController<TutorialController>(scene) != null)
			return;

		// Permite desactivar el feedback para checkpoints específicos
		if (!ShowFeedback)
			return;

		ShowSmallMessage();
		PlayGlowEffect();
	}

	private void ShowSmallMessage()
	{
		CanvasLayer layer = new CanvasLayer
		{
			Layer = 90
		};

		GetTree().CurrentScene.AddChild(layer);

		var panel = new PanelContainer();

		layer.AddChild(panel);

		panel.SetAnchorsPreset(Control.LayoutPreset.TopRight);

		// Posición desde la esquina superior derecha
		panel.OffsetLeft = -260;
		panel.OffsetRight = -20;
		panel.OffsetTop = 20;
		panel.OffsetBottom = 75;

		var style = new StyleBoxFlat
		{
			BgColor = new Color("#17141F"),
			BorderColor = new Color("#D9B56E")
		};

		style.SetBorderWidthAll(1);
		style.SetCornerRadiusAll(8);
		style.SetContentMarginAll(10);

		panel.AddThemeStyleboxOverride("panel", style);

		var label = new Label
		{
			Text = "CHECKPOINT ACTIVADO",
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};

		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeColorOverride(
			"font_color",
			new Color("#D9B56E")
		);

		panel.AddChild(label);

		Tween tween = layer.CreateTween();

		tween.TweenInterval(MessageDuration);
		tween.TweenProperty(panel, "modulate:a", 0.0f, 0.4f);
		tween.TweenCallback(Callable.From(() => layer.QueueFree()));
	}

	private void PlayGlowEffect()
	{
		PointLight2D glow = GetNodeOrNull<PointLight2D>("CheckpointGlow");

		if (glow == null)
			return;

		_glowTween?.Kill();

		glow.Enabled = true;
		glow.Energy = 0.0f;

		_glowTween = CreateTween();

		_glowTween.TweenProperty(glow, "energy", 1.5f, 0.25f);
		_glowTween.TweenProperty(glow, "energy", 0.0f, 1.0f);

		_glowTween.TweenCallback(
			Callable.From(() => glow.Enabled = false)
		);
	}

	private T FindController<T>(Node node) where T : Node
	{
		if (node is T controller)
			return controller;

		foreach (Node child in node.GetChildren())
		{
			T found = FindController<T>(child);

			if (found != null)
				return found;
		}

		return null;
	}
}
