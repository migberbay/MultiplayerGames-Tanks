using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Complete;

public class SceneLoadSignal : MonoBehaviour
{
    public MultiplayerManager mManager;
    public GameManager gManager;
    private void Awake() {
        mManager = GameObject.Find("MultiplayerManager").GetComponent<MultiplayerManager>();
        gManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //Load a reference to the game Manager for the multiplayer manager to use.
        mManager.gameManager = gManager;

        //Setup the number of players from the main menu.
        gManager.m_NumPlayers = mManager.NumPlayers;
        
        Destroy(this.gameObject);
    }
}
