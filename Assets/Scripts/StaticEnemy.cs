using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticEnemy : MonoBehaviour
{
    public bool isSpotted = false;

    void Awake()
    {
        StartCoroutine(RotateEnemy());
    }

    //Rotate enemy if its field of view intersects an obstacle
    IEnumerator RotateEnemy()
    {
        while (true)
        {
            RaycastHit leftHit;
            RaycastHit rightHit;
            bool isLeft = Physics.Raycast(transform.position,
                Quaternion.Euler(0, -30f, 0) * transform.forward, out leftHit);
            bool isRight = Physics.Raycast(transform.position,
                Quaternion.Euler(0, 30f, 0) * transform.forward, out rightHit);

            if (isLeft)
            {
                if (leftHit.collider.gameObject.tag == "Obstacle")
                {
                    transform.Rotate(0, 45f, 0);
                    Debug.Log("Rotate right");
                }
            }

            if (isRight)
            {
                if (rightHit.collider.gameObject.tag == "Obstacle")
                {
                    transform.Rotate(0, -45f, 0);
                    Debug.Log("Rotate left");
                }
            }

            if (isLeft && isRight)
            {
                transform.Rotate(0, 90f, 0);
                Debug.Log("Rotate");
            }

            else
                break;

            yield return null;
        }
    }
}
