using System;
using Godot;

namespace UI.Base
{
    /// <summary>
    /// Base class for all buttons in the application.
    /// Provides common functionality for buttons.
    /// </summary>
    public abstract partial class BaseButton : Button
    {
        /// <summary>
        /// Navigates to the specified scene.
        /// </summary>
        /// <param name="scenePath">Path to the scene to navigate to.</param>
        /// <returns>True if navigation was successful, false otherwise.</returns>
        protected bool NavigateToScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                GD.PrintErr("Cannot navigate to an empty scene path");
                return false;
            }

            Error error = GetTree().ChangeSceneToFile(scenePath);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to load scene: {scenePath}, Error: {error}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Navigates back to the welcome screen.
        /// </summary>
        protected bool NavigateToWelcome()
        {
            return NavigateToScene("res://Scenes/UI/Welcome.tscn");
        }
    }
}
