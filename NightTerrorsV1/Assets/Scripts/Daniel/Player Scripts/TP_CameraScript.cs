using UnityEngine;
using System.Collections;

public class TP_CameraScript : MonoBehaviour 
{
    /*
     * This script will see if we have a main camera in the scene
     *      if so, use that camera
     *      if not, create one and use the new camera
     *              create a game object "Main Camera'
     *              Apply camera component to it
     *              Tag it as a Main Camera
     *  
     * When we have our Main Camera
     *      add the TP_Camera script (this script) to the temp camera
     *      Store a reference to this instance of the TP_camera script
     *      
     * Now check for a target for the camera to look at
     *      if we have one, store it in the targetLookAt
     *      if not, create one at the origin (0,0,0)
     *      
     * MainCamera  look at target
     *      make sure camera is correct distance away from player
     *      if not clamp it to either min or max distance
     * 
     * Process Mouse Input
     *      check if right mouse button (RMB) is being pressed
     *      then check mouse axial motion for sensitivity (x & y)
     *          [x] will be unlimited should be able to go into a circle around the character
     *          [y] will be limited in that not too high above the character and not too far down/below the character
     *      then get the mouse wheel input so that the camera distance is not too far or to close to player
     *          check the deadzone for the mousewheel sensitivity
     *          apply the sensitivity to the mouse wheel
     *          then clamp the y distance above or below the chracter
     * 
     * Make the camera goes where the player wants the camera to go
     *      once we have the desired position we will
     *      move our camera to the final position
     *      look at targetlookat
     * 
     * Have a backup reset/restart camera position
     *      Will restore camera to original/default position (like if player dies)
    */

    public static TP_CameraScript Instance;

    //Camera
    [Header("Camera's True Target")]
    public Transform TargetLookAt; // cameras true target to look at, like the back of the players head

    [Header("Camera Distance from Player")]
	public LayerMask CameraClip = 1 << LayerMask.NameToLayer ("CameraClip");

    public float distance = 5.0f;            // the distance will also be variable, player can change it while playing
    public float distanceMin = 3.0f;         // min distance away from the player
    public float distanceMax = 10.0f;        // max distance away from the player
    public float distanceSmooth = 0.05f;

    private float startDistance = 0.0f;      // starting distance for the camera, default position
    private float desiredDistnace = 0.0f;
    private float velDistance = 0.0f;

    private Vector3 desiredPosition = new Vector3(0,0,0);

    [Header("Mouse Input")]
    private float mouseX = 0.0f;
    private float mouseY = 0.0f;

    public float X_MouseSensitivity = 5.0f;
    public float Y_MouseSensitivity = 5.0f;
    public float MouseWheel_Sensitivity = 5.0f;

    public float Y_MinLimit = -40.0f;
    public float Y_MaxLimit = 80.0f;

    public float X_Smooth = 0.05f;
    public float Y_Smooth = 0.1f;
    private float velX = 0.0f;
    private float velY = 0.0f;
    private float velZ = 0.0f;

    private Vector3 position = Vector3.zero;

	void Awake()
    {
        Instance = this;
	}

    void Start()
    {
        // Make sure our distance is valid
        distance = Mathf.Clamp(distance, distanceMin, distanceMax);

        // So now we have our start distance
        startDistance = distance;

        Reset();
    }
	
	void LateUpdate() 
    {
        if (TargetLookAt == null)
            return;

        HandlePlayerInput();

        CalculateDesiredPosition();

        UpdatePosition();
	}

    void HandlePlayerInput()
    {
        float deadZone = 0.01f;

        // Check for the right mouse button down
        if (Input.GetMouseButton(1))
        {
            // Get mouse axis input
            mouseX += Input.GetAxis("Mouse X") * X_MouseSensitivity;
            mouseY -= Input.GetAxis("Mouse Y") * Y_MouseSensitivity;
        }

        // Clamp / limit our mouseY
        mouseY = TP_HelperClassScript.ClampAngle(mouseY, Y_MinLimit, Y_MaxLimit);

        // Get input from the mousewheel
        if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
        {
            desiredDistnace = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * MouseWheel_Sensitivity,
                                          distanceMin, distanceMax);
        }
    }

    void CalculateDesiredPosition()
    {
        // Calculate distance
        distance = Mathf.SmoothDamp(distance, desiredDistnace, ref velDistance, distanceSmooth);

        // Calculate our desired position
        desiredPosition = CalculatePosition(mouseY, mouseX, distance);
    }

    Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
    {
        // Get our direction
        Vector3 direction = new Vector3(0, 0, -distance);

        // Get the rotation
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);

		// PEIRRE's EDIT (Wall detection)
		RaycastHit hit;
		if (Physics.Linecast (TargetLookAt.position, TargetLookAt.position + rotation * direction, out hit, CameraClip))
		{
			direction += this.transform.InverseTransformPoint (hit.point + hit.normal);
		}
        
        return TargetLookAt.position + rotation * direction;
    }

    void UpdatePosition()
    {
        float posX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
        float posY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
        float posZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);

        position = new Vector3(posX, posY, posZ);

        transform.position = position;

        transform.LookAt(TargetLookAt);
    }

    public void Reset()
    {
        mouseX = 0.0f;
        mouseY = 10.0f;
        distance = startDistance;
        desiredDistnace = distance;
    }

    public static void UseExistingOrCreateNewMainCamera()
    {
        GameObject tempCamera;
        GameObject targetLookAt;
        TP_CameraScript myCamera;
        
        float playersHeight;
        float players_Back_of_the_Head;

        // Get our player, which is our target to look at
        GameObject _Player = GameObject.FindGameObjectWithTag("Player");
        playersHeight = TP_ControllerScript.player_Character_Controller.height;
        players_Back_of_the_Head = playersHeight - playersHeight / 8;   // this is the height, we want our camera to look at on our player

        // Checking if there is a main camera in the scene
        if (Camera.main != null)
        {
            tempCamera = Camera.main.gameObject;
        }
        else
        {
            tempCamera = new GameObject("Main Camera");
            tempCamera.AddComponent<Camera>();
            tempCamera.tag = "MainCamera";

			// PIERRE's EDIT:
			tempCamera.AddComponent<FlareLayer> ();
			tempCamera.AddComponent<GUILayer> ();
			tempCamera.AddComponent<AudioListener> ();
        }

        // Have a reference to the camera script on the main camera/ or new main camera
        tempCamera.AddComponent<TP_CameraScript>();
        myCamera = tempCamera.GetComponent<TP_CameraScript>();

        // See if we have a target for the camera
        targetLookAt = GameObject.Find("targetLookAt");

        // Check to see if we found the cameras target
        if (targetLookAt == null)
        {
            targetLookAt = new GameObject("targetLookAt");
            targetLookAt.transform.position = Vector3.zero;
        }

        // Make our target the same position as our player
        targetLookAt.transform.position = _Player.transform.position;
        
        // Adjust the height of our targets height (back of the players head)
        targetLookAt.transform.position = new Vector3(targetLookAt.transform.position.x,
                                                      players_Back_of_the_Head + targetLookAt.transform.position.y,
                                                      targetLookAt.transform.position.z);
        
        // Then parent that target under the 
        targetLookAt.transform.parent = _Player.transform;

        // Now make our true camera look at our target
        myCamera.TargetLookAt = targetLookAt.transform;
    }
}
