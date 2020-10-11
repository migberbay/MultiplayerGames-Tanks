using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Complete;

public class MultiplayerManager : MonoBehaviour
{

    // number of players the game will start with.
    public int NumPlayers = 2;
    public Text PlayersNumText;
    public GameManager gameManager;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadGameScene(){
        SceneManager.LoadScene(1);
    }

    public void IncreasePlayers(){
        if(NumPlayers < 4){
            NumPlayers++;
            PlayersNumText.text = NumPlayers.ToString();
        }    
    }

    public void DecreasePlayers(){
         if(NumPlayers > 2){
            NumPlayers--;
            PlayersNumText.text = NumPlayers.ToString();
        }
    }
}
