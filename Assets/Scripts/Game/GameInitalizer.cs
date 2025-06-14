using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitalizer : MonoBehaviour
{

    public MatchManager matchManager;

    private void Start()
    {
        // This kicks off ListenToMatch() and TryJoinMatch()
        matchManager.StartMatchmaking();
    }
}
