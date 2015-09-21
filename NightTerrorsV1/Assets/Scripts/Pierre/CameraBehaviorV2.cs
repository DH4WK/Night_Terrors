using UnityEngine;
using System.Collections;

public class CameraBehaviorV2 : MonoBehaviour
{
	//Pivot
	public Transform pivot;

	[SerializeField] private Vector3 targetRotation = Vector3.zero;
	[SerializeField] private Vector2 XAxisLimit = new Vector2 (-30f, 45f);

	// Ipnut
	[SerializeField] private bool invertY = false;

	[SerializeField] private float mouseWheel_Sensitivity = 5.0f;
	[SerializeField] private Vector2 mouse_Sensitivity = new Vector2 (5f, 5f);		// The x cordinate matches to the y-axis, and the y cordinate matches to the x-axis

	private float mouseX = 0f;
	private float mouseY = 0f;

	// Smooth
	[SerializeField] private float lerpDamp = 2f;
	[SerializeField] private float smoothRotationDamp = 8f;
	[HideInInspector] public Quaternion smoothRotation;
	
	// Distance
	public Transform camera;

	private float currentDistance = 0f;		// the distance will also be variable, player can change it while playing
	private float desiredDistance = 5f;
	private float velDistance = 0f;
	[SerializeField] private float distanceSmooth = 0.05f;

	[SerializeField] private float distanceMin = 3f;			// min distance away from the player
	[SerializeField] private float distanceMax = 10f;			// max distance away from the player

	private Vector3 camPosition = Vector3.zero;

	void Awake ()
	{
		pivot = this.transform;

		GameObject _player = GameObject.FindGameObjectWithTag ("Player");

		if (_player)
		{
			float characterHeight = _player.GetComponent<CapsuleCollider> ().height;
			float characterHead = characterHeight - characterHeight / 4f;				// this is the height we want our camera to look at on our player

			pivot.localPosition = new Vector3 (0f, characterHeight, 0f);

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

		if (Input.GetAxis ("Vertical") != 0f || Input.GetAxis ("Horizontal") != 0f)
		{
			pivot.parent.rotation = Quaternion.Slerp (this.transform.parent.rotation, Quaternion.Euler (Vector3.up * targetRotation.y), lerpDamp * Time.deltaTime);
		}

		pivot.rotation = Quaternion.Euler (Vector3.right * smoothRotation.eulerAngles.x + Vector3.up * smoothRotation.eulerAngles.y);

		// Calculate distance
		currentDistance = Mathf.SmoothDamp (currentDistance, desiredDistance, ref velDistance, distanceSmooth);

		// Calculate our desired position realative to the pivot
		camPosition = new Vector3 (0f, 0f, -currentDistance);

		camera.localPosition = camPosition;
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
			desiredDistance = Mathf.Clamp (currentDistance - Input.GetAxis ("Mouse ScrollWheel") * mouseWheel_Sensitivity, distanceMin, distanceMax);
		}
	}
}
