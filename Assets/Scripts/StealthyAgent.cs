using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StealthyAgent : MonoBehaviour
{
    public GameObject player;
    public GameObject destination;

    [SerializeField] [Range(5f, 15f)] private float range = 10f;
    private FSM fsm;
    private DecisionTree decisionTree;

    void Start()
    {
        FSMState moving = new FSMState();
        moving.enterActions.Add(MoveToDestination);

        FSMState idle = new FSMState();
        idle.enterActions.Add(Stop);

        FSMTransition t1 = new FSMTransition(playerOutOfRange);
        FSMTransition t2 = new FSMTransition(playerInRange);

        moving.AddTransition(t1, idle);
        idle.AddTransition(t2, moving);

        fsm = new FSM(moving);

        StartCoroutine(RunFSM());
    }

    private void OnValidate()
    {
        Transform t = transform.Find("Range");
        if (t != null)
        {
            t.localScale = new Vector3(range / transform.localScale.x, 1f, range / transform.localScale.z) / 5f;
        }
    }

    private void MoveToDestination()
    {
        GetComponent<NavMeshAgent>().destination = destination.transform.position;
    }

    private void Stop()
    {
        GetComponent<NavMeshAgent>().destination = transform.position;
    }

    private bool playerInRange()
    {
        return (player.transform.position - transform.position).magnitude <= range ? true : false;
    }

    private bool playerOutOfRange()
    {
        return !playerInRange();
    }

    IEnumerator RunFSM()
    {
        while (true)
        {
            fsm.Update();
            yield return null;
        }
    }
}
