using System;
using UnityEngine;

public static class GameEvents
{
    // Fires the first time Granny eats an NPC (the reveal trigger)
    public static event Action OnFirstKill;
    public static event Action<InteractableItem> OnDisarmed;
    public static event Action<ItemData> OnCheckedIn;
    public static void RaiseFirstEat() => OnFirstKill?.Invoke();
    public static void InvokeOnDisarmed(InteractableItem target) => OnDisarmed?.Invoke(target);
    public static void CheckIn(ItemData checkInData) => OnCheckedIn?.Invoke(checkInData);
    //TODO: Change int to enum
    public static event Action<int> OnPhaseChanged;
    public static void RaisePhaseChanged(int newPhase) => OnPhaseChanged?.Invoke(newPhase);
}
