using Godot;
using System;
using System.Security;

public partial class typed_char : PanelContainer
{

    private Label _label;

    [Export]
    public bool _isControl = false;

    [Export]
    public double Duration = 2;

    public string Text
    {
        get => _label.Text ?? "";
        set
        {
            _label.Text = value;
        }
    }
    public bool IsControl
    {
        get => _isControl;
        set
        {
            _isControl = value;
            if (_isControl)
            {
                Modulate = new Color(1, 1, 1, 1.0f);
                _label.Modulate = new Color(1, 0, 0, 1);
            }
            else
            {
                Modulate = new Color(1, 1, 1, 0.0f);
                _label.Modulate = new Color(1, 1, 1, 1);
            }
        }
    }

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");

        var timer = GetTree().CreateTimer((1));
        timer.TimeLeft = Duration;
        timer.Timeout += OnTimeout;
    }

    private void OnTimeout()
    {
        QueueFree();
    }
}
