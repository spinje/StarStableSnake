using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;

namespace StarStableSnake
	
{
	
	public class BoardManager : MonoBehaviour
	{
		[Serializable]
		public class Count
		{
			public int minimum;
			public int maximum;

			//Constructor
			public Count (int min, int max)
			{
				minimum = min;
				maximum = max;
			}
		}
		
		
		public int columns = 8; 										//Number of columns in our game board
		public int rows = 8;											//Number of rows in our game board
		public Count jumpCount = new Count (1, 2);						//Lower and upper limit for our random number of jumps per level
		public Count fruitCount = new Count (1, 5);						//Lower and upper limit for our random number of fruit items per level
		public GameObject[] floorTiles;									//Array of floor prefabs
		public GameObject[] jumpItem;									//Array of wall prefabs
		public GameObject[] fruitItem;									//Array of food prefabs
		public GameObject[] outerWallTiles;								//Array of outer tile prefabs
		public GameObject[] trailItem;									//Array of trail prefabs
		
		private Transform boardHolder;									//A variable to store a reference to the transform of our Board tiles
		private Transform itemsHolder;									//A variable to store a reference to the transform of our items
		private Transform trailsHolder;									//A variable to store a reference to the transform of our trails
		private List <Vector3> gridPositions = new List <Vector3> ();	//A list of possible locations to place tiles
		private Queue <GameObject> trails = new Queue <GameObject> ();  //A queue of all active trails
		private List <GameObject> activeFruits = new List <GameObject> ();
		private List <GameObject> activeJumps = new List <GameObject> ();


		//Clears our lists and prepares it to generate a new board
		void InitialiseList ()
		{
			//Clear our lists 
			gridPositions.Clear ();
			trails.Clear ();
			activeFruits.Clear ();
			activeJumps.Clear ();
			
			//Loop through x axis (columns)
			for(int x = 1; x < columns-1; x++)
			{
				//Within each column, loop through y axis (rows)
				for(int y = 1; y < rows-1; y++)
				{
					//At each index add a new Vector3 to our list with the x and y coordinates of that position
					gridPositions.Add (new Vector3(x, y, 0f));
				}
			}
		}
		
		
		//Sets up the outer walls and floor (background) of the game board.
		void BoardSetup ()
		{
			//Instantiate Board and set boardHolder to its transform.
			boardHolder = new GameObject ("Board").transform;
			itemsHolder = new GameObject ("Items").transform;
			trailsHolder = new GameObject ("Trails").transform;
			
			//Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
			for(int x = -1; x < columns + 1; x++)
			{
				//Loop along y axis, starting from -1 to place floor or outerwall tiles.
				for(int y = -1; y < rows + 1; y++)
				{
					//Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
					GameObject toInstantiate = floorTiles[Random.Range (0,floorTiles.Length)];

					GameObject instance;

					//Check if we current position is at board edge
					//Select an index from left,right,down,up, corner (top right default)
					if (x == -1 || x == columns || y == -1 || y == rows) {

						bool flipX = false;
						bool flipY = false;

						//Top left corner
						if (x == -1 && y == rows) {
							toInstantiate = outerWallTiles [4];
							flipX = true;
						} 
						//Top right corner
						else if (x == columns && y == rows) {
							toInstantiate = outerWallTiles [4];
			
						}
						//Bottom left corner
						else if (x == -1 && y == -1) {
							toInstantiate = outerWallTiles [4];
							flipX = true;
							flipY = true;
						} 
						//Bottom right corner
						else if (x == columns && y == -1) {
							toInstantiate = outerWallTiles [4];
							flipY = true;
						}
						//Left
						else if (x == -1) {
							toInstantiate = outerWallTiles [0];
							flipX = true;
						}
						//Right
						else if (x == columns) {
							toInstantiate = outerWallTiles [1];
						}
						//Down
						else if (y == -1) {
							toInstantiate = outerWallTiles [2];
							flipY = true;
						}
						//Up
						else if (y == rows)
							toInstantiate = outerWallTiles [3];

						//Instantiate the GameObject instance using the prefab
						instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
						instance.GetComponent<SpriteRenderer> ().flipX = flipX;
						instance.GetComponent<SpriteRenderer> ().flipY = flipY;
						instance.name = "x:" + x + ", y:" + y;
					} else {
						instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;

						// Flip grasstiles for more variation
						if( Random.Range (0, 1) == 1)
							instance.GetComponent<SpriteRenderer>().flipX = true;

						if (Random.Range (0, 1) == 1)
							instance.GetComponent<SpriteRenderer> ().flipY = true;
					}

					//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
					instance.transform.SetParent (boardHolder);
				}
			}
		}

		
		//RandomPosition returns a random position from our list gridPositions.
		Vector3 RandomPosition ()
		{
			//Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
			int randomIndex = Random.Range (0, gridPositions.Count);
			
			//Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
			Vector3 randomPosition = gridPositions[randomIndex];
			
			//Remove the entry at randomIndex from the list so that it can't be re-used.
			gridPositions.RemoveAt (randomIndex);
			
			//Return the randomly selected Vector3 position.
			return randomPosition;
		}

