using UnityEngine;
using System.Collections;

public class CharacterInput
{
    public Vector2 leftStickInput = Vector2.zero;
    public Vector2 rightStickInput = Vector2.zero;
    public Direction leftStickDown = Direction.Neutral;
    public Direction rightStickDown = Direction.Neutral;
    public Direction leftStick = Direction.Neutral;
    public Direction rightStick = Direction.Neutral;


    public void Clear()
    {
        leftStickInput = Vector2.zero;
        rightStickInput = Vector2.zero;
        leftStick = Direction.Neutral;
        leftStickDown = Direction.Neutral;
        rightStick = Direction.Neutral;
        rightStickDown = Direction.Neutral;
    }
}



public interface IGetInput
{
    void GetInput(ref CharacterInput input);
}



public interface ICharacterState
{
    void OnEnter(CharacterController character);

    ICharacterState HandleInput(CharacterInput input);

    void Update();

    void OnExit();
}



public class Idle : ICharacterState
{
    enum GroundedWalking { standing, walking, running }

    CharacterController character;
    Animator animator;
    Rigidbody2D rigidbody;
    bool canJump = false;
    public static float jumpTimer = 0f;

    GroundedWalking currentWalking;


    public void OnEnter(CharacterController character)
    {
        this.character = character;
        animator = character.GetComponent<Animator>();
        rigidbody = character.GetComponent<Rigidbody2D>();
    }



    public ICharacterState HandleInput(CharacterInput input)
    {
        bool walking = false;

        //If the character is stunned, switch to stunned state.
        if (character.stunned)
        {
            return new Stunned();
        }

        //If the right stick is down this frame, switch to attacking state.
        if (input.rightStickDown != Direction.Neutral)
        {
            return new Attacking(input.rightStickDown);
        }

        //If the left stick is pushed up and the character can jump, switch to Jumping state.
        if (input.leftStickInput.y > ControllerThresholds.JumpThreshold && canJump)
        {
            //Trigger air jump animation if character is airborne
            if (!character.grounded)
            {
                animator.SetTrigger(character.airJumpTrigger);
            }

            //Set the jump direction according to the tilt of the left stick in the x axis
            if (input.leftStickInput.x <= -ControllerThresholds.SideJumpThreshold)
            {
                return new Jumping(JumpDirection.leftUp);
            }
            else if (input.leftStickInput.x > -ControllerThresholds.SideJumpThreshold && input.leftStickInput.x < ControllerThresholds.SideJumpThreshold)
            {
                return new Jumping(JumpDirection.up);
            }
            else if (input.leftStickInput.x >= ControllerThresholds.SideJumpThreshold)
            {
                return new Jumping(JumpDirection.rightUp);
            }
        }
        
        //this check prevents jumping infinitely without letting go of the left stick
        if ((input.leftStick == Direction.Neutral && input.leftStickInput.y < ControllerThresholds.JumpThreshold) || (character.grounded && input.leftStickInput.y < ControllerThresholds.JumpThreshold))
        {
            canJump = true;
        }

        /*
        //If the character is on the ground, walk/run/stand depending on the magnitude of leftStick.x
        if (character.grounded)
        {
            
            if (Mathf.Abs(input.leftStickInput.x) > ControllerThresholds.WalkThreshold)
            {
                walking = true;

                if (Mathf.Abs(input.leftStickInput.x) > ControllerThresholds.RunThreshold)
                {
                    rigidbody.velocity = new Vector2(character.runSpeed * (input.leftStickInput.x > 0 ? 1 : -1), 0f);
                }
                else
                {
                    rigidbody.velocity = new Vector2(character.walkSpeed * (input.leftStickInput.x > 0 ? 1 : -1), 0f);
                }
            }
            else
            {
                walking = false;
                rigidbody.velocity = Vector2.zero;
            }
        }
        //If the character is in the air, AddForce to the rigidbody proportional to the magnitude of leftStick.x
        else
        {
            float leftStickXProp = (input.leftStickInput.x + 1) / 2;
            rigidbody.AddForce(new Vector2(Mathf.Lerp(-character.airMoveSpeed, character.airMoveSpeed, leftStickXProp), 0f));
        }
        
        
        animator.SetBool(character.walkingHash, walking);
        */

        if (input.leftStickInput.x > 0)
        {
            character.SetFacingRight(true);
        }
        else if (input.leftStickInput.x < 0)
        {
            character.SetFacingRight(false);
        }

        return null;
    }



    public void Update()
    {

    }



    public void OnExit()
    {
        animator.SetBool(character.walkingHash, false);
    }
}



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
        */
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



    public ICharacterState HandleInput(CharacterInput input)
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
    }



    public ICharacterState HandleInput(CharacterInput input)
    {
        if (exit)
        {
            return new Idle();
        }
        else if (character.stunned)
        {
            return new Stunned();
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);


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
