using System;
using Godot;

namespace UI
{
    public partial class About : Control
    {
        public override void _Ready() { }

        private void OnBtnBackPressed()
        {
            NavigateToScene("res://Scenes/UI/Welcome.tscn");
        }

        private void NavigateToScene(string scenePath)
        {
            if (GetTree().ChangeSceneToFile(scenePath) != Error.Ok)
            {
                GD.PrintErr($"Failed to load scene: {scenePath}");
            }
        }
    }
}
