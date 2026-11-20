
using Godot;
using System;
using System.Collections.Generic;

public partial class RunePuzzle : CanvasLayer
{
	[Signal]
	public delegate void PuzzleSolvedEventHandler();

	[Signal]
	public delegate void PuzzleClosedEventHandler();

	// Combinación configurable desde Godot o desde Crystal/RuneDoor.
	// No hay una combinación ganadora fija.
	[Export]
	public string[] CorrectSequence = new string[3];

	private readonly TextureRect[] _slots = new TextureRect[3];
	private readonly string[] _selectedRunes = new string[3];
	private readonly Dictionary<string, Texture2D> _textures = new();

	private Button _activateButton;
	private Button _exitButton;
	private Label _instructionLabel;

	private bool _solved = false;

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

			Panel panel = slotsContainer.GetNode<Panel>(
				$"Slot{i + 1}"
			);

			var icon = new TextureRect
			{
				MouseFilter = Control.MouseFilterEnum.Ignore,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
			};

			panel.AddChild(icon);
			icon.SetAnchorsAndOffsetsPreset(
				Control.LayoutPreset.FullRect
			);

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

		// Permitimos repetir una runa en varios espacios.
		// Ejemplo: ray, ray, ray.
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
		// Verificar que se configuraron exactamente tres runas.
		if (CorrectSequence == null || CorrectSequence.Length != 3)
		{
			GD.PushError(
                "RunePuzzle: debes configurar exactamente 3 runas ganadoras."
			);

			_instructionLabel.Text =
				"Error: combinación no configurada.";

			return;
		}

		// Verificar que las tres runas tengan nombres válidos.
		for (int i = 0; i < 3; i++)
		{
			string rune = CorrectSequence[i]?.Trim().ToLowerInvariant();

			if (string.IsNullOrEmpty(rune) || !_textures.ContainsKey(rune))
			{
				GD.PushError(
					$"RunePuzzle: la runa ganadora '{CorrectSequence[i]}' " +
                    "no existe entre las runas disponibles."
				);

				_instructionLabel.Text =
					"Error: revisa la combinación configurada.";

				return;
			}
		}

		// Comprobar la combinación seleccionada.
		bool correct = true;

		for (int i = 0; i < 3; i++)
		{
			if (!string.Equals(
				_selectedRunes[i],
				CorrectSequence[i]?.Trim(),
				StringComparison.OrdinalIgnoreCase
			))
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

		// Notificar al objeto que abrió el puzle:
		// RuneDoor desbloquea la puerta.
		// Crystal enciende su luz.
		EmitSignal(SignalName.PuzzleSolved);

		// Mostrar el mensaje antes de cerrar.
		await ToSignal(
			GetTree().CreateTimer(1.5, true),
			SceneTreeTimer.SignalName.Timeout
		);

		ClosePuzzle();
	}

	private void ClosePuzzle()
	{
		// Conservamos la selección si el jugador sale.
		EmitSignal(SignalName.PuzzleClosed);
	}
}
