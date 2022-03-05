/**** 
 * Created by: Akram Taghavi-Burrs
 * Date Created: Feb 23, 2022
 * 
 * Last Edited by: Krieger
 * Last Edited: Feb 28, 2022
 * 
 * Description: Basic GameManager Template for Grocery Bag Blitz
****/

/** Import Libraries **/
using System; //C# library for system properties
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //libraries for accessing scenes


public class GameManager : MonoBehaviour
{
    /*** VARIABLES ***/

    #region GameManager Singleton
    static private GameManager gm; //refence GameManager
    static public GameManager GM { get { return gm; } } //public access to read only gm 

    //Check to make sure only one gm of the GameManager is in the scene
    void CheckGameManagerIsInScene()
    {

        //Check if instnace is null
        if (gm == null)
        {
            gm = this; //set gm to this gm of the game object
            Debug.Log(gm);
        }
        else //else if gm is not null a Game Manager must already exsist
        {
            Destroy(this.gameObject); //In this case you need to delete this gm
        }
        DontDestroyOnLoad(this); //Do not delete the GameManager when scenes load
        Debug.Log(gm);
    }//end CheckGameManagerIsInScene()
    #endregion

    #region Variables
    [Header("GENERAL SETTINGS")]
    public string gameTitle = "Grocery Bag Blitz";  //name of the game
    public string gameCredits = "Made by Krieger (the great and powerful)"; //game creator(s)
    public string copyrightDate = "Copyright " + thisDay; //date created

    [Header("GAME SETTINGS")]

    [Tooltip("Will the high score be recoreded")]
    public bool recordHighScore = false; //is the High Score recorded

    [SerializeField] //Access to private variables in editor
    private int defaultHighScore = 1000;
    static public int highScore = 1000; // the default High Score
    public int HighScore { get { return highScore; } set { highScore = value; } }//access to private variable highScore [get/set methods]

    [Space(10)]

    static public int score = 0;  //score value
    public int Score { get { return score; } set { score = value; } }//access to private variable died [get/set methods]
    public int wavesSurvived; //number of waves the player has survived, used for job titles

    [SerializeField] //Access to private variables in editor
    [Tooltip("Check to test player lost the level")]
    private bool levelLost = false;//we have lost the level (ie. player died)
    public bool LevelLost { get { return levelLost; } set { levelLost = value; } } //access to private variable lostLevel [get/set methods]

    [Space(10)]
    public string defaultEndMessage = "Game Over";//the end screen message, depends on winning outcome
    public string looseMessage = "The Manager was called"; //Message if player loses
    public string winMessage = "Good Bagging!"; //Message if player wins
    [HideInInspector] public string endMsg;//the end screen message, depends on winning outcome

    [Header("SCENE SETTINGS")]
    [Tooltip("Name of the start scene")]
    public string startScene;

    [Tooltip("Name of the game over scene")]
    public string gameOverScene;

    [Tooltip("Count and name of each Game Level (scene)")]
    public string[] gameLevels; //names of levels
    [HideInInspector]
    public int gameLevelsCount; //what level we are on
    private int loadLevel; //what level from the array to load

    public static string currentSceneName; //the current scene name;

    [Header("FOR TESTING")]
    public bool nextLevel = false; //test for next level

    //Game State Varaiables
    [HideInInspector] public enum gameStates { Idle, Playing, Death, GameOver, BeatLevel };//enum of game states
    [HideInInspector] public gameStates gameState = gameStates.Idle;//current game state

    //Timer Varaibles
    private float currentTime; //sets current time for timer
    private bool gameStarted = false; //test if games has started

    //Win/Lose conditon
    [SerializeField] //to test in inspector
    private bool playerWon = false;

    //reference to system time
    private static string thisDay = System.DateTime.Now.ToString("yyyy"); //today's date as string

    //how many items currently exist
    public int numberOfItems = 0;

    //variables for spawning items
    [Space(5)]
    [Header("ITEM SPAWNING")]
    public List<GameObject> items = new List<GameObject>(); //list of item prefabs that will be spawned
    int itemsToSpawn = 5;
    public Vector3 itemSpawnPosition = new Vector3(-5f, 5f, 0f);

    [Space(5)]
    [Header("JOB TITLES")]
    //list of job titles
    public List<string> titles = new List<string>();
    public string currentTitle; //current title as a string
    private int titleIndex = 0; //index of the player's current title

