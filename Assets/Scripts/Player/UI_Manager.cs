using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    private FuelSystem fuelSystem;
    [SerializeField]
    private Slider fuelBar;
    //private Canvas playerUI;

    private void Awake()
    {
        //playerUI = GameManager.Instance.Player.GetComponent<Canvas>();
        //fuelSystem = GameManager.Instance.Player.GetComponent<FuelSystem>();
        fuelSystem = GetComponent<FuelSystem>();
        fuelBar.maxValue = fuelSystem.maxFuel;
    }

    void Start()
    {
        
    }

    void Update()
    {
        fuelBar.value = fuelSystem.currentFuel;
    }

}
