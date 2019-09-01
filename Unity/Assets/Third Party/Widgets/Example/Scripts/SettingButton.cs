using UnityEngine;
using System.Collections;

using eeGames.Actor;

public class SettingButton : MonoBehaviour {

    public Actor ToggleButtonActor;
    public void OnToggleSet(bool value) 
    {
        if (value)
        {
            ToggleButtonActor.PerformActing();
            
        }
        else
        {
            ToggleButtonActor.PerformReverseActing();
            
        } 
    }
}
