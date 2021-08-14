using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StealthyAgent : MonoBehaviour
{
    public GameObject player;
    public GameObject destination;

    [SerializeField] [Range(0f, 2f)] private float reactionTime = 1f;
    [SerializeField] [Range(5f, 15f)] private float sensingRange = 10f; //For both player and enemy detection
    [SerializeField] private float sightAngle = 45f;

    private float startingSightAngle;
    private NavMeshAgent agent;
    private List<StaticEnemy> nearbyEnemies = new List<StaticEnemy>();
    private List<StaticEnemy> visibleEnemies = new List<StaticEnemy>();

    private FSM fsm;
    private DecisionTree decisionTree;

    void Start()
    {
        startingSightAngle = sightAngle;
        agent = GetComponent<NavMeshAgent>();
        agent.destination = destination.transform.position;

        //FSM setup
        FSMState idle = new FSMState();
        idle.enterActions.Add(StopMoving);

        FSMState moving = new FSMState();
        moving.enterActions.Add(StartMoving);

        FSMTransition t1 = new FSMTransition(PlayerInRange);
        FSMTransition t2 = new FSMTransition(PlayerOutOfRange);

        idle.AddTransition(t1, moving);
        moving.AddTransition(t2, idle);

        fsm = new FSM(idle);

        //DT setup
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

        StartCoroutine(RunFSM());
        StartCoroutine(RunDT());
    }

    private void OnValidate()
    {
        Transform t = transform.Find("SensingRange");
        if (t != null)
        {
            t.localScale = new Vector3(sensingRange / transform.localScale.x,
                1f, sensingRange / transform.localScale.z) / 5f;
            t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, sensingRange / 2f);
        }
    }

    private void StartMoving()
    {
        agent.isStopped = false;
    }

    private void StopMoving()
    {
        agent.isStopped = true;
    }

    //Player detection
    private bool PlayerInRange()
    {
        Transform t = transform.Find("SensingRange");
        return (player.transform.position - t.position).magnitude <= sensingRange ? true : false;
    }

    private bool PlayerOutOfRange()
    {
        return !PlayerInRange();
    }

    //Enemy detection
    private object EnemyInRange(object o)
    {
        bool enemyFound = false;
        StaticEnemy enemy;
        Collider c = transform.Find("SensingRange").GetComponent<Collider>();
        nearbyEnemies.Clear();

        //Check if there are nearby enemies (including their field of view)
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (c.bounds.Intersects(go.gameObject.GetComponent<Collider>().bounds))
            {
                if (enemyFound == false)
                    enemyFound = true;

                enemy = go.gameObject.GetComponentInParent<StaticEnemy>();
                if (!nearbyEnemies.Contains(enemy))
                    nearbyEnemies.Add(enemy);
            }
        }

        Debug.Log("EnemyInRange:" + nearbyEnemies.Count.ToString());
        
        if (enemyFound == true) return true;
        return false;
    }

    private object EnemySpotted(object o)
    {
        if (nearbyEnemies.Count > 0)
        {
            for (int i = 0; i < nearbyEnemies.Count; i++)
            {
                if (nearbyEnemies[i].isSpotted == true) return true;
            }
        }
        return false;
    }

    private object EnemyVisible(object o)
    {
        bool enemyVisible = false;
        StaticEnemy enemy;
        Vector3 sphereCastPosition = new Vector3(transform.position.x, 0.5f, transform.position.z);
        RaycastHit[] leftHits = Physics.SphereCastAll(sphereCastPosition, 0.5f,
            Quaternion.Euler(0, -sightAngle, 0) * transform.forward);
        RaycastHit[] centerHits = Physics.SphereCastAll(sphereCastPosition, 0.5f, transform.forward);
        RaycastHit[] rightHits = Physics.SphereCastAll(sphereCastPosition, 0.5f,
            Quaternion.Euler(0, sightAngle, 0) * transform.forward);
        visibleEnemies.Clear();

        //Raycast debug
        Vector3 v = transform.TransformDirection(transform.forward) * 30;
        Debug.DrawRay(sphereCastPosition, Quaternion.Euler(0, -sightAngle, 0) * v, Color.blue, 5f);
        Debug.DrawRay(sphereCastPosition, v, Color.blue, 5f);
        Debug.DrawRay(sphereCastPosition, Quaternion.Euler(0, sightAngle, 0) * v, Color.blue, 5f);

        //Check if there are visible enemies (including their field of view)
        if (leftHits.Length > 0)
        {
            for (int i = 0; i < leftHits.Length; i++)
            {
                if (leftHits[i].collider.gameObject.tag == "Enemy")
                {
                    if (enemyVisible == false)
                        enemyVisible = true;

                    enemy = leftHits[i].collider.gameObject.GetComponentInParent<StaticEnemy>();
                    if (!visibleEnemies.Contains(enemy))
                        visibleEnemies.Add(enemy);
                }
            }
        }
        if (centerHits.Length > 0)
        {
            for (int i = 0; i < centerHits.Length; i++)
            {
                if (centerHits[i].collider.gameObject.tag == "Enemy")
                {
                    if (enemyVisible == false)
                        enemyVisible = true;

                    enemy = centerHits[i].collider.gameObject.GetComponentInParent<StaticEnemy>();
                    if (!visibleEnemies.Contains(enemy))
                        visibleEnemies.Add(enemy);
                }
            }
        }
        if (rightHits.Length > 0)
        {
            for (int i = 0; i < rightHits.Length; i++)
            {
                if (rightHits[i].collider.gameObject.tag == "Enemy")
                {
                    if (enemyVisible == false)
                        enemyVisible = true;

                    enemy = rightHits[i].collider.gameObject.GetComponentInParent<StaticEnemy>();
                    if (!visibleEnemies.Contains(enemy))
                        visibleEnemies.Add(enemy);
                }
            }
        }

        Debug.Log("EnemyVisible:" + visibleEnemies.Count.ToString());
        
        if (enemyVisible == true) return true;
        return false;
    }

    private object SpotEnemy(object o)
    {
        if (visibleEnemies.Count > 0)
        {
            for (int i = 0; i < visibleEnemies.Count; i++)
            {
                if (visibleEnemies[i].isSpotted == false)
                    visibleEnemies[i].isSpotted = true;
            }
        }
        return null;
    }

    private object AvoidEnemy(object o)
    {
        RaycastHit hit;
        Vector3 sphereCastPosition = new Vector3(transform.position.x, 0.5f, transform.position.z);
        bool leftHit = Physics.SphereCast(sphereCastPosition, 0.5f,
            Quaternion.Euler(0, -sightAngle, 0) * transform.forward, out hit);
        bool centerHit = Physics.SphereCast(sphereCastPosition, 0.5f, transform.forward, out hit);
        bool rightHit = Physics.SphereCast(sphereCastPosition, 0.5f,
            Quaternion.Euler(0, sightAngle, 0) * transform.forward, out hit);
        Vector3 left = Quaternion.Euler(0, -sightAngle, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, sightAngle, 0) * transform.forward;

        //Raycast debug
        Vector3 v = transform.TransformDirection(transform.forward) * 30;
        Debug.DrawRay(sphereCastPosition, Quaternion.Euler(0, -sightAngle, 0) * v, Color.red, 3f);
        Debug.DrawRay(sphereCastPosition, v, Color.red, 3f);
        Debug.DrawRay(sphereCastPosition, Quaternion.Euler(0, sightAngle, 0) * v, Color.red, 3f);

        //Collision avoidance 
        if (leftHit && centerHit && rightHit)
        {
            sightAngle += 5f;
            AvoidEnemy(o);
        }
        else
        {
            if (sightAngle > startingSightAngle)
                sightAngle = startingSightAngle;

            if (!leftHit && centerHit && rightHit)
            {
                agent.isStopped = true;
                agent.Move(left * agent.speed * Time.fixedDeltaTime);
                Debug.Log("Left");
            }

            if (leftHit && centerHit && !rightHit)
            {
                agent.isStopped = true;
                agent.Move(right * agent.speed * Time.fixedDeltaTime);
                Debug.Log("Right");
            }

            else
                agent.isStopped = false;
        }
        
        Debug.Log("AvoidEnemy");
        return null;
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
