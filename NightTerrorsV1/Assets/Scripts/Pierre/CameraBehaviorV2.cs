using UnityEngine;
using System.Collections;

public class CameraBehaviorV2 : MonoBehaviour
{
	//Pivot
	public Transform pivot;

	[SerializeField] private Vector3 targetRotation = Vector3.zero;
	[SerializeField] private float realativePositionX = 0f;
	[SerializeField] private Vector2 XAxisLimit = new Vector2 (-30f, 45f);

	// Ipnut
	[SerializeField] private bool invertY = false;

	[SerializeField] private float mouseWheel_Sensitivity = 5.0f;
	[SerializeField] private Vector2 mouse_Sensitivity = new Vector2 (5f, 5f);		// The x cordinate matches to the y-axis, and the y cordinate matches to the x-axis

	private float mouseX = 0.0f;
	private float mouseY = 0.0f;

	// Smooth
	[SerializeField] private float smoothRotationDamp = 8f;
	[HideInInspector] public Quaternion smoothRotation;
	
	// Distance
	public Transform camera;

	[SerializeField] private float currentDistance = 5.0f;		// the distance will also be variable, player can change it while playing

	[SerializeField] private float desiredDistnace = 0.0f;
	private float velDistance = 0.0f;
	[SerializeField] private float distanceSmooth = 0.05f;

	[SerializeField] private float distanceMin = 3.0f;			// min distance away from the player
	[SerializeField] private float distanceMax = 10.0f;			// max distance away from the player

	void Start ()
	{
		pivot = this.transform;

		GameObject _player = GameObject.FindGameObjectWithTag ("Player");

		if (_player)
		{
			float characterHeight = _player.GetComponent<CapsuleCollider> ().height;
			float characterHead = characterHeight - characterHeight / 8f;				// this is the height we want our camera to look at on our player

			this.transform.localPosition = new Vector3 (realativePositionX, characterHeight, 0f);

			if (GetComponentInChildren<Camera> () != null)
			{
				camera = GetComponentInChildren<Camera> ().transform;
			}
			else
			{
				camera = new GameObject ("Main Camera").transform;
				camera.parent = this.transform;
				camera.localPosition = Vector3.zero;
				camera.gameObject.AddComponent<Camera> ();
				camera.tag = "MainCamera";
				
				camera.gameObject.AddComponent<FlareLayer> ();
				camera.gameObject.AddComponent<GUILayer> ();
				//camera.gameObject.AddComponent<AudioListener> ();
			}

			if (!camera)
			{
				Debug.Log ("No Camera! You messed up big time!");
				this.enabled = false;
			}
		}
		else
		{
			Debug.Log ("No Player! You messed up big time!");
			this.enabled = false;
		}
	}

	void LateUpdate ()
	{
		HandleInput ();

		smoothRotation = Quaternion.Slerp (smoothRotation, Quaternion.Euler (targetRotation), smoothRotationDamp * Time.deltaTime);

		this.transform.parent.rotation = Quaternion.Euler (Vector3.up * smoothRotation.eulerAngles.y);
		this.transform.rotation = Quaternion.Euler (Vector3.right * smoothRotation.eulerAngles.x + Vector3.up * smoothRotation.eulerAngles.y);

		// Calculate distance
		currentDistance = Mathf.SmoothDamp (currentDistance, desiredDistnace, ref velDistance, distanceSmooth);
		
		// Calculate our desired position realative to the pivot
		Vector3 cameraPos = new Vector3 (0f, 0f, -currentDistance);

		camera.localPosition = cameraPos;
	}

	void HandleInput ()
	{
		float deadZone = 0.01f;
		
		// Check for the right mouse button down
		if (Input.GetMouseButton (1))
		{
			// Get mouse axis input
			mouseX += Input.GetAxis ("Mouse X") * mouse_Sensitivity.y;

			if (mouseX > 360f || mouseX < -360f)
			{
				mouseX = 0f;
			}

			mouseY -= Input.GetAxis ("Mouse Y") * mouse_Sensitivity.x;
		}
		
		// Clamp / limit our mouseY input
		mouseY = Mathf.Clamp (mouseY, XAxisLimit.x, XAxisLimit.y);

		targetRotation = new Vector3 (mouseY, mouseX, 0f);
		
		// Get input from the mousewheel
		if (Input.GetAxis ("Mouse ScrollWheel") < -deadZone || Input.GetAxis ("Mouse ScrollWheel") > deadZone)
		{
			desiredDistnace = Mathf.Clamp (currentDistance - Input.GetAxis ("Mouse ScrollWheel") * mouseWheel_Sensitivity, distanceMin, distanceMax);
		}
	}
}
