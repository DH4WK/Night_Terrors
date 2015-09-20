using UnityEngine;
using System.Collections;

public class TP_MotorScript : MonoBehaviour 
{
    public static TP_MotorScript Instance;

    public float MoveSpeed = 10.0f;
    public float Gravity = 21.0f;
    public float TerminalVelocity = 20.0f;
    public float JumpSpeed = 6.0f;
    public float SlideThreshold = 0.9f;
    public float MaxControllableSlideMagnitude = 0.4f;

    private Vector3 slideDirection;

    public Vector3 MoveVector { get; set; }
    public float VerticalVelocity { get; set; }

    void Awake()
    {
        Instance = this;
    }

    public void UpdateMotor()
    {
        SnapAlignCharacterWithCamera();
        
        ProcessMotion();
    }

    void ProcessMotion()
    {
        // Transform MoveVector to Worldspace
        MoveVector = transform.TransformDirection(MoveVector);

        // Normalize MoveVector if Magnitude > 1
        if (MoveVector.magnitude > 1)
            MoveVector = Vector3.Normalize(MoveVector);

        // Apply sliding if we are sliding (if applicable)
        ApplySlide();

        // Multiply MoveVector by MoveSpeed (Speed per Frame)
        MoveVector *= MoveSpeed;

        // Reapply vertical velocity to the y axis
        MoveVector = new Vector3(MoveVector.x, VerticalVelocity, MoveVector.z);

        // Apply gravity
        ApplyGravity();

        // Multiply MoveVector by DeltaTime (Speed per Second)
        // Move the Character in the Worldspace
        TP_ControllerScript.player_Character_Controller.Move(MoveVector * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (MoveVector.y > -TerminalVelocity)
            MoveVector = new Vector3(MoveVector.x, 
                                     MoveVector.y - Gravity * Time.deltaTime,
                                     MoveVector.z);
        if(TP_ControllerScript.player_Character_Controller.isGrounded && MoveVector.y < -1)
            MoveVector = new Vector3(MoveVector.x, -1, MoveVector.z);
    }

    void ApplySlide()
    {
        // if in the air, no need to slide
        if (!TP_ControllerScript.player_Character_Controller.isGrounded)
            return;

        // zero out the direction where sliding
        slideDirection = Vector3.zero;

        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitInfo))
        {
            // if we are sliding
            if (hitInfo.normal.y < SlideThreshold)
                slideDirection = new Vector3(hitInfo.normal.x, -hitInfo.normal.y, hitInfo.normal.z);
        }

        if (slideDirection.magnitude < MaxControllableSlideMagnitude)
            MoveVector += slideDirection;
        else
            MoveVector = slideDirection;
    }

    public void Jump()
    {
        // check to see if we are on the ground before we jump
        if (TP_ControllerScript.player_Character_Controller.isGrounded)
            VerticalVelocity = JumpSpeed;
    }

    void SnapAlignCharacterWithCamera()
    {
        if (MoveVector.x != 0 || MoveVector.z != 0)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
                                                  Camera.main.transform.eulerAngles.y,
                                                  transform.eulerAngles.z);
        }
    }
}
