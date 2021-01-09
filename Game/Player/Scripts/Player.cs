using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    // Initiate player variables
    AnimationTree PlayerAnims;
    Sprite PlayerSprite;
    Sprite PlayerShadow;
    AnimationNodeStateMachinePlayback PlayerStateMachine;
    Area2D PlayerHitBox;
    CollisionShape2D HitBox;
    CollisionShape2D PlayerCollisionBox;
    Timer time;
    Stats stats = new Stats();

    // EXPORTED Stat Variables
    [Export] public int MaxHealth;
    [Export] public int MaxMana;
    
    // EXPORTED Movement Variables && General Movement Variables
    [Export] public float MAX_SPEED;
    [Export] public float SPEED_MULTIPLIER;
    [Export] public float ACCELERATION;
    [Export] public float FRICTION;
    Vector2 velocity = Vector2.Zero;

    // STATE MACHINE enum && General State Machine Variables
    enum states {IDLE, RUN, JUMP, ATTACK, HIT, DEATH};

    // Player state and facing direction
    states currentState = states.IDLE;
    bool FacingDirection;
    Label stateText;
    List<int> playerInventory = new List<int>();

    // INITIATE all variables that need to be readied;
    public override void _Ready()
    {
        stats._Ready(MaxHealth, MaxMana);
        // Connect Nodes with variables
        PlayerAnims = GetNode("AnimationTree") as AnimationTree;
        PlayerSprite = GetNode("PlayerSprite") as Sprite;
        PlayerShadow = GetNode("SmallShadow") as Sprite;
        PlayerHitBox = GetNode("PlayerHitBox") as Area2D;
        PlayerCollisionBox = GetNode("PlayerCollision") as CollisionShape2D;
        HitBox = PlayerHitBox.GetNode("HitBoxArea") as CollisionShape2D;
        stateText = GetNode("StateMachineText") as Label;
        time = GetNode("Timer") as Timer;
        PlayerStateMachine = (AnimationNodeStateMachinePlayback)PlayerAnims.Get("parameters/playback");
        PlayerAnims.Active = true;
        
        HitBox.Disabled = true;
        
    }

    // PHYSICS MAIN LOOP
    public override void _PhysicsProcess(float delta)
    {
        switch(currentState)
        {
            case states.IDLE:
                idleState(delta);
                break;
            case states.RUN:
                runState(delta);
                break;
            case states.JUMP:
                jumpState(delta);
                break;
            case states.ATTACK:
                attackState(delta);
                break;
            case states.HIT:
                break;
            case states.DEATH:
                deathState(delta);
                break;
        }

        if(Input.IsActionJustPressed("changeScene"))
        {
            changeScene();
        }

        stateText.Text = currentState.ToString();

        velocity = MoveAndSlide(velocity);
    }

    // STATE MACHINE FUNCTIONS
    public void idleState(float delta)
    {
        velocity = velocity.MoveToward(Vector2.Zero, FRICTION * delta);
        Vector2 current_direciton = returnDirection();

        if(current_direciton != Vector2.Zero) {     currentState = states.RUN;  PlayerStateMachine.Travel("Run");    }


        // Check for State Machine Inputs
        if(Input.IsActionJustPressed("attack"))
        {
            currentState = states.ATTACK;
            PlayerStateMachine.Travel("Attack");
        }
        if(Input.IsActionPressed("jump"))
        {
            currentState = states.JUMP;
            PlayerStateMachine.Travel("Jump");
            time.WaitTime = (float)0.6;
            time.Start();
        }
    }

    public void runState(float delta)
    {
        Vector2 current_direction = returnDirection();

        if(current_direction != Vector2.Zero) 
        {
            velocity = velocity.MoveToward(current_direction * MAX_SPEED, ACCELERATION * delta); 
            if(current_direction.x != 0)
            {
                PlayerSprite.FlipH = current_direction.x < 0;
                PlayerShadow.GlobalPosition = current_direction.x < 0 ? new Vector2(GlobalPosition.x + 1, GlobalPosition.y) : new Vector2(GlobalPosition.x-(float)1.5, GlobalPosition.y);
                //Console.WriteLine(PlayerShadow.GlobalPosition);
                FacingDirection = current_direction.x < 0;
                PlayerHitBox.RotationDegrees = FacingDirection == true ? 0 : 180;
            } 
            
        }
        else
        { 
            velocity = velocity.MoveToward(Vector2.Zero, FRICTION * delta); 
            if(velocity == Vector2.Zero)
            {   
                currentState = states.IDLE;     
                PlayerStateMachine.Travel("Idle");  
            } 
        }

        // Check for state machine inputs
        if(Input.IsActionPressed("attack"))
        {
            velocity = Vector2.Zero;
            currentState = states.ATTACK;
            PlayerStateMachine.Travel("Attack");
        }
        if(Input.IsActionPressed("jump"))
        {
            currentState = states.JUMP;
            PlayerStateMachine.Travel("Jump");
            velocity.x = velocity.x * (float)0.5;
            velocity.y = velocity.y * (float)0.5;
            time.WaitTime = (float)0.6;
            time.Start();
        }
    }

    public void jumpState(float delta)
    {
        PlayerCollisionBox.SetDeferred("disabled", true);
        if(time.TimeLeft == 0)
        {
            currentState = states.IDLE;
            PlayerStateMachine.Travel("Idle");
            jumpStateFinished();
        }

        Vector2 current_direction = returnDirection();
        velocity = velocity.MoveToward((current_direction * MAX_SPEED) * (float)0.5, ACCELERATION * delta);

    }
    public void jumpStateFinished()
    {
        PlayerCollisionBox.SetDeferred("disabled", false);
    }

    public void attackState(float delta)
    {
        HitBox.Disabled = false;
    }

    public void attackAnimationFinished()
    {
        currentState = states.IDLE;
        PlayerStateMachine.Travel("Idle");
        HitBox.Disabled = true;
    }

    public void hitState(float delta)
    {
        
    }

    public void deathState(float delta)
    {
        velocity = Vector2.Zero;
        if(time.TimeLeft == 0)
        {
            Console.WriteLine("Hello");
            stats.Health = 1;
            currentState = states.IDLE;
            PlayerStateMachine.Travel("Idle");
            PlayerCollisionBox.SetDeferred("disabled", false);
        }

    }

    // HELPER FUNCTIONS
    public Vector2 returnDirection()
    {
        Vector2 current_direction = Vector2.Zero;
        current_direction.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        current_direction.y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
        current_direction = current_direction.Normalized();

        return current_direction;
    }

    public void playerHit(Area2D area)
    {
        stats.Health -= 1;
        if(stats.Health <= 0)
        {
            currentState = states.DEATH;
            time.WaitTime = 3;
            time.Start();
            PlayerStateMachine.Travel("Death");
        }
        Console.WriteLine(stats.Health);
    }

    public void changeScene()
    {
        PackedScene world2 = ResourceLoader.Load("res://Worlds/World2.tscn") as PackedScene;
        PackedScene world = ResourceLoader.Load("res://Worlds/World.tscn") as PackedScene;   
        if(GetTree().CurrentScene.Name == "World2")
        {
            GetTree().ChangeScene("res://Worlds/World.tscn");
        }
        else
        {
            GetTree().ChangeScene("res://Worlds/World2.tscn");
        }    
    }

}
