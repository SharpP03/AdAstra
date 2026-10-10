using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool of jump authorization words typed in the Warp Gate terminal.
/// Words are kept as uppercase A-Z so they can be typed on any keyboard layout.
/// </summary>
[CreateAssetMenu(fileName = "WarpGate_WordPool", menuName = "AdAstra/Warp Gate Word Pool")]
public class WarpGateWordPoolSO : ScriptableObject
{
    [Tooltip("Authorization words, letters A-Z only (about 6-10 characters).")]
    [SerializeField] private List<string> words = new List<string> { "HYPERDRIVE" };

    public string GetRandomWord()
    {
        if (words.Count == 0) return "JUMP";
        return words[Random.Range(0, words.Count)];
    }

    private void OnValidate()
    {
        for (int i = 0; i < words.Count; i++)
        {
            words[i] = Sanitize(words[i]);
        }
    }

    private static string Sanitize(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        var chars = new System.Text.StringBuilder(word.Length);
        foreach (char c in word.ToUpperInvariant())
        {
            if (c >= 'A' && c <= 'Z') chars.Append(c);
        }
        return chars.ToString();
    }
}
