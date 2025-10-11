using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GraphTool : MonoBehaviour
{
    [SerializeField] private int pointsCount = 10;
    [SerializeField] private float areaSize = 200f;
    [SerializeField] private int connectionsPerNode = 3;
    [SerializeField] private int objectsBetweenNodes = 10;
    [SerializeField] private float offsetMultiplier = 5f;
    [SerializeField] private float objectSizeMultiplier = 0.5f;
    [SerializeField] private GameObject objectToSpawn;

    private Vector3[] points;
    private List<Node> nodes;

    public class Node
    {
        public Vector3 position;
        public List<Node> connections;
        public Node(Vector3 pos) { position = pos; connections = new List<Node>(); }
    }

    public class NodeDistance
    {
        public Node nodeA;
        public Node nodeB;
        public float distance;
        public override string ToString() => $"A:{nodeA.position} B:{nodeB.position} D:{distance:F2}";
    }

    void Start()
    {
        points = GeneratePoints(pointsCount);
        nodes = ConvertToNodes();
        ConnectClosestNodes();
    }

    private List<Node> ConvertToNodes()
    {
        List<Node> newNodes = new List<Node>();
        foreach (var pos in points) newNodes.Add(new Node(pos));
        return newNodes;
    }

    private void ConnectClosestNodes()
    {
        List<NodeDistance> distances = new List<NodeDistance>();
        for (int i = 0; i < nodes.Count; i++)
            for (int j = i + 1; j < nodes.Count; j++)
                distances.Add(new NodeDistance
                {
                    nodeA = nodes[i],
                    nodeB = nodes[j],
                    distance = Vector3.Distance(nodes[i].position, nodes[j].position)
                });

        foreach (var currentNode in nodes)
        {
            List<NodeDistance> related = distances.FindAll(d => d.nodeA == currentNode || d.nodeB == currentNode);
            related.Sort((a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < Mathf.Min(connectionsPerNode, related.Count); i++)
            {
                Node other = (related[i].nodeA == currentNode) ? related[i].nodeB : related[i].nodeA;
                if (!currentNode.connections.Contains(other)) currentNode.connections.Add(other);
                if (!other.connections.Contains(currentNode)) other.connections.Add(currentNode);
                SpawnBetweenNodes(currentNode, other, objectToSpawn, objectsBetweenNodes);

            }
        }
    }

    private void SpawnBetweenNodes(Node nodeA, Node nodeB, GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 offset = Random.insideUnitSphere * offsetMultiplier;
            Vector3 spawnPosition = Vector3.Lerp(nodeA.position, nodeB.position, t) + offset;

            GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity, GameObject.Find("GraphConatiner").transform);
            obj.transform.localScale = Vector3.one * Random.Range(1f, 5f) * objectSizeMultiplier;
        }
    }

    private Vector3[] GeneratePoints(int count)
    {
        Vector3[] pts = new Vector3[count];
        for (int i = 0; i < count; i++)
            pts[i] = new Vector3(
                Random.Range(-areaSize / 2, areaSize / 2),
                Random.Range(-areaSize / 2, areaSize / 2),
                Random.Range(-areaSize / 2, areaSize / 2)
            );
        return pts;
    }

    private void OnDrawGizmos()
    {
        if (nodes == null) return;

        Gizmos.color = Color.green;
        foreach (var node in nodes)
        {
            // Draw node
            Gizmos.DrawSphere(node.position, 1f);

            // Draw connections
            Gizmos.color = Color.yellow;
            foreach (var connectedNode in node.connections)
            {
                Gizmos.DrawLine(node.position, connectedNode.position);
            }
        }
    }
}
