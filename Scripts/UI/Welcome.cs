using System;
using Godot;

namespace UI
{
    public partial class Welcome : Control
    {
        public override void _Ready()
        {
            GD.Print("Welcome Scene Loaded");
        }

        private void OnBtnKarya1Pressed()
        {
            NavigateToScene("res://Scenes/UI/Karya1.tscn");
        }

        private void OnBtnKarya2Pressed()
        {
            NavigateToScene("res://Scenes/UI/Karya2.tscn");
        }

        private void OnBtnKarya3Pressed()
        {
            NavigateToScene("res://Scenes/UI/Karya3.tscn");
        }

        private void OnBtnAboutPressed()
        {
            NavigateToScene("res://Scenes/UI/About.tscn");
        }

        private void OnBtnGuidePressed()
        {
            NavigateToScene("res://Scenes/UI/Guide.tscn");
        }

        private void OnBtnExitPressed()
        {
            GetTree().Quit();
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
