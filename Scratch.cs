using System;
using Godot;
using godot_getnode;
using SharpHook;
using SharpHook.Native;
using MB = SharpHook.Native.MouseButton;

// TODO: cleanup and commit
//       - rename scene and script names
// TODO: review and audit SharpHook
// TODO: settings window

public partial class Scratch : Control
{
    [GetNode("%TypedLabels")] private Control typedLabels;
    [GetNode("TypeAudio1")] private AudioStreamPlayer2D typeAudio1;
    [GetNode("TypeAudio2")] public AudioStreamPlayer2D typeAudio2;
    [GetNode("TypeAudio3")] public AudioStreamPlayer2D typeAudio3;

    public Texture2D iconMbLeft = GD.Load<Texture2D>("res://Images/mouse-left.png");
    public Texture2D iconMbRight = GD.Load<Texture2D>("res://Images/mouse-right.png");
    public Texture2D iconMbMid = GD.Load<Texture2D>("res://Images/mouse-mid.png");
    public Texture2D iconMbAny = GD.Load<Texture2D>("res://Images/mouse.png");


    PackedScene _labelScene = GD.Load<PackedScene>("res://typed_char.tscn");

    private bool capsAsCtrl = true;
    private bool capslockDown = false;

    public override void _Ready()
    {
        this.GetAnnotatedNodes();

        var hook = new TaskPoolGlobalHook(1, GlobalHookType.All);
        hook.KeyPressed += OnKeyPressed;
        hook.KeyReleased += OnKeyReleased;
        hook.KeyTyped += OnKeyTyped;
        hook.MousePressed += OnMouseClicked;
        hook.RunAsync();

        GetViewport().TransparentBg = true;

        var scrSize = DisplayServer.ScreenGetSize();
        var winSize = DisplayServer.WindowGetSize();
        DisplayServer.WindowSetPosition(new Vector2I(scrSize.X - winSize.X - 20, scrSize.Y - winSize.Y - 40));
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.MousePassthrough, true);
    }


    private void OnMouseClicked(object sender, MouseHookEventArgs e)
    {
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);
        Callable.From(() =>
        {
            var label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            label.IsControl = true;
            switch (e.Data.Button)
            {
                case MB.Button1: label.SetIcon(iconMbLeft); break;
                case MB.Button2: label.SetIcon(iconMbRight); break;
                case MB.Button3: label.SetIcon(iconMbMid); break;
                default:
                    label.SetIcon(iconMbAny); break;
            }
            label.SetModifierMask(mask, includeShift: true);

        }).CallDeferred();

    }

    private void OnKeyTyped(object sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);

        if (char.IsControl((char)data.KeyCode)) return;

        Callable.From(() =>
        {
            var label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            if ((mask & ModifierMask.Ctrl) != 0 || (capsAsCtrl && capslockDown))
                label.Text = ((char)data.RawCode).ToString();
            else
                label.Text = ((char)data.KeyChar).ToString();
            label.SetModifierMask(mask);
            typeAudio1.Play();
        }).CallDeferred();
    }

    private void OnKeyPressed(object sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);

        switch (data.KeyCode)
        {
            case KeyCode.VcLeftMeta:
            case KeyCode.VcRightMeta:
            case KeyCode.VcPrintScreen:
            case KeyCode.VcScrollLock:
            case KeyCode.VcPause:
            case KeyCode.VcContextMenu:

            case KeyCode.VcHome:
            case KeyCode.VcEnd:
            case KeyCode.VcInsert:
            case KeyCode.VcDelete:
            case KeyCode.VcPageDown:
            case KeyCode.VcPageUp:

            case KeyCode.VcEnter:
            case KeyCode.VcEscape:
            case KeyCode.VcBackspace:
            case KeyCode.VcSpace:
            case KeyCode.VcTab:

            case KeyCode.VcUp:
            case KeyCode.VcDown:
            case KeyCode.VcLeft:
            case KeyCode.VcRight:
            case KeyCode.VcF1:
            case KeyCode.VcF2:
            case KeyCode.VcF3:
            case KeyCode.VcF4:
            case KeyCode.VcF5:
            case KeyCode.VcF6:
            case KeyCode.VcF7:
            case KeyCode.VcF8:
            case KeyCode.VcF9:
            case KeyCode.VcF10:
            case KeyCode.VcF11:
            case KeyCode.VcF12:
            case KeyCode.VcF13:
            case KeyCode.VcF14:
            case KeyCode.VcF15:
            case KeyCode.VcF16:
            case KeyCode.VcF17:
            case KeyCode.VcF18:
            case KeyCode.VcF19:
            case KeyCode.VcF20:
            case KeyCode.VcF21:
            case KeyCode.VcF22:
            case KeyCode.VcF23:
            case KeyCode.VcF24:
                break;

            case KeyCode.VcCapsLock:
                capslockDown = true;
                return;

            default:
                if (!char.IsControl((char)data.KeyCode)) return;
                break;
        }


        Callable.From(() =>
        {
            typeAudio2.Play();

            var label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            label.IsControl = true;
            label.SetModifierMask(mask, includeShift: true);

            // symbol mapping based on http://xahlee.info/comp/unicode_computing_symbols.html
            var code = e.Data.KeyCode;
            switch (code)
            {
                case KeyCode.VcEnter:
                    label.Text = "⏎";
                    break;
                case KeyCode.VcPageUp:
                    label.Text = "▲";
                    break;
                case KeyCode.VcPageDown:
                    label.Text = "▼";
                    break;
                case KeyCode.VcHome:
                    label.Text = "⤒";
                    break;
                case KeyCode.VcEnd:
                    label.Text = "⤓";
                    break;
                case KeyCode.VcEscape:
                    label.Text = "⎋";
                    break;
                case KeyCode.VcBackspace:
                    label.Text = "⌫";
                    break;
                case KeyCode.VcDelete:
                    label.Text = "⌦";
                    break;
                case KeyCode.VcSpace:
                    label.Text = "␣";
                    break;
                case KeyCode.VcInsert:
                    label.Text = "⎀";
                    break;
                case KeyCode.VcPrintScreen:
                    label.Text = "⎙";
                    break;
                case KeyCode.VcContextMenu:
                    label.Text = "▤";
                    break;
                case KeyCode.VcRightMeta:
                case KeyCode.VcLeftMeta:
                    label.Text = "❖";
                    break;

                case KeyCode.VcUp: label.Text = "↑"; break;
                case KeyCode.VcDown: label.Text = "↓"; break;
                case KeyCode.VcLeft: label.Text = "←"; break;
                case KeyCode.VcRight: label.Text = "→"; break;

                default:
                    var s = code.ToString();
                    label.Text = s.Substr(2, s.Length);
                    break;
            }
        }).CallDeferred();
    }

    private ModifierMask SetCtrlCapsMask(ModifierMask mask)
    {
        if (capsAsCtrl && capslockDown)
        {
            return mask | ModifierMask.Ctrl;
        }
        return mask;
    }


    private void OnKeyReleased(object sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode == KeyCode.VcCapsLock)
        {
            capslockDown = false;
        }
    }

}
