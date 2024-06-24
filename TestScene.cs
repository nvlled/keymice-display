using Godot;
using godot_getnode;


public partial class TestScene : Control
{

    [GetNode("%Name")]
    public Label _label;

    [GetNode("%Submit")]
    public Button _submit;

    [GetNode("x", AllowNull: true)]
    public Button _button;


    public override void _Ready()
    {
        this.GetAnnotatedNodes();

        GD.PrintT("label", _label);
        _label.Text = "aha";
        _submit.Text = "submit";

        if (_button is not null)
            _button.Text = "cancel";
    }
}
