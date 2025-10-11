using System.Collections.Generic;
using UnityEngine;

public class BranchingTree : MonoBehaviour
{
    public enum GraphDirection { Down, Up, Right, Left, Forward, Backward }

    [SerializeField] private int maxDepth = 5;           // max levels
    [SerializeField] private float spacing = 20f;        // distance between nodes
    [SerializeField] private float randomOffset = 5f;    // random scatter
    [SerializeField] private GraphDirection direction = GraphDirection.Down;
    [SerializeField] private GameObject objectToSpawn;

    private class Node
    {
        public Vector3 position;
        public Node parent;
        public List<Node> children = new List<Node>();
    }

    private List<Node> allNodes = new List<Node>();

    void Start()
    {
        Node root = new Node { position = transform.position };
        allNodes.Add(root);
        GenerateChildren(root, 1);
        SpawnObjects();
    }

    void GenerateChildren(Node parent, int depth)
    {
        if (depth > maxDepth) return;

        // Determine number of children based on depth
        int childrenCount;
        if (depth == 1) childrenCount = Random.Range(2, 4);      // top: more branches
        else if (depth < maxDepth - 1) childrenCount = Random.Range(1, 3); // middle: some branches
        else childrenCount = 1;                                   // bottom: narrow

        float middle = (childrenCount - 1) / 2f;

        for (int i = 0; i < childrenCount; i++)
        {
            Vector3 offset = Vector3.zero;
            switch (direction)
            {
                case GraphDirection.Down: offset = new Vector3((i - middle) * spacing, -spacing, 0); break;
                case GraphDirection.Up: offset = new Vector3((i - middle) * spacing, spacing, 0); break;
                case GraphDirection.Right: offset = new Vector3(spacing, 0, (i - middle) * spacing); break;
                case GraphDirection.Left: offset = new Vector3(-spacing, 0, (i - middle) * spacing); break;
                case GraphDirection.Forward: offset = new Vector3((i - middle) * spacing, 0, spacing); break;
                case GraphDirection.Backward: offset = new Vector3((i - middle) * spacing, 0, -spacing); break;
            }

            // Add small random scatter
            offset += new Vector3(
                Random.Range(-randomOffset, randomOffset),
                Random.Range(-randomOffset, randomOffset),
                Random.Range(-randomOffset, randomOffset)
            );

            Node child = new Node
            {
                position = parent.position + offset,
                parent = parent
            };
            parent.children.Add(child);
            allNodes.Add(child);

            GenerateChildren(child, depth + 1);
        }
    }

    void SpawnObjects()
    {
        foreach (var node in allNodes)
        {
            if (node.parent != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    float t = i / 4f;
                    Vector3 spawnPos = Vector3.Lerp(node.parent.position, node.position, t);
                    spawnPos += Random.insideUnitSphere * 2f;
                    Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (allNodes == null || allNodes.Count == 0) return;

        Gizmos.color = Color.green;
        foreach (var node in allNodes)
        {
            Gizmos.DrawSphere(node.position, 1f);
            foreach (var child in node.children)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(node.position, child.position);
            }
        }
    }
}