		//Used to remove items of type Jump and Fruit
		public void RemoveItem (GameObject item) {
			gridPositions.Add (item.transform.position);

			// Spawn new item
			SpawnNewItem(item);

			// Destroy the old instance
			if (item.tag == "Jump") {
				StartCoroutine (DestroyItem (item, 1.4f));
			} else {
				
				Destroy (item);
			}
		}

		private IEnumerator DestroyItem(GameObject item, float delay) {
			yield return new WaitForSeconds (delay);
		
			Destroy (item);
		}

		public void SpawnNewItem (GameObject item) {

			if (item.tag == "Jump") {
				activeJumps.Remove (item);

				if(activeJumps.Count > 1)
					LayoutObjectAtRandom (jumpItem, 0, jumpCount.maximum - activeJumps.Count);
				else
					LayoutObjectAtRandom (jumpItem, 1, 2);
			} else if (item.tag == "Fruit") {
				activeFruits.Remove (item);

				if(activeFruits.Count > 1)
					LayoutObjectAtRandom (fruitItem, 0, fruitCount.maximum - activeFruits.Count);
				else
					LayoutObjectAtRandom (fruitItem, 1, 2);
			}

		}

		//Used to remove the last trail
		public void RemoveTrail() {

			if (trails.Count < 1)
				return;

			GameObject goToBeRemoved = trails.Dequeue ();

			//Add possible location
			if(gridPositions != null && goToBeRemoved != null)
				gridPositions.Add (goToBeRemoved.transform.position);

			Destroy (goToBeRemoved);
		}

		public void CreateTrail() {
			GameObject player = GameObject.FindGameObjectWithTag ("Player");

			GameObject tileChoice = trailItem[Random.Range (0, trailItem.Length)];
			GameObject instance = Instantiate(tileChoice, player.transform.position, Quaternion.identity);
			instance.transform.SetParent (trailsHolder);

			trails.Enqueue (instance);

			gridPositions.Remove (instance.transform.position);
		}
		
		//LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
		void LayoutObjectAtRandom (GameObject[] tileArray, int minimum, int maximum)
		{
			//Choose a random number of objects to instantiate within the minimum and maximum limits
			int objectCount = Random.Range (minimum, maximum+1);
			
			//Instantiate objects until the randomly chosen limit objectCount is reached
			for(int i = 0; i < objectCount; i++)
			{
				Vector3 randomPosition = RandomPosition();
				
				//Choose a random tile from tileArray and assign it to tileChoice
				GameObject tileChoice = tileArray[Random.Range (0, tileArray.Length)];
				
				//Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
				GameObject instance = Instantiate(tileChoice, randomPosition, Quaternion.identity);

				if (instance.tag == "Fruit")
					activeFruits.Add (instance);
				else if (instance.tag == "Jump")
					activeJumps.Add (instance);

				instance.transform.SetParent (itemsHolder);
			}
		}
		
		
		//SetupScene initializes our level and calls the previous functions to lay out the gameboard
		public void SetupScene ()
		{
			//Creates the outer walls and floor.
			BoardSetup ();
			
			//Reset our list of gridpositions.
			InitialiseList ();
			
			//Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (jumpItem, jumpCount.minimum, jumpCount.maximum);
			
			//Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
			LayoutObjectAtRandom (fruitItem, fruitCount.minimum, fruitCount.maximum);

		}
	}
}
