using UnityEngine;
using System.Collections;

namespace StarStableSnake
{
	public class SoundManager : MonoBehaviour 
	{
		public AudioSource effectSource;
		public AudioSource musicSource;	
		public static SoundManager instance = null;	
		public float lowPitchRange = .95f;				//The lowest a sound effect will be randomly pitched
		public float highPitchRange = 1.05f;			//The highest a sound effect will be randomly pitched
		
		
		void Awake ()
		{
			//Enforce singleton pattern
			if (instance == null)
				instance = this;
			else if (instance != this)
				Destroy (gameObject);
			
			//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading scene
			DontDestroyOnLoad (gameObject);
		}
		

		public void PlaySingle(AudioClip clip)
		{
			effectSource.clip = clip;
			effectSource.Play ();
		}
		

		public void RandomizeSoundEffects (params AudioClip[] clips)
		{
			int randomIndex = Random.Range(0, clips.Length);
			float randomPitch = Random.Range(lowPitchRange, highPitchRange);

			effectSource.pitch = randomPitch;
			effectSource.clip = clips[randomIndex];

			effectSource.Play();
		}
	}
}
