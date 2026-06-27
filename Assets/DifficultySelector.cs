using UnityEngine;
using TMPro;

public class DifficultySelector : MonoBehaviour
{
    public void OnDifficultyChanged(int index)
    {
        GameSettings.SelectedDifficulty = (GameSettings.Difficulty)index;
        // This will prove the UI is talking to the code
        Debug.Log("BRIDGE UPDATE: Difficulty is now " + GameSettings.SelectedDifficulty);
    }
}