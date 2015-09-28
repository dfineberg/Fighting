using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum JumpDirection { leftUp, up, rightUp }

[System.Serializable]
public class Attack
{
    public float moveForce;
    public float damage;
    public float stunTime;
    public float knockbackForce;
    public GameObject projectile;
}

public class CharacterController : MonoBehaviour {

    ICharacterState currentState;
    IGetInput inputComponent;
    CharacterInstructions instructions;
    LinkedList<CharacterInstructions> instructionsBuffer;

    public int playerNo;
    public int inputBufferLength;
    public float accelleration;
    public float runSpeed;
    public float walkSpeed;
    public float gravityScale;
    public float jumpPower;
    public float jumpWindow;
    [Range(0, 1)]
    public float sideJumpDirection;
    
    public Attack upAttack;
    public Attack sideUpAttack;
    public Attack sideAttack;
    public Attack sideDownAttack;
    public Attack downAttack;
    public Collider2D Hitbox1;
    public Collider2D Hitbox2;

    Dictionary<Direction, Attack> attacks;
    Direction currentAttackDirection;
    Attack currentAttack;

    public bool grounded { get; private set; }
    public bool stunned { get; private set; }
    public bool jumping {get; private set; }
    [HideInInspector]
    public bool canMove = true;
    public Vector2 leftStickInput { get; private set; }
    public Vector2 rightStickInput { get; private set; }
    [HideInInspector]
    public bool facingRight;
    float startXScale;

    new Rigidbody2D rigidbody;
    Animator animator;
    public int upAttackTrigger { get; private set; }
    public int sideUpAttackTrigger { get; private set; }
    public int sideAttackTrigger { get; private set; }
    public int sideDownAttackTrigger { get; private set; }
    public int downAttackTrigger { get; private set; }
    public int airJumpTrigger { get; private set; }
    public int groundedHash { get; private set; }
    public int walkingHash { get; private set; }
    public int stunnedHash { get; private set; }

    //*********************************
    // VVVVV MONOBEHAVIOUR EVENTS VVVVV
    //*********************************

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        upAttackTrigger = Animator.StringToHash("UpAttack");
        sideUpAttackTrigger = Animator.StringToHash("SideUpAttack");
        sideAttackTrigger = Animator.StringToHash("SideAttack");
        sideDownAttackTrigger = Animator.StringToHash("SideDownAttack");
        downAttackTrigger = Animator.StringToHash("DownAttack");
        airJumpTrigger = Animator.StringToHash("AirJump");
        groundedHash = Animator.StringToHash("Grounded");
        walkingHash = Animator.StringToHash("Walking");
        stunnedHash = Animator.StringToHash("Stunned");

        startXScale = transform.localScale.x;

        attacks = new Dictionary<Direction, Attack>();
        attacks.Add(Direction.Up, upAttack);
        attacks.Add(Direction.RightUp, sideUpAttack);
        attacks.Add(Direction.Right, sideAttack);
        attacks.Add(Direction.RightDown, sideDownAttack);
        attacks.Add(Direction.Down, downAttack);
        attacks.Add(Direction.LeftDown, sideDownAttack);
        attacks.Add(Direction.Left, sideAttack);
        attacks.Add(Direction.LeftUp, sideUpAttack);

        currentState = new Idle();
        currentState.OnEnter(this);
        inputComponent = GetComponent(typeof(IGetInput)) as IGetInput;
        instructions = new CharacterInstructions();
        instructionsBuffer = new LinkedList<CharacterInstructions>();

        for(int i = 0; i < inputBufferLength; i++)
        {
            instructionsBuffer.AddFirst(new CharacterInstructions());
        }

