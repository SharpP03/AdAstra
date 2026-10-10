using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Composition root and game-state orchestrator: tracks the run, drives the Warp Gate jump procedure,
/// ends the run on victory or death and restarts it by reloading the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Player_Spaceship Player { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [SerializeField] private AnnihilationWaveController annihilationWave;
    [SerializeField] private RunEndPanel runEndPanel;
    [SerializeField] private WarpGate warpGate;
    [SerializeField] private JumpTerminal jumpTerminal;
    [SerializeField] private DynamicCamera dynamicCamera;
    [SerializeField] private ScreenFade screenFade;

    [Tooltip("Time in seconds the white flash takes to clear after the jump, revealing the summary.")]
    [SerializeField] private float jumpFadeOutDuration = 0.6f;

    private HealthSystem playerHealth;
    private float runStartTime;
    private float runEndTime;
    private bool jumpAuthorized;
    private float closestWaveDistance = float.MaxValue;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); Debug.LogError("A duplicate of GameManager was attempted to create"); return; }
        Instance = this; // assign the singleton reference
        // No DontDestroyOnLoad: restarting a run reloads the scene, and the scene owns a fresh GameManager.
    }

    private void OnEnable()
    {
        if (annihilationWave != null) annihilationWave.OnDistanceChanged += HandleWaveDistanceChanged;
        if (runEndPanel != null) runEndPanel.OnRestartRequested += RestartRun;
        if (playerHealth != null) playerHealth.OnDied += HandlePlayerDied;
        if (warpGate != null) warpGate.OnShipDocked += HandleShipDocked;
        if (warpGate != null) warpGate.OnJumpFinished += HandleJumpFinished;
        if (jumpTerminal != null) jumpTerminal.OnWordCompleted += HandleJumpAuthorized;
    }

    private void OnDisable()
    {
        if (annihilationWave != null) annihilationWave.OnDistanceChanged -= HandleWaveDistanceChanged;
        if (runEndPanel != null) runEndPanel.OnRestartRequested -= RestartRun;
        if (playerHealth != null) playerHealth.OnDied -= HandlePlayerDied;
        if (warpGate != null) warpGate.OnShipDocked -= HandleShipDocked;
        if (warpGate != null) warpGate.OnJumpFinished -= HandleJumpFinished;
        if (jumpTerminal != null) jumpTerminal.OnWordCompleted -= HandleJumpAuthorized;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        runStartTime = Time.time;
        CurrentState = GameState.Playing;
    }

    public void RegisterPlayer(Player_Spaceship player)
    {
        Player = player;

        if (playerHealth != null) playerHealth.OnDied -= HandlePlayerDied;
        playerHealth = player.GetComponent<HealthSystem>();
        if (playerHealth != null) playerHealth.OnDied += HandlePlayerDied;
    }

    private void HandleWaveDistanceChanged(float distance)
    {
        if (CurrentState != GameState.Playing || jumpAuthorized) return;
        closestWaveDistance = Mathf.Min(closestWaveDistance, distance);
    }

    private void HandlePlayerDied()
    {
        // Once the jump is authorized the run is won; leftover wave damage during the jump does not count.
        if (jumpAuthorized) return;
        EndRun(GameState.GameOver);
    }

    // The wave keeps moving and dealing damage while the player types the authorization word.
    private void HandleShipDocked(Player_Spaceship ship)
    {
        if (CurrentState != GameState.Playing || jumpTerminal == null) return;
        jumpTerminal.Open(warpGate.PickAuthorizationWord());
    }

    /// <summary>
    /// Correct word typed: the run clock and the wave stop, and the jump sequence starts
    /// (ship pulled through the portal, camera FOV kick, fade to white).
    /// </summary>
    private void HandleJumpAuthorized()
    {
        if (CurrentState != GameState.Playing || jumpAuthorized) return;
        jumpAuthorized = true;
        runEndTime = Time.time;

        if (annihilationWave != null) annihilationWave.SetActive(false);
        jumpTerminal.Close();

        float duration = warpGate.JumpDuration;
        warpGate.Jump(Player);
        if (dynamicCamera != null) dynamicCamera.PlayWarpEffect(duration);
        if (screenFade != null) screenFade.FadeIn(duration);
    }

    private void HandleJumpFinished()
    {
        EndRun(GameState.Victory);
        if (screenFade != null) screenFade.FadeOut(jumpFadeOutDuration);
    }

    /// <summary>
    /// Ends the run once: locks the ship controls, freezes the run statistics and shows the end-of-run panel.
    /// </summary>
    public void EndRun(GameState result)
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = result;

        if (Player != null) Player.SetControlsEnabled(false);
        if (result == GameState.Victory && annihilationWave != null) annihilationWave.SetActive(false);
        if (jumpTerminal != null && jumpTerminal.IsOpen) jumpTerminal.Close();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (runEndPanel != null) runEndPanel.Show(BuildSummary(result));
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private RunSummary BuildSummary(GameState result)
    {
        float hull = 0f;
        float fuel = 0f;
        if (Player != null)
        {
            if (playerHealth != null) hull = playerHealth.currentHealth / playerHealth.MaxHealth;
            var fuelSystem = Player.GetComponent<FuelSystem>();
            if (fuelSystem != null) fuel = fuelSystem.FuelPercent;
        }

        // The escape time ends at the jump authorization, not after the jump animation.
        float endTime = jumpAuthorized ? runEndTime : Time.time;
        return new RunSummary(result, endTime - runStartTime, hull, fuel, Mathf.Max(0f, closestWaveDistance));
    }
}
