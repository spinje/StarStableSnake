using UnityEngine;
using UnityEngine.UI;

namespace StarStableSnake {
	public class StartScreenController : MonoBehaviour 
	{
		public GameObject background;

		void Awake()
		{
			Time.timeScale = 0f;
			AudioListener.volume = 0f;
			background.SetActive (true);
		}

		public void StartGame()
		{		
			background.SetActive (false);
	        AudioListener.volume = 1f;
	        Time.timeScale = 1f;
			GameManager.instance.gameStarted = true;
		}

		public void RestartGame() {
			GameManager.instance.Restart ();
		}
			
	}
}
