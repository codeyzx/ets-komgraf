using System;
using Godot;

namespace UI
{
    public partial class Guide : Control
    {
        public override void _Ready()
        {
            // Initialization code here
        }

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
