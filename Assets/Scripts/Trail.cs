using UnityEngine;
using System.Collections;

namespace StarStableSnake
{
	public class Trail : MonoBehaviour
	{

		void Start () {
			Invoke ("ActivateCollider", 1.3f);
		}

		private void ActivateCollider () {
			gameObject.GetComponent<BoxCollider2D> ().enabled = true;
		}
	}
}
