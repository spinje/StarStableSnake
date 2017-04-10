using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

namespace StarStableSnake
{
	public class Player : MonoBehaviour
	{
		//Public variables
		public Text starCoinText;					//UI Text to display current player food total.
		public Text goodJobText;					//UI Text to display after a jump.
		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
		public AudioClip jumpSound1;				//1 of 2 Audio clips to play when player collects a soda object.
		public AudioClip jumpSound2;				//2 of 2 Audio clips to play when player collects a soda object.
		public AudioClip gameOverSound;				//Audio clip to play when player dies.
		public float playerSpeed = 0.01f;
		public GameObject particlePrefab;

		//Private variables
		private Animator childAnimator;					//Used to store a reference to the Player's animator component.
		private Animator animator;						//Used to store a reference to the Player's animator component.
		private int starCoins;                          //Used to store player food points total during level.
		private Vector3 currentDirection;
		private Vector3 nextDirection;
		private Vector3 nextMoveGoal;
		private Transform playerTransform;
		private bool shouldGrow = false;

		void Start ()
		{
			//Get a component reference to the Player's animator component
			var animators = GetComponentsInChildren<Animator>();
			animator = animators [0];
			childAnimator = animators [1];

			starCoins = GameManager.instance.playerStarCoins;
			starCoinText.text = "Star Coins: " + starCoins;

			playerTransform = transform;

			currentDirection = new Vector3(1,0,0); // Start by moving the player right
			nextDirection = new Vector3(1,0,0); // Start by moving the player right
			nextMoveGoal = playerTransform.position + currentDirection;

		}

		private void Update ()
		{
			if (!GameManager.instance.gameStarted)
				return;
			
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.

			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			//Check if moving horizontally, if so set vertical to zero.
			if(horizontal != 0)
			{
				vertical = 0;
			}

			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				QueueDirectionChange (horizontal, vertical);
			}

			//The player should always be moving
			MovePlayer (); 
		}

		private void QueueDirectionChange (int xDir, int yDir) {
			// The player can't turn around since this would always result in an instant death
			if (nextDirection.x == Math.Abs (xDir) || currentDirection.x == Math.Abs (xDir))
				return;
			else if (nextDirection.y == Math.Abs (yDir) || currentDirection.y == Math.Abs (yDir))
				return;

			nextDirection = new Vector3 (xDir, yDir, 0);
		}

		private void ChangeDirection () {
			// Make sure we are in correct alignment
			// Even the slightest floatingnumber can grow with time
			playerTransform.position = nextMoveGoal;

			GameManager.instance.CreateTrail (shouldGrow);
			shouldGrow = false;

			if (currentDirection == nextDirection) {
				nextMoveGoal += currentDirection;
			}
			else {
				currentDirection = nextDirection;
				nextMoveGoal = transform.position + currentDirection;
				SetAnimatorTrigger (nextDirection.x, nextDirection.y);
			}
			
		}

		private void MovePlayer () {
			if (!animator.GetCurrentAnimatorStateInfo (0).IsName ("HorseJumpHorizontal"))
				playerTransform.rotation = Quaternion.identity;

			if(currentDirection.x > 0 && (playerTransform.position.x > nextMoveGoal.x)) {
				ChangeDirection ();
			}
			else if(currentDirection.x < 0 && (playerTransform.position.x < nextMoveGoal.x)) {
				ChangeDirection ();
			}
			else if(currentDirection.y > 0 && (playerTransform.position.y > nextMoveGoal.y)) {
				ChangeDirection ();
			}
			else if(currentDirection.y < 0 && (playerTransform.position.y < nextMoveGoal.y)) {
				ChangeDirection ();
			}

			//Always move the player forward
			playerTransform.position += currentDirection * playerSpeed * Time.fixedDeltaTime;
		}
			
		private void SetAnimatorTrigger (float xDir, float yDir) {

			//Make the player face the right direction and start playing runanimation
			if(yDir > 0)
				childAnimator.SetTrigger ("playerUp");
			else if(yDir < 0)
				childAnimator.SetTrigger ("playerDown");
			else if(xDir > 0)
				childAnimator.SetTrigger ("playerRight");
			else if(xDir < 0)
				childAnimator.SetTrigger ("playerLeft");
		}

		private void OnTriggerEnter2D (Collider2D other)
		{
			
			//Check if the tag of the trigger collided with is Food.
			if (other.tag == "Fruit") {

				//Grow trails when eating fruit
				shouldGrow = true;

				//Increase score
				starCoins += other.GetComponent<Item> ().rewardValue;;
				starCoinText.text =  "Star Coins: " + starCoins;

				goodJobText.enabled = true;
				goodJobText.text = other.GetComponent<Item> ().rewardText;
				goodJobText.GetComponent<Animation>().Play ();

				SpawnParticles ();

				//Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
				SoundManager.instance.RandomizeSoundEffects (eatSound1, eatSound2);
				
				//Disable the food object the player collided with.
				GameManager.instance.DestroyItem(other.gameObject);
			}

			// Do not check for collisions while jumping since collider is tilted
			if(animator.GetCurrentAnimatorStateInfo(0).IsName("HorseJumpHorizontal"))
				return;
			

			if (other.tag == "Jump") {
				//Increase score
				starCoins += other.GetComponent<Item> ().rewardValue;;
				starCoinText.text =  "Star Coins: " + starCoins;

				SoundManager.instance.RandomizeSoundEffects (jumpSound1, jumpSound2);

				goodJobText.enabled = true;
				goodJobText.text = other.GetComponent<Item> ().rewardText;
				goodJobText.GetComponent<Animation>().Play ();

				//Play jumpanimation
				animator.SetTrigger ("jumpLeft");

				//Make spriteanimation slower while jumping
				//Just approximating the time
				childAnimator.speed = 0.3f;
				Invoke("ResetAnimatorSpeed", 0.9f);

				GameManager.instance.DestroyItem(other.gameObject);
			}
			else if (other.tag == "OuterBounds" || other.tag == "Trail") {
				GameOver ();
			}
		}

		private void ResetAnimatorSpeed() {
			childAnimator.speed = 1f;
		}

		private void SpawnParticles () {
			GameObject particles = (GameObject)Instantiate(particlePrefab);

			particles.transform.position = playerTransform.position;

			particles.SetActive(true);
			for(int i = 0; i < particles.transform.childCount; i++)
				particles.transform.GetChild(i).gameObject.SetActive(true);

		}

		private void GameOver ()
		{
			SoundManager.instance.PlaySingle (gameOverSound);
			SoundManager.instance.musicSource.Stop();

			GameManager.instance.playerStarCoins = starCoins;
			
			//Call the GameOver function in GameManager
			GameManager.instance.GameOver ();
		}
	}
}