        grounded = false;
        stunned = false;
        rigidbody.gravityScale = gravityScale;
    }



    void Update()
    {
        //flip the character's x-scale to face left or right
        transform.localScale = new Vector3(facingRight ? startXScale : -startXScale, transform.localScale.y, transform.localScale.z);

        //fetch this frame's input from the input component
        //instructions.Clear();
        //inputComponent.GetInput(ref instructions);

        CharacterInstructions thisFrameInstructions = instructionsBuffer.Last.Value;
        instructionsBuffer.RemoveLast();
        thisFrameInstructions.Clear();
        inputComponent.GetInput(ref thisFrameInstructions);
        instructionsBuffer.AddFirst(thisFrameInstructions);
        bool actionFound = false;

        foreach(CharacterInstructions i in instructionsBuffer)
        {
            if (i.actedOn)
            {
                break;
            }
            else
            {
                if (i.actionInstructions != CharacterInstructions.ActionInstructions.none)
                {
                    instructions = i;
                    actionFound = true;
                    break;
                }
            }
        }

        if (!actionFound)
        {
            instructions = instructionsBuffer.First.Value;
        }
        else
        {
            Debug.Log("action found");
        }

        if (instructions.actedOn)
        {
            Debug.Log("acted on");
        }

        //send this frame's input to the current state object
        //if this frame's input doesn't cause the character to change state, HandleInput returns null
        ICharacterState nextState = currentState.HandleInput(instructions);


        //if nextState is not null, we are switching states this frame. Call OnExit on the old state and OnEnter on the new state
        if (nextState != null)
        {
            currentState.OnExit();
            currentState = nextState;
            currentState.OnEnter(this);
        }

        //now that we know for certain what state we're in this frame, call this state's Update function
        currentState.Update();

        if (Mathf.Abs(rigidbody.velocity.x) > 0.1f)
        {
            animator.SetBool(walkingHash, true);
        }
        else
        {
            animator.SetBool(walkingHash, false);
        }
    }



    void FixedUpdate()
    {
        if (canMove)
        {
            Vector2 runForce = Vector2.zero;

            if (instructions.moveInstructions != CharacterInstructions.MoveInstructions.neutral)
            {
                runForce = Vector2.right * accelleration * (instructions.moveInstructions == CharacterInstructions.MoveInstructions.right ? 1 : -1);
            }

            rigidbody.AddForce(runForce);
            rigidbody.velocity = new Vector2(Mathf.Clamp(rigidbody.velocity.x, -runSpeed, runSpeed), rigidbody.velocity.y);
        }
    }



    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("environment"))
        {
            if (Vector2.Angle(Vector2.up, col.contacts[0].normal) < 30)
            {
                rigidbody.velocity = Vector2.zero;
                grounded = true;
                animator.SetBool(groundedHash, true);
            }
        }
    }



    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("environment"))
        {
            if (Vector2.Angle(Vector2.up, col.contacts[0].normal) < 30)
            {
                grounded = false;
                animator.SetBool(groundedHash, false);
            }
        }
    }



    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && currentAttack != null)
        {
            CharacterController otherCharacter = col.GetComponent<CharacterController>();
            otherCharacter.Stun(DirectionUtility.GetVector(currentAttackDirection) * currentAttack.knockbackForce, currentAttack.stunTime);
        }
    }

    //*********************************
    // ^^^^^ MONOBEHAVIOUR EVENTS ^^^^^
    //*********************************

    //*********************************
    // VVVVVVVVVV ATTACKING VVVVVVVVVVV
    //*********************************

    //NOTE: All attacking methods are triggered by mecanim events besides the initial Attack call
    public void Attack(Direction direction)
    {
        if (direction != Direction.Neutral)
        {
            animator.SetBool(walkingHash, false);
            currentAttack = attacks[direction];
            currentAttackDirection = direction;
            Vector2 directionVector = DirectionUtility.GetVector(direction);

            switch (direction)
            {
                case Direction.Up:
                    animator.SetTrigger(upAttackTrigger);
                    break;

                case Direction.RightUp:
                    animator.SetTrigger(sideUpAttackTrigger);
                    facingRight = true;
                    break;

                case Direction.Right:
                    animator.SetTrigger(sideAttackTrigger);
                    facingRight = true;
                    break;

                case Direction.RightDown:
                    animator.SetTrigger(sideDownAttackTrigger);
                    facingRight = true;
                    break;

                case Direction.Down:
                    animator.SetTrigger(downAttackTrigger);
                    break;

                case Direction.LeftDown:
                    animator.SetTrigger(sideDownAttackTrigger);
                    facingRight = false;
                    break;

                case Direction.Left:
                    animator.SetTrigger(sideAttackTrigger);
                    facingRight = false;
                    break;

                case Direction.LeftUp:
                    animator.SetTrigger(sideUpAttackTrigger);
                    facingRight = false;
                    break;
            }
        }
    }


    //Called by animation events in mecanim - propels the character in the current attack's direction
    public void AttackForce()
    {
        if (currentAttack != null)
        {
            StopCoroutine("StopRoutine");
            rigidbody.velocity = Vector2.zero;
            rigidbody.AddForce(DirectionUtility.GetVector(currentAttackDirection) * currentAttack.moveForce, ForceMode2D.Impulse);
        }
    }


    //Called by animation events in mecanim - stops the character in place until an AttackForce event is triggered or the attack finishes
    public void StopInPlace()
    {
        StartCoroutine("StopRoutine");
    }



    IEnumerator StopRoutine()
    {
        Vector3 position = transform.position;
        canMove = false;

        while (currentState is Attacking)
        {
            transform.position = position;
            rigidbody.velocity = Vector2.zero;
            yield return null;
        }
    }



    //Called by animation events in mecanim
    public void FireProjectile()
    {
        if (currentAttack.projectile != null)
        {
            GameObject proj = (GameObject)Instantiate(currentAttack.projectile);
            Projectile projectile = proj.GetComponent<Projectile>();
            projectile.direction = currentAttackDirection;
            Vector2 localPos = transform.TransformPoint(projectile.localPos);
            proj.transform.position = localPos;
            float zRot = 0f;

            //Projectiles will fly straight along their local x axis. Rotate in the z plane to allow projectiles to fly in different directions
            switch (currentAttackDirection)
            {
                case Direction.RightUp:
                    zRot = 45f;
                    break;

                case Direction.Up:
                    zRot = 90f;
                    break;

                case Direction.LeftUp:
                    zRot = 135f;
                    break;

                case Direction.Left:
                    zRot = 180f;
                    break;

                case Direction.LeftDown:
                    zRot = -135f;
                    break;

                case Direction.Down:
                    zRot = -90f;
                    break;

                case Direction.RightDown:
                    zRot = -45f;
                    break;
            }

            proj.transform.eulerAngles = new Vector3(0f, 0f, zRot);
        }
    }


    //Called by animation events in mecanim
    public void EndAttack()
    {
        StopCoroutine("StopRoutine");
        canMove = true;
        currentAttackDirection = Direction.Neutral;
        currentAttack = null;
        Hitbox1.gameObject.SetActive(false);
        Hitbox2.gameObject.SetActive(false);

        if (currentState is Attacking)
        {
            Attacking attackState = (Attacking)currentState;
            attackState.EndAttack();
        }
    }

    //*********************************
    // ^^^^^^^^^^ ATTACKING ^^^^^^^^^^^
    //*********************************

    //*********************************
    // VVVVVVVVVVV STUNNED VVVVVVVVVVVV
    //*********************************

    public void Stun(Vector2 stunForce, float stunTime)
    {
        stunned = true;
        canMove = false;
        rigidbody.velocity = Vector2.zero;
        StopCoroutine("StopRoutine");
        rigidbody.AddForce(stunForce, ForceMode2D.Impulse);
        StopCoroutine("RecoveryRoutine");
        StartCoroutine("RecoveryRoutine", stunTime);
    }



    IEnumerator RecoveryRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        stunned = false;
        canMove = true;
    }

    //*********************************
    // ^^^^^^^^^^^ STUNNED ^^^^^^^^^^^^
    //*********************************

    //*********************************
    // VVVVVVVVVVVV MISC. VVVVVVVVVVVVV
    //*********************************

    public void Jump(JumpDirection jumpDirection)
    {
        jumping = true;
        Vector2 jumpVector = Vector2.zero;
        rigidbody.velocity = Vector2.zero;
        rigidbody.gravityScale = 0f;

        switch (jumpDirection)
        {
            case JumpDirection.leftUp:
                jumpVector = Vector2.Lerp(Vector2.up, -Vector2.right, sideJumpDirection).normalized * jumpPower;
                break;

            case JumpDirection.up:
                jumpVector = Vector2.up * jumpPower;
                break;

            case JumpDirection.rightUp:
                jumpVector = Vector2.Lerp(Vector2.up, Vector2.right, sideJumpDirection).normalized * jumpPower;
                break;
        }

        rigidbody.AddForce(jumpVector, ForceMode2D.Impulse);
    }



    public void EndJump()
    {
        if (rigidbody.velocity.y > 0f)
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
        }

        jumping = false;
    }



    public void SetFacingRight(bool facingRight)
    {
        this.facingRight = facingRight;
    }

    //*********************************
    // ^^^^^^^^^^^^ MISC. ^^^^^^^^^^^^^
    //*********************************
}
