
using Godot;
using System;
using System.Collections.Generic;

public partial class RunePuzzle : CanvasLayer
{
	[Signal]
	public delegate void PuzzleSolvedEventHandler();

	[Signal]
	public delegate void PuzzleClosedEventHandler();

	private readonly TextureRect[] _slots = new TextureRect[3];
	private readonly string[] _selectedRunes = new string[3];
	private readonly Dictionary<string, Texture2D> _textures = new();

	private Button _activateButton;
	private Button _exitButton;
	private Label _instructionLabel;

	private bool _solved = false;

	private static readonly string[] CorrectSequence =
	{
		"eye",
		"chain",
        "nature"
	};

	public bool IsSolved => _solved;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		var slotsContainer = GetNode<HBoxContainer>(
            "PuzzlePanel/ContentMargin/MainVBox/RuneSlots"
		);

		_activateButton = GetNode<Button>(
            "PuzzlePanel/ContentMargin/MainVBox/ActionButtons/ActivateButton"
		);

		_exitButton = GetNode<Button>(
            "PuzzlePanel/ContentMargin/MainVBox/ActionButtons/ExitButton"
		);

		_instructionLabel = GetNode<Label>(
            "PuzzlePanel/ContentMargin/MainVBox/InstructionLabel"
		);

		for (int i = 0; i < 3; i++)
		{
			int index = i;

			Panel panel = slotsContainer.GetNode<Panel>($"Slot{i + 1}");

			var icon = new TextureRect
			{
				MouseFilter = Control.MouseFilterEnum.Ignore,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
			};

			panel.AddChild(icon);
			icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

			_slots[i] = icon;

			panel.MouseFilter = Control.MouseFilterEnum.Stop;

			panel.SetDragForwarding(
				Callable.From<Vector2, Variant>(
					position => SlotDragData(index, position)
				),
				Callable.From<Vector2, Variant, bool>(
					(position, data) => CanDropOnSlot(data)
				),
				Callable.From<Vector2, Variant>(
					(position, data) => DropOnSlot(index, data)
				)
			);
		}

		RegisterRunes("AvailableRunes");
		RegisterRunes("AvailableRunes2");

		_activateButton.Pressed += CheckSequence;
		_exitButton.Pressed += ClosePuzzle;

		UpdateSlots();
	}

	private void RegisterRunes(string containerName)
	{
		var container = GetNode<HBoxContainer>(
			$"PuzzlePanel/ContentMargin/MainVBox/{containerName}"
		);

		foreach (Node child in container.GetChildren())
		{
			if (child is not TextureRect rune)
				continue;

			string id = rune.Name.ToString().ToLowerInvariant();

			if (rune.Texture == null)
				continue;

			_textures[id] = rune.Texture;

			rune.MouseFilter = Control.MouseFilterEnum.Stop;

			rune.SetDragForwarding(
				Callable.From<Vector2, Variant>(
					position => StartRuneDrag(id, rune)
				),
				Callable.From<Vector2, Variant, bool>(
					(position, data) => false
				),
				Callable.From<Vector2, Variant>(
					(position, data) => { }
				)
			);
		}
	}

	private Variant StartRuneDrag(string id, TextureRect rune)
	{
		if (_solved)
			return default;

		var preview = new TextureRect
		{
			Texture = rune.Texture,
			CustomMinimumSize = new Vector2(64, 64),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
		};

		rune.SetDragPreview(preview);

		return Variant.From(id);
	}

	private Variant SlotDragData(int index, Vector2 position)
	{
		if (_solved || string.IsNullOrEmpty(_selectedRunes[index]))
			return default;

		string id = _selectedRunes[index];

		var preview = new TextureRect
		{
			Texture = _textures[id],
			CustomMinimumSize = new Vector2(64, 64),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
		};

		_slots[index].GetParent<Control>().SetDragPreview(preview);

		return Variant.From(id);
	}

	private bool CanDropOnSlot(Variant data)
	{
		if (_solved || data.VariantType != Variant.Type.String)
			return false;

		return _textures.ContainsKey(data.AsString());
	}

	private void DropOnSlot(int index, Variant data)
	{
		if (!CanDropOnSlot(data))
			return;

		string id = data.AsString();

		// Evitar que una misma runa ocupe dos espacios.
		for (int i = 0; i < 3; i++)
		{
			if (i != index && _selectedRunes[i] == id)
				_selectedRunes[i] = null;
		}

		_selectedRunes[index] = id;

		_instructionLabel.Text =
			"Arrastra las runas a los espacios vacíos en el orden correcto.";

		UpdateSlots();
	}

	private void UpdateSlots()
	{
		for (int i = 0; i < 3; i++)
		{
			string id = _selectedRunes[i];

			_slots[i].Texture =
				!string.IsNullOrEmpty(id) && _textures.ContainsKey(id)
					? _textures[id]
					: null;
		}

		_activateButton.Disabled =
			_solved ||
			Array.Exists(_selectedRunes, string.IsNullOrEmpty);
	}

	
	private async void CheckSequence()
	{
		bool correct = true;

		for (int i = 0; i < 3; i++)
		{
			if (_selectedRunes[i] != CorrectSequence[i])
			{
				correct = false;
				break;
			}
		}

		if (!correct)
		{
			_instructionLabel.Text =
				"Secuencia incorrecta. Intenta nuevamente.";
			return;
		}

		_solved = true;
		_instructionLabel.Text = "¡EL SELLO HA SIDO ROTO!";

		UpdateSlots();

		// Avisar a Door10 que el puzle está resuelto.
		EmitSignal(SignalName.PuzzleSolved);

		// Esperar 1,5 segundos para mostrar el resultado.
		await ToSignal(
			GetTree().CreateTimer(1.5, true),
			SceneTreeTimer.SignalName.Timeout
		);

		// Cerrar el popup y reanudar el juego.
		ClosePuzzle();
	}


	private void ClosePuzzle()
	{
		// No borramos _selectedRunes: se conserva el progreso.
		EmitSignal(SignalName.PuzzleClosed);
	}
}
