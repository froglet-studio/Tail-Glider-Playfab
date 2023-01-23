using StarWriter.Core;
using System;
using UnityEngine;
using StarWriter.Core.Input;

public class FuelSystem : MonoBehaviour
{
    #region Events
    public delegate void OnFuelChangeEvent(float amount);
    public static event OnFuelChangeEvent OnFuelChange;

    public delegate void OnFuelEmptyEvent();
    public static event OnFuelEmptyEvent OnFuelEmpty;
    #endregion

    #region Floats
    [Tooltip("Initial and Max fuel level from 0-1")]
    [SerializeField]
    [Range(0, 1)]
    static float maxFuel = 1f;
    static float currentFuel; // TODO: this should be part of ShipData
    #endregion

    public static float CurrentFuel { 
        get => currentFuel; 
        private set 
        { 
            currentFuel = value; 
            OnFuelChange?.Invoke(currentFuel);
            if (currentFuel <= 0) OnFuelEmpty?.Invoke();
        }
    }

    public static void ResetFuelEmptyListeners()
    {
        foreach (Delegate d in OnFuelEmpty.GetInvocationList())
        {
            OnFuelEmpty -= (OnFuelEmptyEvent)d;
        }
    }

    void OnEnable()
    {
        GameManager.onExtendGamePlay += ResetFuel;
        Skimmer.OnSkim += ChangeFuelAmount;
        InputController.OnBoost += ChangeFuelAmount;
    }

    void OnDisable()
    {
        GameManager.onExtendGamePlay -= ResetFuel;
        Skimmer.OnSkim -= ChangeFuelAmount;
        InputController.OnBoost -= ChangeFuelAmount;
    }

    void Start()
    {
        ResetFuel();
    }

    public static void ResetFuel()
    {
        CurrentFuel = maxFuel;
    }

    public static void ChangeFuelAmount(string uuid, float amount)
    {
        CurrentFuel = Mathf.Clamp(currentFuel + amount, 0, 1);
    }
}