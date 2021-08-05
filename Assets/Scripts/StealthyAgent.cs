using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StealthyAgent : MonoBehaviour
{
    public GameObject destination;

    void Start()
    {
        GetComponent<NavMeshAgent>().destination = destination.transform.position;
    }
}
