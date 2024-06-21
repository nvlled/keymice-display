using System;
using Godot;
using godot_getnode;
using SharpHook;
using SharpHook.Native;
using MB = SharpHook.Native.MouseButton;


// TODO: ctrl+num doesn't work on widows
// TODO: passthrough also doesn't work
//
// TODO: review and audit SharpHook
// TODO: settings window

public partial class Main : Control
{
    [GetNode("%TypedLabels")] private Control typedLabels;
    [GetNode("TypeAudio1")] private AudioStreamPlayer2D typeAudio1;
    [GetNode("TypeAudio2")] public AudioStreamPlayer2D typeAudio2;

    public Texture2D iconMbLeft = GD.Load<Texture2D>("res://Images/mouse-left.png");
    public Texture2D iconMbRight = GD.Load<Texture2D>("res://Images/mouse-right.png");
    public Texture2D iconMbMid = GD.Load<Texture2D>("res://Images/mouse-mid.png");
    public Texture2D iconMbAny = GD.Load<Texture2D>("res://Images/mouse.png");


    PackedScene _labelScene = GD.Load<PackedScene>("res://TypedChar.tscn");

    private bool capsAsCtrl = true;
    private bool capslockDown = false;

    public override void _Ready()
    {
        this.GetAnnotatedNodes();

        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);

        var hook = new TaskPoolGlobalHook(1, GlobalHookType.All);
        hook.KeyPressed += OnKeyPressed;
        hook.KeyReleased += OnKeyReleased;
        hook.KeyTyped += OnKeyTyped;
        hook.MousePressed += OnMouseClicked;
        hook.MouseWheel += OnMouseWheel;
        hook.RunAsync();

        GetViewport().TransparentBg = true;

        Callable.From(() =>
        {
            var scrSize = DisplayServer.ScreenGetSize();
            var winSize = DisplayServer.WindowGetSize();
            DisplayServer.WindowSetPosition(new Vector2I(scrSize.X - winSize.X - 20, scrSize.Y - winSize.Y - 40));
            var win = GetWindow();
            win.Unfocusable = false;
        }).CallDeferred();
    }

    private void OnMouseWheel(object sender, MouseWheelHookEventArgs e)
    {
        GD.PrintT("wheel", e.Data);
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);
        Callable.From(() =>
        {
            var label = _labelScene.Instantiate<TypedChar>();
            typedLabels.AddChild(label);
            label.IsFunction = true;
            label.SetIcon(iconMbAny);
            label.Text = e.Data.Rotation > 0 ? "↑" : "↓";
            label.SetModifierMask(mask, includeShift: true);

        }).CallDeferred();
    }

    private void OnMouseClicked(object sender, MouseHookEventArgs e)
    {
        GD.PrintT("mouse", e.Data);
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);
        Callable.From(() =>
        {
            var label = _labelScene.Instantiate<TypedChar>();
            typedLabels.AddChild(label);
            label.IsFunction = true;
            switch (e.Data.Button)
            {
                case MB.Button1: label.SetIcon(iconMbLeft); break;
                case MB.Button2: label.SetIcon(iconMbRight); break;
                case MB.Button3: label.SetIcon(iconMbMid); break;
                default:
                    label.SetIcon(iconMbAny); break;
            }
            label.SetModifierMask(mask, includeShift: true);

            /*
            // TODO: if click coordinates contains window, show border and toggle unfocusable
            // - changing window properties doesn't seem to have any effect on linux
            var data = e.Data;
            var win = GetWindow();
            if (
                data.X > win.Position.X && data.X < win.Position.X + win.Size.X &&
                data.Y > win.Position.Y && data.Y < win.Position.Y + win.Size.Y)
            {
                GD.PrintT("window click");
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.NoFocus, false);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                win.Unfocusable = false;
                win.Borderless = false;
                win.AlwaysOnTop = false;
                win.AlwaysOnTop = false;
            }
            */

        }).CallDeferred();
    }

    private void OnKeyTyped(object sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);
        var ch = (char)data.KeyCode;

        GD.PrintT("typed", data);

        if (char.IsControl(ch) || ch == ' ') return;

        Callable.From(() =>
        {
            var label = _labelScene.Instantiate<TypedChar>();
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
        GD.PrintT("pressed", data);

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

            var label = _labelScene.Instantiate<TypedChar>();
            typedLabels.AddChild(label);
            label.IsFunction = true;
            label.SetModifierMask(mask, includeShift: true);

            // symbol mapping based on http://xahlee.info/comp/unicode_computing_symbols.html
            var code = e.Data.KeyCode;
            switch (code)
            {
                case KeyCode.VcEnter:
                    label.Text = "⏎";
                    break;
                case KeyCode.VcPageUp:
                    label.Text = "PgUp";
                    break;
                case KeyCode.VcPageDown:
                    label.Text = "PgDown";
                    break;
                case KeyCode.VcHome:
                    label.Text = "Home";
                    break;
                case KeyCode.VcEnd:
                    label.Text = "End";
                    break;
                case KeyCode.VcEscape:
                    label.Text = "Esc";
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
                    label.Text = "Insert";
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
