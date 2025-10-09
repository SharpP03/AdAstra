using System.Collections.Generic;
using UnityEngine;

public class GraphTool : MonoBehaviour
{
    [SerializeField]
    private int pointsCount = 10;
    private float areaSize = 200f;
    private float offsetMultiplier = 5f;
    private float objectSizeMultiplier = .5f;
    [SerializeField]
    GameObject objectToSpawn;

    Vector3[] points;

    public class Node
    {
        public Vector3 position;
        public List<Node> connections;

        public Node(Vector3 pos)
        {
            position = pos;
            connections = new List<Node>();
        }
    }

    void Start()
    {
        // 1. rozproszyæ punkty w przestrzeni 3d
        points = GeneratePoints(pointsCount);
        // 2. utworzyæ po³¹czenia pomiêdzy nimi
        //GenerateNodes();
        Node nodeA = new Node(new Vector3(1, 2, 3));
        Node nodeB = new Node(new Vector3(1, 200, 3));
        SpawnBetweenNodes(nodeA,nodeB,objectToSpawn,50);
    }

    void Update()
    {
        
    }

    private void GenerateNodes() { 


    }

    private void SpawnBetweenNodes(Node nodeA,Node nodeB, GameObject objectToSpawn, int objectsCount) {

        for (int i = 0; i < objectsCount; i++)
        {
            // dyskretyzacja obszaru
            float t = i / (float)(objectsCount - 1);
            Vector3 offset = Random.insideUnitSphere * offsetMultiplier;
            Vector3 spawnPosition = Vector3.Lerp(nodeA.position, nodeB.position, t);
            
            // zmiana skali i rotacji
            GameObject newObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            newObject.transform.localScale = new Vector3(
                Random.Range(1,10)*objectSizeMultiplier,
                Random.Range(1,10)*objectSizeMultiplier,
                Random.Range(1,10)*objectSizeMultiplier
                );
            newObject.transform.localRotation = new Quaternion(
                Random.Range(1,10)*objectSizeMultiplier,
                Random.Range(1, 10) * objectSizeMultiplier,
                Random.Range(1, 10) * objectSizeMultiplier,
                Random.Range(1, 10) * objectSizeMultiplier
                );
        }
    }

    private Vector3[] GeneratePoints(int count) {

        Vector3[] points = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            points[i] = new Vector3(
            Random.Range(-areaSize/2,areaSize/2),
            Random.Range(-areaSize/2,areaSize/2),
            Random.Range(-areaSize/2,areaSize/2)
            );
        }

        return points;
    }
}
