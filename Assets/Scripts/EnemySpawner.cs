using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int enemyNumber = 10;
    public GameObject enemyPrefab;
    public List<Transform> enemySpawnPositions = new List<Transform>();

    /*void Awake()
    {
        List<int> spawnPositions = Enumerable.Range(0, enemySpawnPositions.Count).ToList<int>();
        for (int i=0; i<enemyNumber; i++)
        {
            //Set enemy spawn positions
            int ri = Random.Range(0, enemySpawnPositions.Count - i);
            Transform position = enemySpawnPositions[spawnPositions[ri]];
            spawnPositions.RemoveAt(ri);
            int rotationAngle = Random.Range(0, 9) * 45;
            Quaternion rotation = Quaternion.identity * Quaternion.AngleAxis(rotationAngle, transform.up);
            Instantiate(enemyPrefab, position.position, rotation);
            
            //Rotate enemy if its field of view intersects an obstacle
            Collider c = enemyPrefab.GetComponentInChildren<Collider>();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Obstacle"))
            {
                while (c.bounds.Intersects(go.gameObject.GetComponent<Collider>().bounds))
                    enemyPrefab.transform.Rotate(0, rotationAngle, 0);
            }
        }
    }*/
}