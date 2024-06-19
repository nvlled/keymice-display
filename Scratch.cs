using System;
using Godot;
using SharpHook;
using SharpHook.Native;

// TODO: cleanup and commit
//       - rename scene and script names
// TODO: review and audit SharpHook

public partial class Scratch : Control
{
    private AudioStreamPlayer2D typeAudio1;
    public AudioStreamPlayer2D typeAudio2;
    public AudioStreamPlayer2D typeAudio3;

    private Control typedLabels;

    PackedScene _labelScene = GD.Load<PackedScene>("res://typed_char.tscn");

    private bool capsAsCtrl = true;
    private bool capslockDown = false;

    public override void _Ready()
    {
        typedLabels = GetNode<Control>("%TypedLabels");
        typeAudio1 = GetNode<AudioStreamPlayer2D>("TypeAudio1");
        typeAudio2 = GetNode<AudioStreamPlayer2D>("TypeAudio2");
        typeAudio3 = GetNode<AudioStreamPlayer2D>("TypeAudio3");

        var hook = new TaskPoolGlobalHook(1, GlobalHookType.All);
        hook.KeyPressed += OnKeyPressed;
        hook.KeyReleased += OnKeyReleased;
        hook.KeyTyped += OnKeyTyped;
        hook.MousePressed += OnMouseClicked;
        hook.RunAsync();

        //GetViewport().TransparentBg = true;

        var scrSize = DisplayServer.ScreenGetSize();
        var winSize = DisplayServer.WindowGetSize();
        DisplayServer.WindowSetPosition(new Vector2I(scrSize.X - winSize.X, scrSize.Y - winSize.Y - 50));
    }

    private void OnMouseClicked(object sender, MouseHookEventArgs e)
    {
        Callable.From(() =>
        {
            AppendMaskLabels(e.RawEvent.Mask);
            var label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            label.IsControl = true;
            label.Text = e.Data.Button switch
            {
                SharpHook.Native.MouseButton.Button1 => "🖱️L",
                SharpHook.Native.MouseButton.Button2 => "🖱️R",
                SharpHook.Native.MouseButton.Button3 => "🖱️M",
                _ => "🖱️",
            };
        }).CallDeferred();
        GD.PrintT("click", e.Data.Button, e.Data.Clicks);
    }

    private void OnKeyTyped(object sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;
        var mask = e.RawEvent.Mask;

        if (char.IsControl((char)data.KeyCode)) return;

        Callable.From(() =>
        {
            AppendMaskLabels(e.RawEvent.Mask);
            var label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            if ((mask & ModifierMask.Ctrl) != 0 || (capsAsCtrl && capslockDown))
                label.Text = ((char)data.RawCode).ToString();
            else
                label.Text = ((char)data.KeyChar).ToString();
            typeAudio1.Play();
        }).CallDeferred();
    }

    private void OnKeyReleased(object sender, KeyboardHookEventArgs e)
    {
        if (e.Data.KeyCode == KeyCode.VcCapsLock)
        {
            capslockDown = false;
        }
    }

    private void OnKeyPressed(object sender, KeyboardHookEventArgs e)
    {
        var data = e.Data;

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
            AppendMaskLabels(e.RawEvent.Mask, includeShift: true);
            typeAudio2.Play();

            var label = _labelScene.Instantiate<typed_char>();
            var code = e.Data.KeyCode;
            typedLabels.AddChild(label);
            label.IsControl = true;

            // symbol mapping based on http://xahlee.info/comp/unicode_computing_symbols.html
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

    private void AppendMaskLabels(ModifierMask mask, bool includeShift = false)
    {

        typed_char label;
        if ((mask & ModifierMask.Ctrl) != 0 || (capsAsCtrl && capslockDown))
        {
            label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            label.Text = "✲";
            label.IsControl = true;
        }
        if ((mask & ModifierMask.Alt) != 0)
        {
            label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            label.Text = "⎇";
            label.IsControl = true;
        }
        if (includeShift && (mask & ModifierMask.Shift) != 0)
        {
            label = _labelScene.Instantiate<typed_char>();
            typedLabels.AddChild(label);
            label.Text = "Shift";
            label.IsControl = true;
        }
    }


    public override void _Process(double delta)
    {
        base._Process(delta);

        //if (!keyEvents.TryDequeue(out data))
        //{
        //    return;
        //}

        ////GD.PrintT("key typed", data.KeyChar, data.KeyCode, data.KeyCode.ToString());


        //var label = new typed_char();

        //if (char.IsLetterOrDigit(data.KeyChar))
        //{
        //    label.Text = char.ToUpper(data.KeyChar).ToString();
        //}
        //else
        //{
        //    label.Text = data.KeyCode.ToString();
        //    label.IsControl = true;
        //    switch (data.KeyCode)
        //    {
        //        case KeyCode.VcEnter: label.Text = "⏎"; break;

        //        case KeyCode.VcLeftAlt:
        //        case KeyCode.VcRightAlt:
        //            label.Text = "⎇Alt";
        //            break;

        //        case KeyCode.VcLeftShift:
        //        case KeyCode.VcRightShift:
        //            label.Text = "⇧Shift";
        //            break;
        //    }
        //}
        //typedLabels.AddChild(label);
    }


    /*
    private void onFilesDropped(string[] files)
    {
        GD.PrintT("files dropped", files[0]);
    }

    public static void Run(string filePath)
    {
        using (EpubBookRef bookRef = EpubReader.OpenBook(filePath))
        {
            Console.WriteLine("Navigation:");
            foreach (EpubNavigationItemRef navigationItemRef in bookRef.GetNavigation())
            {
                PrintNavigationItem(navigationItemRef, 0);
            }
        }
        Console.WriteLine();
    }

    private static void PrintNavigationItem(EpubNavigationItemRef navigationItemRef, int identLevel)
    {
        Console.Write(new string(' ', identLevel * 2));
        Console.WriteLine(navigationItemRef.Title);
        foreach (EpubNavigationItemRef nestedNavigationItemRef in navigationItemRef.NestedItems)
        {
            PrintNavigationItem(nestedNavigationItemRef, identLevel + 1);
        }
    }
    */
}
