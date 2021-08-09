using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StealthyAgent : MonoBehaviour
{
    public GameObject player;
    public GameObject destination;

    [SerializeField] [Range(0f, 3f)] private float reactionTime = 1f;
    [SerializeField] [Range(5f, 15f)] private float sensingRange = 10f;
    [SerializeField] private float sightAngle = 45f;

    private List<StaticEnemy> nearbyEnemies = null;

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
        DTDecision d1 = new DTDecision(EnemyInRange);
        DTDecision d2 = new DTDecision(EnemySpotted);
        DTDecision d3 = new DTDecision(EnemyVisible);
       
        DTAction a1 = new DTAction(SpotEnemy);
        DTAction a2 = new DTAction(AvoidEnemy);

        d1.AddLink(true, d2);
        d2.AddLink(true, a2);
        d2.AddLink(false, d3);
        d3.AddLink(true, a1);

        decisionTree = new DecisionTree(d1);
        #endregion

        GetComponent<NavMeshAgent>().destination = destination.transform.position;
        StartCoroutine(RunFSM());
    }

    private void OnValidate()
    {
        Transform t = transform.Find("SensingRange");
        if (t != null)
        {
            t.localScale = new Vector3(sensingRange / transform.localScale.x, 1f, sensingRange / transform.localScale.z) / 5f;
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
        return (player.transform.position - transform.position).magnitude <= sensingRange ? true : false;
    }

    private bool PlayerOutOfRange()
    {
        return !PlayerInRange();
    }

    private object EnemyInRange(object o)
    {
        bool enemyFound = false;
        StaticEnemy enemy;
        nearbyEnemies.Clear();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy = go.gameObject.GetComponent<StaticEnemy>();
            if (!nearbyEnemies.Contains(enemy))
                nearbyEnemies.Add(enemy);
            if ((go.transform.position - transform.position).magnitude <= sensingRange)
                enemyFound = true;
        }
        if (enemyFound == true) return true;
        return false;
    }

    private object EnemySpotted(object o)
    {
        for (int i=0; i<nearbyEnemies.Count; i++)
        {
            if (nearbyEnemies[i].isSpotted == true) return true;
        }
        return false;
    }

    private object EnemyVisible(object o)
    {
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, 1f, transform.forward, out hitInfo);
        if (hitInfo.transform.gameObject.tag == "Enemy")
            Debug.Log("Enemy visible");
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
