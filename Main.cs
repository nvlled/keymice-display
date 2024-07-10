using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [GetNode("TypeAudio3")] public AudioStreamPlayer2D typeAudio3;
    [GetNode] public Timer Timer;

    public Texture2D iconMbLeft = GD.Load<Texture2D>("res://Images/mouse-left.png");
    public Texture2D iconMbRight = GD.Load<Texture2D>("res://Images/mouse-right.png");
    public Texture2D iconMbMid = GD.Load<Texture2D>("res://Images/mouse-mid.png");
    public Texture2D iconMbAny = GD.Load<Texture2D>("res://Images/mouse.png");


    PackedScene _labelScene = GD.Load<PackedScene>("res://TypedChar.tscn");
    private bool capsAsCtrl = true;
    private bool capslockDown = false;
    private long lastWheelFrame = 0;

    private bool editable = false;

    SynchronizationContext _uiSyncContext;

    public override void _Ready()
    {
        this.GetAnnotatedNodes();

        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);

        _uiSyncContext = SynchronizationContext.Current;

        var hook = new SimpleGlobalHook(GlobalHookType.All);
        hook.KeyPressed += OnKeyPressed;
        hook.KeyReleased += OnKeyReleased;
        hook.KeyTyped += OnKeyTyped;
        hook.MouseReleased += OnMouseClicked;
        hook.MouseWheel += OnMouseWheel;
        hook.RunAsync();

        Callable.From(() =>
        {
            var scrSize = DisplayServer.ScreenGetSize();
            var winSize = DisplayServer.WindowGetSize();
            DisplayServer.WindowSetPosition(new Vector2I(scrSize.X - winSize.X - 20, scrSize.Y - winSize.Y - 40));
        }).CallDeferred();

        var win = GetWindow();
        win.Unfocusable = true;
        win.Borderless = true;
        win.AlwaysOnTop = true;
        win.Unresizable = true;
        win.Title = "keyboard&mouse actions display";
    }

    private void OnMouseWheel(object sender, MouseWheelHookEventArgs e)
    {
        var frame = GetTree().GetFrame();
        if (frame - lastWheelFrame < 30)
        {
            return;
        }
        lastWheelFrame = frame;

        var mask = SetCtrlCapsMask(e.RawEvent.Mask);
        Callable.From(() =>
        {
            GD.PrintT("wheel", e.Data.Direction);
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
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);
        Callable.From(() =>
        {
            GD.PrintT("clicked", e.Data.Button);
            if (e.Data.Button > MB.Button5 || e.Data.Button < MB.Button1)
            {
                return;
            }

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
            typeAudio3.Play();

            var data = e.Data;
            var win = GetWindow();
            if (
                data.X > win.Position.X && data.X < win.Position.X + win.Size.X &&
                data.Y > win.Position.Y && data.Y < win.Position.Y + win.Size.Y)
            {
                GD.PrintT("window click");
                editable = !editable;
                win.Unfocusable = !editable;
                win.Borderless = !editable;
                win.Unresizable = !editable;
                //win.AlwaysOnTop = !editable;
                // TODO: add button to show settings window
            }

        }).CallDeferred();
    }

    private void OnKeyTyped(object sender, KeyboardHookEventArgs e)
    {
        var ch = (char)e.Data.KeyCode;

        if (!char.IsControl(ch) && ch != ' ')
        {
            _uiSyncContext.Send(HandleKeyEvent, e);
        }
    }

    private void OnKeyPressed(object sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;

        if (data.KeyCode == KeyCode.VcCapsLock && capsAsCtrl)
        {
            capslockDown = true;
            return;
        }

        if (IsFunctionKey(data.KeyCode))
        {
            _uiSyncContext.Send(HandleKeyEvent, e);
        }
    }

    private void HandleKeyEvent(object obj)
    {
        var e = obj as KeyboardHookEventArgs;
        if (e is null) return;

        GD.PrintT("key", e.Data.KeyCode);

        var data = e.Data;
        var mask = SetCtrlCapsMask(e.RawEvent.Mask);

        var lastChild = GetLastChild();
        if (lastChild is not null && lastChild.keyData == data)
        {
            lastChild.Count += 1;

            if (!IsFunctionKey(data.KeyCode)) typeAudio1.Play();
            else typeAudio2.Play();
        }
        else if (!IsFunctionKey(data.KeyCode))
        {
            var label = _labelScene.Instantiate<TypedChar>();
            typedLabels.AddChild(label);
            if ((mask & ModifierMask.Ctrl) != 0 || (capsAsCtrl && capslockDown))
                label.Text = ((char)data.RawCode).ToString();
            else
                label.Text = data.KeyChar.ToString();
            label.SetModifierMask(mask);
            label.keyData = data;
            typeAudio1.Play();
        }
        else
        {
            var label = _labelScene.Instantiate<TypedChar>();
            typedLabels.AddChild(label);
            label.IsFunction = true;
            label.Text = GetFunctionKeyText(data.KeyCode);
            label.keyData = data;
            label.SetModifierMask(mask, includeShift: true);
            typeAudio2.Play();
        }

        RemoveOlderItems();
    }

    private TypedChar GetLastChild()
    {
        if (typedLabels.GetChildCount() == 0)
            return null;
        return typedLabels.GetChild(-1) as TypedChar;
    }


    private bool IsFull()
    {
        return typedLabels.Size.X > GetWindow().Size.X;
    }


    private static string GetFunctionKeyText(KeyCode code)
    {
        // symbol mapping based on http://xahlee.info/comp/unicode_computing_symbols.html
        switch (code)
        {
            case KeyCode.VcEnter:
                return "⏎";
            case KeyCode.VcPageUp:
                return "PgUp";
            case KeyCode.VcPageDown:
                return "PgDown";
            case KeyCode.VcHome:
                return "Home";
            case KeyCode.VcEnd:
                return "End";
            case KeyCode.VcEscape:
                return "Esc";
            case KeyCode.VcBackspace:
                return "⌫";
            case KeyCode.VcDelete:
                return "⌦";
            case KeyCode.VcSpace:
                return "␣";
            case KeyCode.VcInsert:
                return "Insert";
            case KeyCode.VcPrintScreen:
                return "⎙";
            case KeyCode.VcContextMenu:
                return "▤";
            case KeyCode.VcRightMeta:
            case KeyCode.VcLeftMeta:
                return "❖";

            case KeyCode.VcUp: return "↑";
            case KeyCode.VcDown: return "↓";
            case KeyCode.VcLeft: return "←";
            case KeyCode.VcRight: return "→";

            default:
                var s = code.ToString();
                return s.Substr(2, s.Length);
        }
    }

    private static bool IsFunctionKey(KeyCode code)
    {
        return code switch
        {
            KeyCode.VcLeftMeta or KeyCode.VcRightMeta or KeyCode.VcPrintScreen or KeyCode.VcScrollLock or
            KeyCode.VcPause or KeyCode.VcContextMenu or
            KeyCode.VcHome or KeyCode.VcEnd or KeyCode.VcInsert or KeyCode.VcDelete or KeyCode.VcPageDown or KeyCode.VcPageUp or
            KeyCode.VcEnter or KeyCode.VcEscape or KeyCode.VcBackspace or KeyCode.VcSpace or KeyCode.VcTab or
            KeyCode.VcUp or KeyCode.VcDown or KeyCode.VcLeft or KeyCode.VcRight or
            KeyCode.VcF1 or KeyCode.VcF2 or KeyCode.VcF3 or KeyCode.VcF4 or KeyCode.VcF5 or KeyCode.VcF6 or KeyCode.VcF7 or
            KeyCode.VcF8 or KeyCode.VcF9 or KeyCode.VcF10 or KeyCode.VcF11 or KeyCode.VcF12 or KeyCode.VcF13 or KeyCode.VcF14 or
            KeyCode.VcF15 or KeyCode.VcF16 or KeyCode.VcF17 or KeyCode.VcF18 or KeyCode.VcF19 or KeyCode.VcF20 or KeyCode.VcF21 or
            KeyCode.VcF22 or KeyCode.VcF23 or KeyCode.VcF24 or KeyCode.VcCapsLock => true,
            _ => char.IsControl((char)code),
        };
    }

    private void RemoveOlderItems()
    {
        for (var i = 0; IsFull() && i < 3; i++)
        {
            var child = typedLabels.GetChild(0);
            child.QueueFree();
        }
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
