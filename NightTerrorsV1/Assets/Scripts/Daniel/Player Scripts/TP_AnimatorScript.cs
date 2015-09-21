using UnityEngine;
using System.Collections;

public class TP_AnimatorScript : MonoBehaviour 
{
    public static TP_AnimatorScript Instance;

    public enum Direction
    {
        Stationary, Forward, Backward, Left, Right,
        LeftForward, RightForward, LeftBackward, RightBackward
    }

    public Direction MoveDirection { get; set; }

	void Awake()
    {
        Instance = this;
	}
	
	void Update() 
    {
	
	}

    public void DetermineCurrentMoveDirection()
    {
        // Determine the move direction state based off of the move vector
        bool forward = false;
        bool backward = false;
        bool left = false;
        bool right = false;

        // Checking for the basic movement
        if (TP_MotorScript.Instance.MoveVector.z > 0)
            forward = true;

        if (TP_MotorScript.Instance.MoveVector.z < 0)
            backward = true;

        if (TP_MotorScript.Instance.MoveVector.x > 0)
            right = true;

        if (TP_MotorScript.Instance.MoveVector.x < 0)
            left = true;

        // Now checking for a slight more advanced movement
        if (forward)
        {
            if (left)
                MoveDirection = Direction.LeftForward;
            else if (right)
                MoveDirection = Direction.RightForward;
            else
                MoveDirection = Direction.Forward;
        }
        else if (backward)
        {
            if (left)
                MoveDirection = Direction.LeftBackward;
            else if (right)
                MoveDirection = Direction.RightBackward;
            else
                MoveDirection = Direction.Backward;
        }
        else if (left)
            MoveDirection = Direction.Left;
        else if (right)
            MoveDirection = Direction.Right;
        // if we are not moving
        else
            MoveDirection = Direction.Stationary;
    }
}
