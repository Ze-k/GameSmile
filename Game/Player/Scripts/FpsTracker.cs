using Godot;
using System;

public class FpsTracker : Label
{ 
    public override void _PhysicsProcess(float delta)
    {
        Text = "FPS: " + Godot.Engine.GetFramesPerSecond();
    }
}
