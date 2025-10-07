using Godot;
using System;

public partial class Player : CharacterBody2D
{
    // Exports
    [Export] float MaxWalkSpeed = 70;
    [Export] float MaxSprintWalkSpeed = 140;
    [Export] float WalkAcceleration = 500;
    [Export] float WalkDeceleration = 4;
    [Export] float StopSpeed = 20;
    [Export] float MaxJumpForce = 4000;

    PlayerState CurrentState = PlayerState.Walking;

    // States the player can be in (Probably only use walking for this project)
    enum PlayerState
    {
        Walking,
        Swimming,
        Climbing,
    }

    AnimatedSprite2D Sprite;

    bool Jumping;
    float CurrJumpForce;

    public override void _Ready()
    {
        // GetNode returns a reference to a sprite based on the relative path put in.
        Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (CurrentState)
        {
            case PlayerState.Walking:
                UpdateWalking((float)delta);
                break;
            case PlayerState.Swimming:
                // Implement UpdateSwimming()
                break;
            case PlayerState.Climbing:
                // Implement UpdateClimbing()
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Switch player state, handle potentially state enter/exit logic.
    /// </summary>
    /// <param name="NewState">New state to enter.</param>
    void SwitchState(PlayerState NewState)
    {
        CurrentState = NewState;
    }

    /// <summary>
    /// Updates walking state
    /// </summary>
    /// <param name="delta">The time elapsed since the last frame.</param>
    /// <returns>The updated velocity vector after applying jump mechanics.</returns>
    private void UpdateWalking(float delta)
    {
        // Create temp velocity variable since the X and Y components of 'Velocity' cannot be individually modified
        Vector2 velocity = Velocity;

        // Get direction, 
        float direction = Input.GetAxis("move_left", "move_right");

        // Get max speed based on if sprinting
        float maxSpeedAdjusted = Input.IsActionPressed("sprint") ? MaxSprintWalkSpeed : MaxWalkSpeed;

        // Get the horizontal velocity only
        Vector2 HorizVelocity = new Vector2(velocity.X, 0);

        // Accelerates and apply friction (decelerate) with different magnitudes based on if the player is on the floor or not
        if (IsOnFloor())
        {
            HorizVelocity = Accelerate(HorizVelocity, new Vector2(direction, 0), maxSpeedAdjusted, WalkAcceleration, (float)delta);
            HorizVelocity = Decelerate(HorizVelocity, WalkDeceleration, (float)delta);
        }
        else
        {
            HorizVelocity = Accelerate(HorizVelocity, new Vector2(direction, 0), maxSpeedAdjusted, WalkAcceleration * .5f, (float)delta);
            HorizVelocity = Decelerate(HorizVelocity, WalkDeceleration * .5f, (float)delta);
        }
        velocity.X = HorizVelocity.X;
        velocity = UpdateJumping(velocity, (float)delta);

        // Apply gravity
        velocity -= UpDirection * 9.8f * 50 * (float)delta;

        // Sprite faces velocity direction
        if (Velocity.X != 0)
        {
            Sprite.FlipH = Velocity.X < 0;
        }

        Velocity = velocity;

        MoveAndSlide();
    }

    /// <summary>
    /// Updates the player jump input, allowing for a greater jump the longer the jump action is held
    /// </summary>
    /// <param name="velocity">The current velocity vector of the player (Vector2).</param>
    /// <param name="direction">The current direction the player wishes to move in (Vector2).</param>
    /// <param name="maxSpeed">The max length velocity can accelerate to.</param>
    /// <param name="acceleration">Speed that velocity accelerates to maxSpeed.</param>
    /// <param name="delta">The time elapsed since the last frame.</param>
    /// <returns>The updated velocity vector after applying jump mechanics.</returns>
    Vector2 Accelerate(Vector2 velocity, Vector2 direction, float maxSpeed, float acceleration, float delta)
    {
        float speed = velocity.Length();
        float addSpeed = Mathf.Clamp(maxSpeed - speed, 0, acceleration * delta);
        velocity += addSpeed * direction;
        return velocity;
    }

    /// <summary>
    /// Updates the player jump input, allowing for a greater jump the longer the jump action is held
    /// </summary>
    /// <param name="velocity">The current velocity vector of the player (Vector2).</param>
    /// <param name="deceleration">Decelerates vector, completing zeroing velocity once it decelerates to StopSpeed.</param>
    /// <param name="delta">The time elapsed since the last frame.</param>
    /// <returns>The updated velocity vector after applying jump mechanics.</returns>
    Vector2 Decelerate(Vector2 velocity, float deceleration, float delta)
    {
        float speed = velocity.Length();
        if (speed != 0)
        {
            float control = Mathf.Max(StopSpeed, speed);
            float drop = control * deceleration * (float)delta;
            velocity *= Mathf.Max(speed - drop, 0) / speed;
        }
        return velocity;
    }


    /// <summary>
    /// Updates the player jump input. The longer the player the holds jump, the more jump velocity is applied.
    /// </summary>
    /// <param name="velocity">The current velocity vector of the player (Vector2).</param>
    /// <param name="delta">The time elapsed since the last frame.</param>
    /// <returns>The updated velocity vector after applying jump mechanics.</returns>
    Vector2 UpdateJumping(Vector2 velocity, float delta)
    {
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            CurrJumpForce = MaxJumpForce;
            Jumping = true;
        }
        if (Input.IsActionJustReleased("jump"))
        {
            Jumping = false;
        }

        if (Jumping)
        {
            velocity += UpDirection * CurrJumpForce * (float)delta;
            CurrJumpForce = Mathf.Lerp(CurrJumpForce, 0, (float)delta * 15);
        }
        return velocity;
    }
}
