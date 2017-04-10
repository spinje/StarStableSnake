using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace StarStableSnake
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{
		public int playerStarCoins = 0;							//Starting value for Player food points.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		public bool gameStarted = false;						//Prevents the player from moving
		
		private Text EndGameText;								//Text to display current level number.
		private GameObject EndGameImage;						//Image to block out level as levels are being set up, background for levelText.
		private BoardManager boardManager;						//Store a reference to our BoardManager which will set up the level.
		private bool enemiesMoving;								//Boolean to check if enemies are moving.

		
		//Awake is always called before any Start functions
		void Awake()
		{
            //Check if instance already exists
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);	
			
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);

			//Get a component reference to the attached BoardManager script
			boardManager = GetComponent<BoardManager>();

			InitGame();
		}
			
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //Register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            instance.InitGame();
        }

		
		//Initializes the game for each level
		void InitGame()
		{
			//Get a reference to our image LevelImage by finding it by name.
			EndGameImage = GameObject.Find("EndGameImage");
			
			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
			EndGameText = GameObject.Find("EndGameText").GetComponent<Text>();

			HideLevelImage ();
			
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			boardManager.SetupScene();

		}

		public void DestroyItem (GameObject item) {
			boardManager.RemoveItem (item);
		}

		public void CreateTrail(bool growing = false) {
			if (growing == false)
				RemoveTrail ();
			
			boardManager.CreateTrail ();
		}

		public void RemoveTrail() {
			boardManager.RemoveTrail ();
		}

		void HideLevelImage()
		{
			EndGameImage.SetActive(false);
		}

		public void GameOver()
		{
			EndGameText.text = "Good job! You earned " + playerStarCoins + " Star Coins!";

			EndGameImage.SetActive(true);
			
			//Disable this GameManager.
			enabled = false;
		}

		//Restart reloads the scene when called.
		public void Restart ()
		{
			gameStarted = false;
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}

	}
}

