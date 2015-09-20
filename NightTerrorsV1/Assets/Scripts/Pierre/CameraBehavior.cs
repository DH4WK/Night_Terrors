using UnityEngine;
using System.Collections;

public class CameraBehavior : MonoBehaviour
{
	public Transform player;

	// rot
	private Vector3 targetRotation = Vector3.zero;
	[SerializeField] private Vector3 sensitivity = new Vector3 (5f, 5f, 0f);
	[SerializeField] private bool invertY = false;
	[SerializeField] private Vector2 clamp = new Vector2 (-30f, 45f);
	
	[SerializeField] private float rotDamp = 8f;
	[HideInInspector] public Quaternion smoothRotation;
	
	// pos
	[SerializeField] private Vector3 distanceAway = new Vector3 (1.3f, 2.9f, -3.7f);

	void Start ()
	{
		player = GameObject.FindWithTag ("Player").transform;
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 lookInput = new Vector3 (Input.GetAxis ("Mouse Y"), Input.GetAxis ("Mouse X"), 0f);
		
		targetRotation += new Vector3 (lookInput.x * sensitivity.x * (invertY ? 1f : -1f), lookInput.y * sensitivity.y, 0f);
		targetRotation.x = Mathf.Clamp (targetRotation.x, clamp.x, clamp.y);
		
		if (targetRotation.y >= 360f || targetRotation.y <= -360f)
		{
			targetRotation.y = 0f;
		}
		
		smoothRotation = Quaternion.Lerp (smoothRotation, Quaternion.Euler (targetRotation), rotDamp * Time.deltaTime);
		Vector3 eulerAngles = smoothRotation.eulerAngles;
		eulerAngles.z = 0f;
		smoothRotation.eulerAngles = eulerAngles;
		
		this.transform.position = player.position + (smoothRotation * new Vector3 (distanceAway.x, 0f, distanceAway.z)) + new Vector3 (0f, distanceAway.y, 0f);
		this.transform.rotation = smoothRotation;
	}
}
