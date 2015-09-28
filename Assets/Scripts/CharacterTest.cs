﻿using UnityEngine;
using System.Collections;

public class CharacterTest : MonoBehaviour, IGetInput {

    public Direction repeatDirection;

    CharacterController myCharacter;


    public void GetInput(ref CharacterInput input)
    {
        input.rightStickDown = repeatDirection;
    }
}