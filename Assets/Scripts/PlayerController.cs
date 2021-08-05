using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController : MonoBehaviour
{
	[SerializeField] [Range(0f, 10f)] private float movementSpeed = 5f;
	[SerializeField] [Range(0f, 360f)] private float rotationSensitivity = 90f;

	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		// gas and brake are converted into a translation forward/backward
		rb.MovePosition(transform.position
						 + transform.forward * movementSpeed * (Input.GetAxis("Vertical") * Time.deltaTime));
		// steering is translated into a rotation
		rb.MoveRotation(Quaternion.Euler(0.0f, rotationSensitivity * (Input.GetAxis("Horizontal") * Time.deltaTime), 0.0f)
						* transform.rotation);
	}
}
