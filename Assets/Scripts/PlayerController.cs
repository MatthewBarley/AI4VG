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
        rb.MovePosition(transform.position + transform.forward
            * movementSpeed * (Input.GetAxis("Vertical") * Time.fixedDeltaTime));
        rb.MoveRotation(Quaternion.Euler(0.0f, rotationSensitivity *
            (Input.GetAxis("Horizontal") * Time.fixedDeltaTime), 0.0f)
                        * transform.rotation);
    }
}
