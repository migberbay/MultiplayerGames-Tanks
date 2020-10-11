using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 4;            // The number of rounds a single player has to win to win the game.
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;             // Reference to the prefab the players will control.
        public TankManager[] m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks.
        public int m_NumPlayers;                    // Maximum number of players (min = 2, max = 4);
        private Camera m_MainCamera;                // Reference to Global Camera.

        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.
        private float maxDistanceBetweenTanks = 0f; // The maximum distance between the tanks 
        public int cameraSwapCheckFrequency = 60;   // How often we check for tanks to be close to change the camera type, the higher the better performance.
        private int currentCameraSwapCount = 0;     // The current number to check for camera swap.
        public float distanceToSwapToGlobalCamera = 25f; // The distance we choose to swap from global to player cam.

        public Camera[] m_Cameras;                  // Camera Array for tanks
        public CinemachineVirtualCamera[] m_VitualCameras;  // Virtual Cinemachine Camera array.

        private bool CanAddAnother;

        private void Start()
        {
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds (m_StartDelay);
            m_EndWait = new WaitForSeconds (m_EndDelay);

            SpawnAllTanks();
            SetCameraTargets();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine (GameLoop ());

            // Limit the number of players to 2 - 4 just in case.
            if(m_NumPlayers > 4) m_NumPlayers = 4;
            else if(m_NumPlayers < 2) m_NumPlayers = 2;
           
            // Number of rounds varies depending on how many players there are. (4 for 2 players, 3 for 3 , 2 for 4).
            m_NumRoundsToWin = (int)9/m_NumPlayers;
        }


        private void SpawnAllTanks()
        {   
            m_MainCamera = GameObject.Find ("Main Camera").GetComponent<Camera>();

            // For all the tanks...
            for (int i = 0; i < m_NumPlayers; i++)
            {
                // ... create them, set their player number and references needed for control.
                m_Tanks[i].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
                AddCamera(i, m_NumPlayers);
            }
            // In case we are 3 players we activate the minimap camera.
            if(m_NumPlayers == 3) m_Cameras[4].gameObject.SetActive(true);
            m_MainCamera.gameObject.SetActive(false);
        }

        private void AddCamera(int i, int totaltanks){
            Camera newCam = m_Cameras[i];
            newCam.gameObject.SetActive(true);
            
            newCam.transform.parent = m_Tanks[i].m_Instance.transform;

            m_VitualCameras[i].Follow = m_Tanks[i].m_Instance.transform;
            m_VitualCameras[i].LookAt = m_Tanks[i].m_Instance.transform;

            float width = 1.0f, height = 0.5f, xPosOdd = 0.0f, xPosEven = 0.0f, yPosTop = 0.5f, yPosBottom =  0.0f;

            if (m_NumPlayers >=3){
                width = 0.5f; xPosOdd = 0.5f;
                if (i%2 == 0){
                    if (i == 0) newCam.rect = new Rect (xPosEven, yPosTop, width, height);//
                    else newCam.rect = new Rect (xPosEven, yPosBottom, width, height);
                }else{
                    if(i == 1) newCam.rect = new Rect (xPosOdd, yPosTop, width, height);
                    else newCam.rect = new Rect (xPosOdd, yPosBottom, width, height);
                } 
            }
            else if(m_NumPlayers == 2){
                if(i == 0) 
                    newCam.rect = new Rect (xPosEven, yPosTop, width, height);
                else
                    newCam.rect = new Rect (xPosEven, yPosBottom, width, height);
            }
        }

        private void AdjustCameras (){
            float width = 0.5f, height = 0.5f, xPosOdd = 0.5f, xPosEven = 0.0f, yPosTop = 0.5f, yPosBottom =  0.0f;

            for (int i = 0; i < m_NumPlayers; i++){
                if (i%2 == 0){
                    if (i == 0) m_Cameras[i].rect = new Rect (xPosEven, yPosTop, width, height);//
                    else m_Cameras[i].rect = new Rect (xPosEven, yPosBottom, width, height);
                }else{
                    if(i == 1) m_Cameras[i].rect = new Rect (xPosOdd, yPosTop, width, height);
                    else m_Cameras[i].rect = new Rect (xPosOdd, yPosBottom, width, height);
                } 
            }
            if(m_NumPlayers == 3){m_Cameras[4].gameObject.SetActive(true);}
            if(m_NumPlayers == 4){m_Cameras[4].gameObject.SetActive(false);}
            // We add the new tank to the global camera.
            SetCameraTargets();
        }


        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks playing .
            Transform[] targets = new Transform[m_NumPlayers];

            // For each of these transforms...
            for (int i = 0; i < targets.Length; i++)
            {
                // ... set it to the appropriate tank transform.
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow.
            m_CameraControl.m_Targets = targets;
        }


        // This is called from start and will run each phase of the game one after another.
        private IEnumerator GameLoop ()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine (RoundStarting ());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine (RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine (RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                // If there is a game winner, go back to main menu.
                SceneManager.LoadScene (0);
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine (GameLoop ());
            }
        }

        

        private IEnumerator RoundStarting ()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks ();
            DisableTankControl ();

            // Allows adding extra tanks to the game mid round (limited to 1 per round.)
            if(m_NumPlayers == 4){
                CanAddAnother = false;
            }else{
                CanAddAnother = true;
            } 

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize ();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying ()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl ();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;
                
            // While there is not one tank left...
            while (!OneTankLeft())
            {   
                // Adding an extra tank
                if(Input.GetKey(KeyCode.A) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && CanAddAnother){
                    AddPlayerDuringRuntime();
                    CanAddAnother = false;
                }
                // Checking for tank closeness
                if(currentCameraSwapCount >= cameraSwapCheckFrequency){
                    GlobalVsPlayerCameras();
                    currentCameraSwapCount = 0;
                }
                currentCameraSwapCount++;
                // ... return on the next frame.
                yield return null;
            }
        }
        
        private void GlobalVsPlayerCameras(){
            /*
                We check for player distance to choose which cameras will be active
                activate and deactivate cameras accordingly.
            */

            // Checking for max distance between tanks.
            maxDistanceBetweenTanks = 0;
            for (int i = 0; i < m_NumPlayers; i++){
                if(m_Tanks[i].m_Instance.gameObject.activeInHierarchy){
                    for (int j = 0; j < m_NumPlayers; j++){
                        // we dont want to check the distance to itself.
                        // nor with a tank that's not active right now
                        if(j!=i && m_Tanks[j].m_Instance.gameObject.activeInHierarchy){
                            float dist = (m_Tanks[i].m_Instance.transform.position - m_Tanks[j].m_Instance.transform.position).magnitude;
                            if(dist > maxDistanceBetweenTanks){
                                maxDistanceBetweenTanks = dist;
                            }
                        }
                    }
                }
            }

            // Activate and deactivate cameras.
            if(maxDistanceBetweenTanks <= distanceToSwapToGlobalCamera){
                //Change to global camera
                m_MainCamera.gameObject.SetActive(true);
                foreach (var cam in m_Cameras){
                    // 9th layer == player layer.
                    if(cam.transform.parent.gameObject.layer == 9)
                        cam.gameObject.SetActive(false);
                }
                if(m_NumPlayers == 3)
                    // turn off minimapCamera
                    m_Cameras[4].gameObject.SetActive(false);
            }else{
                m_MainCamera.gameObject.SetActive(false);
                foreach (var cam in m_Cameras){
                    if(cam.transform.parent.gameObject.layer == 9)
                        cam.gameObject.SetActive(true);
                }
                if(m_NumPlayers == 3)
                    // turn on minimapCamera
                    m_Cameras[4].gameObject.SetActive(true);
            }
        }


        private void AddPlayerDuringRuntime(){
            /* We need to add a tank to the corresponding checkpoint
                Enable it to move
                Assing its camera to it, and enable the camera.
                Readjust the viewports accordingly.
                And update the number of tanks playing.
            */

            // Adding a new tank to the scene on its corresponding checkpoint
            m_Tanks[m_NumPlayers].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[m_NumPlayers].m_SpawnPoint.position, m_Tanks[m_NumPlayers].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[m_NumPlayers].m_PlayerNumber = m_NumPlayers + 1;
            m_Tanks[m_NumPlayers].Setup();
            AddCamera(m_NumPlayers, m_NumPlayers + 1);
            m_NumPlayers += 1;
            AdjustCameras();
            Debug.Log("Adding Another Tank");
        }

        private IEnumerator RoundEnding ()
        {
            // Stop tanks from moving.
            DisableTankControl ();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner ();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner ();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage ();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end.
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_NumPlayers; i++)
            {
                // ... and if they are active, increment the counter.
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }
        
        
        // This function is to find out if there is a winner of the round.
        // This function is called with the assumption that 1 or fewer tanks are currently active.
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_NumPlayers; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_NumPlayers; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < m_NumPlayers; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_NumPlayers; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_NumPlayers; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_NumPlayers; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }
    }   
}