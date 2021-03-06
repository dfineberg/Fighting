﻿using UnityEngine;
using System.Collections;

public class CharacterInstructions
{
    public enum MoveInstructions
    {
        left, neutral, right
    }

    public enum ActionInstructions
    {
        none, attack, jump, dash
    }

    public MoveInstructions moveInstructions = MoveInstructions.neutral;
    public ActionInstructions actionInstructions = ActionInstructions.none;
    public Direction direction = Direction.Neutral;
    public bool actedOn = false;

    public void Clear()
    {
        moveInstructions = MoveInstructions.neutral;
        actionInstructions = ActionInstructions.none;
        direction = Direction.Neutral;
        actedOn = false;
    }
}



public interface IGetInput
{
    void GetInput(ref CharacterInstructions instructions);
}



public interface ICharacterState
{
    void OnEnter(CharacterController character);

    ICharacterState HandleInput(CharacterInstructions instructions);

    void Update();

    void OnExit();
}



public class Idle : ICharacterState
{
    CharacterController character;
    Animator animator;
    Rigidbody2D rigidbody;
    bool canJump = false;
    bool jumpThisFrame = false;
    public static float jumpTimer = 0f;

    public void OnEnter(CharacterController character)
    {
        this.character = character;
        animator = character.GetComponent<Animator>();
        rigidbody = character.GetComponent<Rigidbody2D>();
    }



    public ICharacterState HandleInput(CharacterInstructions instructions)
    {
        if (character.stunned)
        {
            return new Stunned();
        }

        if(instructions.actionInstructions == CharacterInstructions.ActionInstructions.attack)
        {
            instructions.actedOn = true;
            return new Attacking(instructions.direction);
        }
        else if(instructions.actionInstructions == CharacterInstructions.ActionInstructions.jump && character.grounded)
        {
            instructions.actedOn = true;
            jumpThisFrame = true;
        }
        else
        {
            jumpThisFrame = false;

            if(instructions.actionInstructions == CharacterInstructions.ActionInstructions.dash)
            {
                instructions.actedOn = true;
                return new Dashing(instructions.direction);
            }
        }

        if (instructions.moveInstructions == CharacterInstructions.MoveInstructions.right)
        {
            character.facingRight = true;
        }
        else if (instructions.moveInstructions == CharacterInstructions.MoveInstructions.left)
        {
            character.facingRight = false;
        }

        return null;
    }



    public void Update()
    {
        if (jumpThisFrame)
        {
            rigidbody.velocity = new Vector2(character.grounded ? rigidbody.velocity.x : 0, character.jumpPower);
        }
    }



    public void OnExit()
    {
        animator.SetBool(character.walkingHash, false);
    }
}


/*
public class Jumping : ICharacterState
{
    
    CharacterController character;
    Rigidbody2D rigidbody;
    JumpDirection jumpDirection;
    float jumpTimer = 0f;


    public Jumping(JumpDirection direction)
    {
        jumpDirection = direction;
    }



    public void OnEnter(CharacterController character)
    {
        this.character = character;
        rigidbody = character.GetComponent<Rigidbody2D>();
        bool resetXVel = false;

        //if the jump is not in the direction that the character is already moving, reset the x velocity to 0
        if ((rigidbody.velocity.x > 0 && jumpDirection != JumpDirection.rightUp) || (rigidbody.velocity.x < 0 && jumpDirection != JumpDirection.leftUp))
        {
            resetXVel = true;
        }

        rigidbody.velocity = new Vector2(resetXVel ? 0 : rigidbody.velocity.x, character.jumpPower);
        rigidbody.gravityScale = 0;
    }



    public ICharacterState HandleInput(CharacterInstructions instructions)
    {
        return null;
    }



    public ICharacterState HandleInput(CharacterInput input)
    {
        if (character.stunned)
        {
            return new Stunned();
        }
        else if (input.rightStickDown != Direction.Neutral)
        {
            return new Attacking(input.rightStickDown);
        }
        //if the character isn't stunned and isn't attacking this frame...
        else
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer <= character.jumpWindow && input.leftStickInput.y > ControllerThresholds.JumpThreshold)
            {
                return null;
            }
            else
            {
                return new Idle();
            }

            /*
            //if jump is still held and the character's y-velocity is positive, we're still jumping...
            if (input.leftStickInput.y > ControllerThresholds.JumpThreshold && rigidbody.velocity.y > 0)
            {
                //...so add air movement force to the character according to the x-axis of the left stick
                float leftStickXProp = (input.leftStickInput.x + 1) / 2;
                Vector2 airMoveForce = new Vector2(Mathf.Lerp(-character.airMoveSpeed, character.airMoveSpeed, leftStickXProp), 0f);
                rigidbody.AddForce(airMoveForce);
                return null;
            }
            //if we've stopped holding jump but the character is still moving upwards, set our y-velocity to zero
            else if (rigidbody.velocity.y > 0)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
            }

            return new Idle();
        
        }
    }



    public void Update()
    {
        
    }



    public void OnExit()
    {
        rigidbody.gravityScale = character.gravityScale;
    }
}
*/


