using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// End-of-run panel shared by victory and game over: shows the run summary and requests a restart.
/// </summary>
public class RunEndPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button restartButton;

    public event Action OnRestartRequested;

    private void Awake()
    {
        panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        restartButton.onClick.AddListener(HandleRestartClicked);
    }

    private void OnDisable()
    {
        restartButton.onClick.RemoveListener(HandleRestartClicked);
    }

    public void Show(RunSummary summary)
    {
        bool victory = summary.Result == GameState.Victory;
        titleText.text = victory ? "JUMP COMPLETE" : "SHIP LOST";

        TimeSpan duration = TimeSpan.FromSeconds(summary.Duration);
        string closest = summary.ClosestWaveDistance < 100000f ? $"{summary.ClosestWaveDistance:F0} m" : "-";
        statsText.text =
            $"Escape time: {duration:mm\\:ss}\n" +
            $"Hull: {summary.HullPercent:P0}\n" +
            $"Fuel: {summary.FuelPercent:P0}\n" +
            $"Closest to the wave: {closest}";

        panelRoot.SetActive(true);
    }

    private void HandleRestartClicked()
    {
        OnRestartRequested?.Invoke();
    }
}
