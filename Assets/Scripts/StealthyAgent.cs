using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StealthyAgent : MonoBehaviour
{
    public GameObject player;
    public GameObject destination;

    [SerializeField] [Range(5f, 15f)] private float playerRange = 10f;
    [SerializeField] [Range(0f, 3f)] private float reactionTime = 1f;
    [SerializeField] private float sightDistance = 10f;
    [SerializeField] private float sightAngle = 45f;
    
    private FSM fsm;
    private DecisionTree decisionTree;

    void Start()
    {
        #region FSM
        FSMState idle = new FSMState();
        idle.enterActions.Add(StopMoving);
        idle.enterActions.Add(StopDT);

        FSMState moving = new FSMState();
        moving.enterActions.Add(StartMoving);
        moving.enterActions.Add(StartDT);

        FSMTransition t1 = new FSMTransition(PlayerInRange);
        FSMTransition t2 = new FSMTransition(PlayerOutOfRange);

        idle.AddTransition(t1, moving);
        moving.AddTransition(t2, idle);

        fsm = new FSM(idle);
        #endregion

        #region DT
        DTDecision d1 = new DTDecision(EnemyVisible);
        DTDecision d2 = new DTDecision(EnemySpotted);
        DTDecision d3 = new DTDecision(EnemyHasLOS);

        DTAction a1 = new DTAction(SpotEnemy);
        DTAction a2 = new DTAction(AvoidEnemy);

        d1.AddLink(true, d2);
        d2.AddLink(true, d3);
        d2.AddLink(false, a1);
        d3.AddLink(true, a2);

        decisionTree = new DecisionTree(d1);
        #endregion

        GetComponent<NavMeshAgent>().destination = destination.transform.position;
        StartCoroutine(RunFSM());
    }

    private void OnValidate()
    {
        Transform t = transform.Find("Range");
        if (t != null)
        {
            t.localScale = new Vector3(playerRange / transform.localScale.x, 1f, playerRange / transform.localScale.z) / 5f;
        }
    }

    private void StartMoving()
    {
        GetComponent<NavMeshAgent>().isStopped = false;
    }

    private void StopMoving()
    {
        GetComponent<NavMeshAgent>().isStopped = true;
    }

    private bool PlayerInRange()
    {
        return (player.transform.position - transform.position).magnitude <= playerRange ? true : false;
    }

    private bool PlayerOutOfRange()
    {
        return !PlayerInRange();
    }

    private object EnemyVisible(object o)
    {
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, 1f, transform.forward, out hitInfo, sightDistance);
        if (hitInfo.collider.CompareTag("Enemy"))
            Debug.Log("Naples");
        return null;
    }

    private object EnemySpotted(object o)
    {
        return null;
    }

    private object EnemyHasLOS(object o)
    {
        return null;
    }

    private object SpotEnemy(object o)
    {
        return null;
    }

    private object AvoidEnemy(object o)
    {
        return null;
    }

    private void StartDT()
    {
        StartCoroutine(RunDT());
    }

    private void StopDT()
    {
        StopCoroutine(RunDT());
    }

    IEnumerator RunFSM()
    {
        while (true)
        {
            fsm.Update();
            yield return new WaitForSeconds(reactionTime);
        }
    }

    IEnumerator RunDT()
    {
        while (true)
        {
            decisionTree.walk();
            yield return null;
        }
    }
}