public class Stunned : ICharacterState
{
    CharacterController character;
    Animator animator;

    public void OnEnter(CharacterController character)
    {
        this.character = character;
        animator = character.GetComponent<Animator>();
        animator.SetBool(character.stunnedHash, true);
    }



    public ICharacterState HandleInput(CharacterInstructions instructions)
    {
        if (!character.stunned)
        {
            return new Idle();
        }

        return null;
    }



    public void Update()
    {

    }



    public void OnExit()
    {
        animator.SetBool(character.stunnedHash, false);
    }
}



public class Attacking : ICharacterState
{
    CharacterController character;
    Animator animator;
    int idleHash;
    bool exit = false;
    Direction direction;

    public Attacking(Direction AttackDir)
    {
        direction = AttackDir;
    }

    public void OnEnter(CharacterController character)
    {
        this.character = character;
        character.Attack(direction);
        animator = character.GetComponent<Animator>();
        idleHash = Animator.StringToHash("Idle");
    }



    public ICharacterState HandleInput(CharacterInstructions instructions)
    {
        if (exit || animator.GetCurrentAnimatorStateInfo(0).shortNameHash == idleHash)
        {
            return new Idle();
        }
        else if (character.stunned)
        {
            return new Stunned();
        }

        return null;
    }



    public void EndAttack()
    {
        exit = true;
    }



    public void Update()
    {

    }



    public void OnExit()
    {

    }
}



public class Dashing : ICharacterState
{
    enum State { startup, dash, recovery, end }
    State currentState = State.startup;
    CharacterController character;
    Rigidbody2D rigidbody;
    Direction dashDirection;
    Vector2 dashVector;
    float gravityScale;
    float timer = 0f;
    bool recovery = false;
    bool end = false;

    public Dashing(Direction direction)
    {
        dashDirection = direction;
    }

    public void OnEnter(CharacterController character)
    {
        this.character = character;
        rigidbody = character.GetComponent<Rigidbody2D>();
        dashVector = DirectionUtility.GetVector(dashDirection) * character.dashSpeed;
        gravityScale = rigidbody.gravityScale;
        rigidbody.gravityScale = 0f;
        rigidbody.velocity = Vector2.zero;
    }

    public ICharacterState HandleInput(CharacterInstructions instructions)
    {
        if (character.stunned)
        {
            return new Stunned();
        }

        if(currentState == State.end)
        {
            return new Idle();
        }

        return null;
    }

    public void Update()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case State.startup:

                if(timer >= character.dashStartTime)
                {
                    currentState = State.dash;
                    timer = 0f;
                }
                break;

            case State.dash:

                rigidbody.velocity = dashVector;

                if(timer >= character.dashTime)
                {
                    currentState = State.recovery;
                    timer = 0f;
                }
                break;

            case State.recovery:

                rigidbody.velocity = Vector2.Lerp(dashVector, Vector2.zero, timer / character.dashRecoveryTime);

                if(timer >= character.dashRecoveryTime)
                {
                    rigidbody.velocity = Vector2.zero;
                    currentState = State.end;
                }
                break;
        }

        /*
        if(timer > character.dashTime && !recovery)
        {
            timer = 0f;
            recovery = true;
        }
        else if(timer > character.dashRecoveryTime && recovery)
        {
            end = true;
        }
        else if (recovery)
        {
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, Vector2.zero, Time.deltaTime * character.dashStopSpeed);
        }
        */
    }

    public void OnExit()
    {
        rigidbody.gravityScale = gravityScale;
    }
}
