using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Player_Spaceship Player { get; private set; }


    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); Debug.LogError("A duplicate of GameManager was attempted to create"); return; }
        Instance = this; // assign the singleton reference 
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayer(Player_Spaceship player)
    {
        Player = player;
    }
}
