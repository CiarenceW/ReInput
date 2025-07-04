﻿#if SANDBOX
using System;
using System.Linq;

namespace ReActionPlugin.Editor;

partial class ActionPanel : Widget
{
	private ReAction.Action Action
	{
		get; set;
	}

	private ReActionActionsWidget Page
	{
		get; set;
	}

	int Index { get; set; } = -1;

	public ActionPanel(ReAction.Action action, ReActionActionsWidget page) : base(null)
	{
		Page = page;
		Action = action;
		MinimumHeight = 24;
		Cursor = CursorShape.Finger;
	}

	private string GetFriendlyGamepadInput(ReAction.GamepadInput value)
	{
		return DisplayInfo.ForEnumValues<ReAction.GamepadInput>()
			.FirstOrDefault(x => x.value.Equals(value))
			.info.Name;
	}

	private string GetFriendlyKeyCode(ReAction.KeyCode value)
	{
		return DisplayInfo.ForEnumValues<ReAction.KeyCode>()
			.FirstOrDefault(x => x.value.Equals(value))
			.info.Name;
	}

	private float DrawTextWithIcon(Rect r, string text, string icon, int iconSize = 14)
	{
		var textRect = Paint.DrawText(r, text, TextFlag.RightCenter);

		r.Right -= textRect.Width + 4;

		var iconRect = Paint.DrawIcon(r, icon, iconSize, TextFlag.RightCenter);

		return textRect.Width + iconRect.Width + 24;
	}

	protected override void OnPaint()
	{
		if (Index == -1)
			Index = Parent?.Children?.ToList()?.IndexOf(this) ?? -1;

		Paint.ClearPen();

		if (Index % 2 == 0)
		{
			Paint.SetBrush(Color.Black.WithAlpha(0.05f));
			Paint.DrawRect(LocalRect);
		}

		Paint.Antialiasing = true;

		var r = LocalRect;

		Paint.ClearPen();
		Paint.ClearBrush();
		Paint.SetPen(Theme.Text.WithAlpha(Paint.HasMouseOver ? 1f : 0.7f));

		if (Paint.HasMouseOver)
		{
			Paint.DrawIcon(r, "edit", 14, TextFlag.LeftCenter);
			r.Left += 20;
		}

		var name = Action.Name;
		var nameRect = Paint.DrawText(r, name, TextFlag.LeftCenter);

		float width = 0;

		if (Action.SecondaryBind.key != ReAction.KeyCode.KEY_NONE)
		{
			width = DrawTextWithIcon(r, $"{ReAction.FormatModifiersString(Action.SecondaryBind.modifiers)} {GetFriendlyKeyCode(Action.SecondaryBind.key)}", "keyboard_hide");
			r.Right -= width - 8;
		}

		width = DrawTextWithIcon(r, $"{ReAction.FormatModifiersString(Action.Bind.modifiers)} {GetFriendlyKeyCode(Action.Bind.key)}", "keyboard");
		r.Right -= width - 8;

		/*if (Action.GamepadInput != ReAction.GamepadInput.None)
		{
			width = DrawTextWithIcon(r, GetFriendlyGamepadInput(Action.GamepadInput), "sports_esports");
			r.Right -= width - 8;
		}*/
	}

	protected override void OnMouseReleased(MouseEvent e)
	{
		if (e.LeftMouseButton)
		{
			ShowModal();
		}

		if (e.RightMouseButton)
		{
			var m = new ContextMenu(this);
			m.AddOption("Edit", "edit", ShowModal);

			m.AddOption("Duplicate", "file_copy", () =>
			{
				Page.AddAction(Activator.CreateInstance(typeof(ReAction.Action), Action) as ReAction.Action);
			});

			m.AddOption("Delete", "delete", () =>
			{
				Page.RemoveAction(Action);
			});

			m.OpenAtCursor();
		}
	}

	/// <summary>
	/// Display a modal for editing/creating the input action in a window
	/// </summary>
	void ShowModal()
	{
		var d = new Dialog(Page);
		d.DeleteOnClose = true;
		d.Layout = Layout.Column();
		d.Layout.Margin = 16;
		d.Window.Size = new(400, 270);

		var sheet = new ControlSheet();
		sheet.AddObject(Action.GetSerialized());
		d.Layout.Add(sheet);
		d.Layout.AddStretchCell(1);

		var row = d.Layout.AddRow();
		row.Spacing = 8;
		row.AddStretchCell();
		row.Add(new Button("Delete", "delete") { Clicked = () => { Page.RemoveAction(Action); d.Close(); } });
		row.Add(new Button.Primary("Done", "done") { MouseClick = () => { Page.UpdateActionList(); d.Close(); } });

		d.Show();
	}
}
#endif