using Godot;
using SharpHook.Native;
using godot_getnode;


public partial class TypedChar : PanelContainer
{

    [GetNode("%Ctrl")] Label _ctrl;
    [GetNode("%Alt")] Label _alt;
    [GetNode("%Shift")] Label _shift;
    [GetNode("%Key")] Label _key;
    [GetNode("%KeyFn")] Label _keyFn;
    [GetNode("%Icon")] TextureRect _icon;


    [Export]
    public bool _isFunction = false;

    [Export]
    public double Duration = 2;

    public string Text
    {
        get => _key.Text ?? _keyFn.Text ?? "";
        set
        {
            if (_isFunction)
            {
                _keyFn.Text = value;
                _key.Text = "";
            }
            else
            {
                _key.Text = value;
                _keyFn.Text = "";
            }
        }
    }

    public override void _Ready()
    {
        this.GetAnnotatedNodes();
        _ctrl.Visible = false;
        _alt.Visible = false;
        _shift.Visible = false;
        _icon.Visible = false;

        _keyFn.Text = "";
        _key.Text = "";

        var timer = GetTree().CreateTimer((1));
        timer.TimeLeft = Duration;
        timer.Timeout += OnTimeout;
    }


    public bool IsFunction
    {
        get => _isFunction;
        set
        {
            _isFunction = value;
            if (_isFunction)
            {
                _key.Text = "";
            }
            else
            {
                _keyFn.Text = "";
            }
        }
    }

    public void SetIcon(Texture2D texture)
    {
        _icon.Texture = texture;
        _icon.Visible = true;
    }


    public void SetModifierMask(ModifierMask mask, bool includeShift = false)
    {
        if ((mask & ModifierMask.Ctrl) != 0)
            _ctrl.Visible = true;
        if ((mask & ModifierMask.Alt) != 0)
            _alt.Visible = true;
        if (includeShift && (mask & ModifierMask.Shift) != 0)
            _shift.Visible = true;
    }

    private void OnTimeout()
    {
        QueueFree();
    }
}
