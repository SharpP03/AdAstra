using UnityEngine;
using System.Collections.Generic;

public class ProceduralGraph : MonoBehaviour
{
    [Header("Graph Generation Settings")]
    public int nodeCount = 20;
    public float areaSize = 100f;          // total cube space to spawn nodes in
    public int connectionsPerNode = 3;     // average branching factor
    public float connectionDistance = 40f; // max allowed distance for a connection

    [Header("Visual / Debris Settings")]
    public GameObject nodePrefab;          // optional visual for node
    public GameObject debrisPrefab;        // what to spawn along connections
    public int debrisPerConnection = 50;
    public float debrisScatterRadius = 5f;

    private List<Vector3> nodes = new List<Vector3>();
    private HashSet<(int, int)> connectedPairs = new HashSet<(int, int)>();

    private void Start()
    {
        GenerateNodes();
        ConnectNodes();
        SpawnDebrisAlongConnections();
    }

    private void GenerateNodes()
    {
        nodes.Clear();
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-areaSize / 2f, areaSize / 2f),
                Random.Range(-areaSize / 2f, areaSize / 2f),
                Random.Range(-areaSize / 2f, areaSize / 2f)
            );
            nodes.Add(pos);

            if (nodePrefab)
                Instantiate(nodePrefab, pos, Quaternion.identity, transform);
        }
    }

    private void ConnectNodes()
    {
        connectedPairs.Clear();

        for (int i = 0; i < nodes.Count; i++)
        {
            int connections = Random.Range(1, connectionsPerNode + 1);

            // find nearest candidates
            List<int> candidateIndices = GetClosestNodes(i, connections * 2);
            int added = 0;

            foreach (int j in candidateIndices)
            {
                if (i == j || added >= connections) break;
                if (Vector3.Distance(nodes[i], nodes[j]) > connectionDistance) continue;

                var pair = (Mathf.Min(i, j), Mathf.Max(i, j));
                if (connectedPairs.Contains(pair)) continue; // already connected

                connectedPairs.Add(pair);
                added++;
            }
        }
    }

    private List<int> GetClosestNodes(int index, int count)
    {
        List<(float dist, int idx)> distances = new List<(float, int)>();
        for (int j = 0; j < nodes.Count; j++)
        {
            if (j == index) continue;
            float d = Vector3.Distance(nodes[index], nodes[j]);
            distances.Add((d, j));
        }
        distances.Sort((a, b) => a.dist.CompareTo(b.dist));
        List<int> closest = new List<int>();
        for (int k = 0; k < Mathf.Min(count, distances.Count); k++)
            closest.Add(distances[k].idx);
        return closest;
    }

    private void SpawnDebrisAlongConnections()
    {
        foreach (var pair in connectedPairs)
        {
            Vector3 start = nodes[pair.Item1];
            Vector3 end = nodes[pair.Item2];

            for (int i = 0; i < debrisPerConnection; i++)
            {
                float t = Random.Range(0f, 1f);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                Vector3 offset = Random.insideUnitSphere * debrisScatterRadius;
                Vector3 pos = basePos + offset;

                Instantiate(debrisPrefab, pos, Random.rotation, transform);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (nodes == null || nodes.Count == 0) return;

        Gizmos.color = Color.yellow;
        foreach (var pair in connectedPairs)
        {
            Vector3 start = nodes[pair.Item1];
            Vector3 end = nodes[pair.Item2];
            Gizmos.DrawLine(start, end);
        }
    }
}
