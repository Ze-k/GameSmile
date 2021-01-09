using Godot;
using System;

public class World : Node2D
{
    public void sceneExited()
    {
        QueueFree();
    }
}
