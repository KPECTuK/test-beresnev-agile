using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenConnection : MonoBehaviour, IScreen
{
	public Button[] Buttons;
	public Image[] ButtonBack;
	public Text[] Lables;
	public Image[] Samples;

	public UnityAction[] _callbacks = new UnityAction[5];

	private ControllerScreens _controllerScreens;
	private RectTransform _rectTransform;
	private CanvasGroup _canvasGroup;
	private Color _backDefault;

	private bool _isControlActive;

	public Transform Transform => transform;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_canvasGroup = GetComponent<CanvasGroup>();

		_canvasGroup.alpha = 0;
		if(Buttons.Length != 5 || ButtonBack.Length != 5 || Lables.Length != 5 || Samples.Length != 5)
		{
			Debug.LogWarning("setup error");
		}
		else
		{
			for(var index = 0; index < 5; index++)
			{
				var indexButton = index;
				_callbacks[index] = () =>
				{
					var list = Interlocked.CompareExchange(ref Composition.Matches, null, null);
					if(!ReferenceEquals(null, list))
					{
						var desc = list[indexButton];

						//Composition.JoinMatch(desc);
					}
				};
			}
		}

		_backDefault = ButtonBack[0].color;

		OnDisableControls();
	}

	private void Update()
	{
		if(Application.isPlaying && _isControlActive)
		{
			var descs = Interlocked.CompareExchange(ref Composition.Matches, null, null);
			if(!ReferenceEquals(null, descs))
			{
				for(var index = 0; index < descs.Length; index++)
				{
					if(descs[index] == null)
					{
						continue;
					}

					Lables[index].text = descs[index].NameUser;
					Samples[index].color = descs[index].BallColor;
					//ButtonBack[index].color = descs[index].IsRequest ? Color.green : _backDefault;
				}
			}
		}
	}

	public void Init(ControllerScreens machine)
	{
		_controllerScreens = machine;
	}

	public UnityAction GetShowAction()
	{
		//return () => { _controllerScreens.Show<ScreenConnection>(); };
		return () => { Composition.ControllerScreens.Show<ScreenGame>(); };
	}

	public void OnActivate() { }

	public void OnDeactivate() { }

	public void OnDisableControls()
	{
		_isControlActive = false;

		_canvasGroup.blocksRaycasts = false;
		_canvasGroup.interactable = false;

		for(var index = 0; index < 5; index++)
		{
			Buttons[index].onClick.RemoveListener(_callbacks[index]);
		}
	}

	public void OnEnableControls()
	{
		_isControlActive = true;

		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.interactable = true;

		for(var index = 0; index < 5; index++)
		{
			Buttons[index].onClick.AddListener(_callbacks[index]);
		}
	}

	public IEnumerator GetFadeIn()
	{
		while(_canvasGroup.alpha < 1f)
		{
			_canvasGroup.alpha += Time.deltaTime * 3f;
			yield return new WaitForEndOfFrame();
		}
		_canvasGroup.alpha = 1f;
	}

	public IEnumerator GetFadeOut()
	{
		while(_canvasGroup.alpha > 0f)
		{
			_canvasGroup.alpha -= Time.deltaTime * 3f;
			yield return new WaitForEndOfFrame();
		}
		_canvasGroup.alpha = 0f;
	}
}
