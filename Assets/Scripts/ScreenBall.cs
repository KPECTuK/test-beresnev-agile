using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenBall : MonoBehaviour, IScreen
{
	private ControllerScreens _controllerScreens;
	private RectTransform _rectTransform;
	private CanvasGroup _canvasGroup;

	public bool IsControlActive { get; private set; }

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
		return () => { _controllerScreens.Show<ScreenBall>(); };
	}

	public void OnActivate() { }

	public void OnDeactivate() { }

	public void OnDisableControls()
	{
		IsControlActive = false;

		_canvasGroup.blocksRaycasts = false;
		_canvasGroup.interactable = false;
	}

	public void OnEnableControls()
	{
		IsControlActive = true;

		_canvasGroup.blocksRaycasts = true;
		_canvasGroup.interactable = true;
	}

	public IEnumerator GetFadeIn()
	{
		Composition.ControllerCameraRig.SetSource(Composition.ControllerCameraRig.CameraSetMain.transform);
		Composition.ControllerCameraRig.SetTarget(Composition.ControllerCameraRig.CameraSetMenu.transform);

		while(_canvasGroup.alpha < 1f)
		{
			var key = _canvasGroup.alpha;
			key += Time.deltaTime * 3f;
			_canvasGroup.alpha = key;
			Composition.ControllerCameraRig.UpdateRig(key);

			yield return new WaitForEndOfFrame();
		}

		_canvasGroup.alpha = 1f;
		Composition.ControllerCameraRig.UpdateRig(1f);
	}

	public IEnumerator GetFadeOut()
	{
		Composition.ControllerCameraRig.SetSource(Composition.ControllerCameraRig.CameraSetMain.transform);
		Composition.ControllerCameraRig.SetTarget(Composition.ControllerCameraRig.CameraSetMenu.transform);

		while(_canvasGroup.alpha > 0f)
		{
			var key = _canvasGroup.alpha;
			key -= Time.deltaTime * 3f;
			_canvasGroup.alpha = key;
			Composition.ControllerCameraRig.UpdateRig(key);

			yield return new WaitForEndOfFrame();
		}

		_canvasGroup.alpha = 0f;
		Composition.ControllerCameraRig.UpdateRig(0f);
	}
}
