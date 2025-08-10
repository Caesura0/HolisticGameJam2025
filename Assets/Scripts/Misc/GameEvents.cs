using System;
using UnityEngine;

public static class GameEvents
{
    // Fires the first time Granny eats an NPC (the reveal trigger)
    public static event Action OnFirstEat;
    public static void RaiseFirstEat() => OnFirstEat?.Invoke();


    //TODO: Change int to enum
    public static event Action<int> OnPhaseChanged;
    public static void RaisePhaseChanged(int newPhase) => OnPhaseChanged?.Invoke(newPhase);
}
