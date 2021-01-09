using Godot;
using System;

public class Grass : Sprite
{
    Timer time;

    Area2D hurtBoxArea;
    CollisionShape2D hurtBox;

    [Export] int respawnTimer = 3;

    public override void _Ready()
    {
        time = GetNode("Timer") as Timer;
        hurtBoxArea = GetNode("HurtBox") as Area2D;
        hurtBox = hurtBoxArea.GetNode("HurtBoxArea") as CollisionShape2D;
    }

    public void destroyGrass(Area2D area)
    {
        Visible = false;
        var rand = new Random();
        time.WaitTime = rand.Next(respawnTimer-2, respawnTimer+3);
        time.Start();
        hurtBox.SetDeferred("disabled", true);
    }

    public void respawn()
    {
        Visible  = true;
        hurtBox.SetDeferred("disabled", false);
    }
}
