using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenMeta : MonoBehaviour, IScreen
{
	private ControllerScreens _controllerScreens;
	private RectTransform _rectTransform;
	private CanvasGroup _canvasGroup;

	public Transform Transform => transform;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_canvasGroup = GetComponent<CanvasGroup>();

		_canvasGroup.alpha = 0;

		OnDisableControls();
	}

	public void Init(ControllerScreens machine)
	{
		_controllerScreens = machine;
	}

	public UnityAction GetShowAction()
	{
		throw new NotSupportedException();
	}

	public void OnActivate()
	{
		Composition.ControllerSim = new ControllerSimLobby();
	}

	public void OnDeactivate() { }

	public void OnDisableControls()
	{
		_canvasGroup.blocksRaycasts = false;
		_canvasGroup.interactable = false;
	}

	public void OnEnableControls()
	{
		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.interactable = true;
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
