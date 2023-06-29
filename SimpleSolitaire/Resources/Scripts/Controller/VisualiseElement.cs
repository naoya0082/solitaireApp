using UnityEngine;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
	public class VisualiseElement : MonoBehaviour
	{
		public Image VisualImage;
		public Image CheckMark;
		public Animator Anim;
		public Button Btn;
		
		[HideInInspector] public string ElementName;

		public void ActivateCheckmark()
		{
			CheckMark.enabled = true;
		}

		public void DeactivateCheckmark()
		{
			CheckMark.enabled = false;
		}
	}
}