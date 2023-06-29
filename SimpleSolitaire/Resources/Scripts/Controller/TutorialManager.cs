using System.Collections;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
	public class TutorialManager : MonoBehaviour
	{
		public GameManager GameManagerComponent;

		private const string FIRST_PLAY = "FirstPlay";

		/// <summary>
		/// Activate tutorial if player game first one.
		/// </summary>
		private IEnumerator Start()
		{
			yield return new WaitForSeconds(0.1f);

			if (!PlayerPrefs.HasKey(FIRST_PLAY))
			{
				GameManagerComponent.ShowTutorialLayer();
			}
		}

		/// <summary>
		/// Close game tutorial window <see cref="Tutorial"/>.
		/// </summary>
		public void CloseTutorial()
		{
			PlayerPrefs.SetInt(FIRST_PLAY, 1);
			GameManagerComponent.HideTutorialLayer();
		}

		/// <summary>
		/// Is first play or not.
		/// </summary>
		public bool IsHasKey()
		{
			return PlayerPrefs.HasKey(FIRST_PLAY);
		}
	}
}