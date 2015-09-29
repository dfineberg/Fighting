using UnityEngine;
using System.Collections;

public class ControllerThresholds : MonoBehaviour {

    public float sideJumpThreshold;
    public float jumpThreshold;
    public float walkThreshold;
    public float runThreshold;

    public static float SideJumpThreshold
    {
        get { return instance.sideJumpThreshold; }
    }

    public static float JumpThreshold
    {
        get { return instance.jumpThreshold; }
    }

    public static float WalkThreshold
    {
        get { return instance.walkThreshold; }
    }

    public static float RunThreshold
    {
        get { return instance.runThreshold; }
    }

    static ControllerThresholds instance;


    void Start()
    {
        instance = this;
    }
}
