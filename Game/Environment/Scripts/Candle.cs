using Godot;
using System;

public class Candle : Area2D
{
    Light2D light;
    Timer time;

    [Export] public float HIGHEST = (float)2;
    [Export] public float LOWEST = (float)1.5;
    enum direction {UP, DOWN};
    direction current = direction.DOWN;
    public override void _Ready()
    {
        light = GetNode("Light2D") as Light2D;
        light.Scale = new Vector2(LOWEST, LOWEST);
        time = GetNode("Timer") as Timer;
        time.Connect("timeout", this, "lightOn");
        time.Autostart = true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(light.Scale.x >= HIGHEST)
        {
            current = direction.DOWN;
        }
        else if(light.Scale.x <= LOWEST)
        {
            current = direction.UP;
        }

        light.Scale = current == direction.DOWN ? 
        new Vector2(light.Scale.x - (float).005, light.Scale.x - (float).005): 
        new Vector2(light.Scale.x + (float).005, light.Scale.x + (float).005);

        light.Energy = current == direction.DOWN ? 
        light.Energy -= (float)0.005: 
        light.Energy += (float)0.005;

    }

    public void lightOff(Area2D area)
    {
        light.Enabled = false;
        time.WaitTime = 4;
        time.Start();
    }

    public void lightOn()
    {
        light.Enabled = true;
    }

}
