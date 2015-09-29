using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum Direction { Up, RightUp, Right, RightDown, Down, LeftDown, Left, LeftUp, Neutral }

public static class DirectionUtility
{
    public static Vector2 GetVector(Direction thisDir)
    {
        Vector2 dir = Vector2.zero;

        switch (thisDir)
        {
            case Direction.Up:
                dir = Vector2.up;
                break;

            case Direction.RightUp:
                dir = new Vector2(1f, 1f);
                break;

            case Direction.Right:
                dir = Vector2.right;
                break;

            case Direction.RightDown:
                dir = new Vector2(1f, -1f);
                break;

            case Direction.Down:
                dir = -Vector2.up;
                break;

            case Direction.LeftDown:
                dir = new Vector2(-1f, -1f);
                break;

            case Direction.Left:
                dir = -Vector2.right;
                break;
                
            case Direction.LeftUp:
                dir = new Vector2(-1f, 1f);
                break;
        }

        return dir.normalized;
    }



    public static Direction GetDirection(Vector2 thisVector)
    {
        float dot = Vector2.Dot(Vector2.up, thisVector);

        if (dot >= 0.75f)
        {
            return Direction.Up;
        }
        else if (dot < 0.75f && dot >= 0.25f)
        {
            if (thisVector.x > 0)
            {
                return Direction.RightUp;
            }
            else
            {
                return Direction.LeftUp;
            }
        }
        else if (dot < 0.25f && dot >= -0.25f)
        {
            if (thisVector.x > 0)
            {
                return Direction.Right;
            }
            else
            {
                return Direction.Left;
            }
        }
        else if (dot < -0.25f && dot >= -0.75f)
        {
            if (thisVector.x > 0)
            {
                return Direction.RightDown;
            }
            else
            {
                return Direction.LeftDown;
            }
        }
        else
        {
            return Direction.Down;
        }
    }
}

public class ControllerInput : MonoBehaviour, IGetInput {

    public Text leftStickText;
    public Text rightStickText;
    Vector2 leftStickInput = Vector2.zero;
    public Vector2 LeftStickInput
    {
        get { return leftStickInput; }
    }
    Vector2 rightStickInput = Vector2.zero;
    public Vector2 RightStickInput
    {
        get { return rightStickInput; }
    }

    bool leftStickPressed = false;
    bool leftStickPressedCache = false;
    bool leftStickDown = false;
    bool rightStickPressed = false;
    bool rightStickPressedCache = false;
    bool rightStickDown = false;
    bool jumpSwitch = false;

    CharacterController character;

    Direction leftStickDirection = Direction.Neutral;
    Direction rightStickDirection = Direction.Neutral;

    public static float stickInputThreshold = 0.8f;


    void Start()
    {
        character = GetComponent<CharacterController>();
    }



    public void GetInput(ref CharacterInstructions instructions)
    {
        float L_X = Input.GetAxis("L_XAxis_" + character.playerNo);
        float L_Y = Input.GetAxis("L_YAxis_" + character.playerNo);
        leftStickInput = new Vector2(L_X, L_Y);

        float R_X = Input.GetAxis("R_XAxis_" + character.playerNo);
        float R_Y = Input.GetAxis("R_YAxis_" + character.playerNo);
        rightStickInput = new Vector2(R_X, R_Y);

        //If left stick is pressed this frame
        if (leftStickInput.sqrMagnitude >= stickInputThreshold * stickInputThreshold)
        {
            leftStickPressed = true;
            leftStickDown = false;
            leftStickDirection = DirectionUtility.GetDirection(leftStickInput);

            //If left stick down this frame
            if (!leftStickPressedCache)
            {
                leftStickDown = true;
            }
        }
        else
        {
            leftStickPressed = false;

            //If left stick up this frame
            if (leftStickPressedCache)
            {
                leftStickDirection = Direction.Neutral;
            }
        }

        //If right stick is pressed this frame
        if (rightStickInput.sqrMagnitude >= stickInputThreshold * stickInputThreshold)
        {
            rightStickPressed = true;
            rightStickDown = false;
            rightStickDirection = DirectionUtility.GetDirection(rightStickInput);

            //If right stick down this frame
            if (!rightStickPressedCache)
            {
                rightStickDown = true;
            }
        }
        else
        {
            rightStickPressed = false;

            //If right stick up this frame
            if (rightStickPressedCache)
            {
                rightStickDirection = Direction.Neutral;
            }
        }

        leftStickPressedCache = leftStickPressed;
        rightStickPressedCache = rightStickPressed;

        //if the right stick is down this frame, set this frame's action instructions to attacking
        if(rightStickDown)
        {
            instructions.actionInstructions = CharacterInstructions.ActionInstructions.attack;
            instructions.direction = rightStickDirection;
        }
        else if(character.grounded)
        {
            if(leftStickInput.y > ControllerThresholds.JumpThreshold)
            {
                if (!jumpSwitch)
                {
                    instructions.actionInstructions = CharacterInstructions.ActionInstructions.jump;
                    jumpSwitch = true;
                }
            }
            else
            {
                jumpSwitch = false;
            }

            if(leftStickInput.x > ControllerThresholds.WalkThreshold)
            {
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.right;
            }
            else if(leftStickInput.x < -ControllerThresholds.WalkThreshold)
            {
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.left;
            }
            else
            {
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.neutral;
            }
        }
        else
        {
            if (leftStickDown)
            {
                instructions.actionInstructions = CharacterInstructions.ActionInstructions.dash;
                instructions.direction = leftStickDirection;
            }
        }

        /*
        bool tryToJump = false;

        switch (leftStickDirection)
        {
            case Direction.Neutral:
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.neutral;
                break;

            case Direction.Up:
                tryToJump = true;
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.neutral;
                break;

            case Direction.RightUp:
                tryToJump = true;
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.right;
                break;

            case Direction.Right:
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.right;
                break;

            case Direction.RightDown:
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.right;
                break;

            case Direction.Down:
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.neutral;
                break;

            case Direction.LeftDown:
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.left;
                break;

            case Direction.Left:
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.left;
                break;

            case Direction.LeftUp:
                tryToJump = true;
                instructions.moveInstructions = CharacterInstructions.MoveInstructions.left;
                break;
        }

        if(!character.grounded && leftStickDown)
        {
            instructions.actionInstructions = CharacterInstructions.ActionInstructions.dash;
            instructions.direction = leftStickDirection;
            instructions.moveInstructions = CharacterInstructions.MoveInstructions.neutral;
        }
        //if the left stick is down this frame and the direction is up and the action this frame is not attacking
        else if (tryToJump)
        {
            if (!jumpSwitch && instructions.actionInstructions == CharacterInstructions.ActionInstructions.none)
            {
                jumpSwitch = true;
                instructions.actionInstructions = CharacterInstructions.ActionInstructions.jump;
            }
        }
        else
        {
            jumpSwitch = false;
        }
        */
    }
}