    private int nextTitleRequirement = 1;

    #endregion


    /*** MEHTODS ***/

    //Awake is called when the game loads (before Start).  Awake only once during the lifetime of the script instance.
    void Awake()
    {
        //runs the method to check for the GameManager
        CheckGameManagerIsInScene();

        //store the current scene
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        //Get the saved high score
        GetHighScore();
        currentTitle = titles[0];
    }//end Awake()


    // Update is called once per frame
    private void Update()
    {
        //if ESC is pressed , exit game
        if (Input.GetKey("escape")) { ExitGame(); }

        //if we are playing the game
        if (gameState == gameStates.Playing)
        {
            //if we have died and have no more lives, go to game over
            if (levelLost) { GameOver(); }

        }//end if (gameState == gameStates.Playing)

        //Check Score
        CheckScore();



    }//end Update


    //LOAD THE GAME FOR THE FIRST TIME OR RESTART
    public void StartGame()
    {
        //SET ALL GAME LEVEL VARIABLES FOR START OF GAME

        gameLevelsCount = 1; //set the count for the game levels
        loadLevel = gameLevelsCount - 1; //the level from the array
        SceneManager.LoadScene(gameLevels[loadLevel]); //load first game level

        gameState = gameStates.Playing; //set the game state to playing

        score = 0; //set starting score

        //set High Score
        if (recordHighScore) //if we are recording highscore
        {
            //if the high score, is less than the default high score
            if (highScore <= defaultHighScore)
            {
                highScore = defaultHighScore; //set the high score to defulat
                PlayerPrefs.SetInt("HighScore", highScore); //update high score PlayerPref
            }//end if (highScore <= defaultHighScore)
        }//end  if (recordHighScore) 

        endMsg = defaultEndMessage; //set the end message default

        playerWon = false; //set player winning condition to false
    }//end StartGame()



    //EXIT THE GAME
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exited Game");
    }//end ExitGame()


    //GO TO THE GAME OVER SCENE
    public void GameOver()
    {
        gameState = gameStates.GameOver; //set the game state to gameOver

        if (playerWon) { endMsg = winMessage; } else { endMsg = looseMessage; } //set the end message

        SceneManager.LoadScene(gameOverScene); //load the game over scene
        Debug.Log("Gameover");
    }

    void CheckScore()
    { //This method manages the score on update. Right now it just checks if we are greater than the high score.
        //if the score is more than the high score
        if (score > highScore)
        {
            highScore = score; //set the high score to the current score
            PlayerPrefs.SetInt("HighScore", highScore); //set the playerPref for the high score
        }//end if(score > highScore)

    }//end CheckScore()

    void GetHighScore()
    {//Get the saved highscore

        //if the PlayerPref alredy exists for the high score
        if (PlayerPrefs.HasKey("HighScore"))
        {
            Debug.Log("Has Key");
            highScore = PlayerPrefs.GetInt("HighScore"); //set the high score to the saved high score
        }//end if (PlayerPrefs.HasKey("HighScore"))

        PlayerPrefs.SetInt("HighScore", highScore); //set the playerPref for the high score
    }//end GetHighScore()

    //check if the player has earned a new title
    void CheckTitles()
    {
        if (wavesSurvived >= nextTitleRequirement)
        {
            Debug.Log("title upgrade earned " + titles.Count);
            if (titles.Count > (titleIndex + 1))
            {
                titleIndex++;
                currentTitle = titles[titleIndex];
                Debug.Log(currentTitle);
            }

            nextTitleRequirement += 5;
        }
    }

    //spawn a wave of items
    public void SpawnItems()
    {
        for(int i = 0; i < itemsToSpawn; i++)
        {
            int newItemIndex = Random.next(0, items.Count);
            GameObject newItem = Instantiate<GameObject>(items[newItemIndex]);
            
        }
    }

    //reduce the number of items by 1
    public void DecrimentItems()
    {
        numberOfItems--;
        score++;
    }

    //check if there are any items remaining
    public void CheckItems()
    {
        Debug.Log("CheckItems called");
        if (numberOfItems > 0)
        {
            GameOver();

        }
        else
        {
            wavesSurvived++;
            CheckTitles();
        }
    }

    public string GetTitle()
    {
        return currentTitle;
    }
}
