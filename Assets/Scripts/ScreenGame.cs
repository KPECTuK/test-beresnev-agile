using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenGame : MonoBehaviour, IScreen
{
	// можно использовать рутины

	private ControllerScreens _controllerScreens;
	private RectTransform _rectTransform;
	private CanvasGroup _canvasGroup;
	private Text _text;

	private readonly Queue<IEnumerator> _process = new Queue<IEnumerator>();

	public Transform Transform => transform;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_canvasGroup = GetComponent<CanvasGroup>();
		_text = GetComponent<Text>();

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

	public void OnActivate() { }

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

		_process.Enqueue(WaitConsent());
		_process.Enqueue(TimeSync());
		_process.Enqueue(WaitSimComplete());
		_process.Enqueue(Restart());
	}

	private IEnumerator WaitConsent()
	{
		_text.text = "waiting for remote";

		while(!Composition.NetState.IsLoggedIn)
		{
			yield return new WaitForEndOfFrame();
		}

		Debug.Log(">> screen: waiting opponent");
		Composition.NetState.MatchMakingStart();

		while(!Composition.NetState.IsConsent)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator TimeSync()
	{
		Debug.Log(">> routine: time sync");

		for(var index = 0; index < Composition.NetState.TimeProbeSize; index++)
		{
			Composition.NetState.Send(index);

			yield return new WaitForEndOfFrame();
		}

		while(!Composition.NetState.IsTimeSync) // or timeout
		{
			yield return new WaitForEndOfFrame();
		}

		var lag = Composition.NetState.CalculateLag();
		Debug.Log($">> approx clock lag: {lag.TotalMilliseconds} ms");

		var delay = TimeSpan.FromSeconds(Bootstrapper.MATCH_START_DELAY_SEC);
		while(delay.TotalSeconds > 0d)
		{
			delay -= TimeSpan.FromSeconds(Time.deltaTime);
			_text.text = delay.TotalSeconds > 1
				? $"{Mathf.FloorToInt((float)delay.TotalSeconds - 1)}"
				: string.Empty;

			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator WaitSimComplete()
	{
		Debug.Log(">> routine: sim start");

		// ReSharper disable once UseObjectOrCollectionInitializer
		Composition.ControllerSim = new ControllerSim();
		Composition.ControllerSim.BallColor = Composition.BallColorConsent();
		Composition.ControllerSim.BallRadius = Composition.BallRadiusConsent();

		while(Composition.ControllerSim.GetFirstCompleteFinisher() == null)
		{
			Composition.ControllerSim.Update(Composition.ControllerInput);
			Composition.NetState.Send(
				Composition.ControllerSim.Frame,
				Composition.ControllerInput.HasChange);

			yield return new WaitForEndOfFrame();
		}

		Debug.Log($">> sim complete, win: {Composition.ControllerSim.GetFirstCompleteFinisher().GetType().Name}");
	}

	private IEnumerator Restart()
	{
		yield return new WaitForEndOfFrame();

		Composition.ControllerScreens.Show<ScreenMeta>();
	}

	private void Update()
	{
		if(_process.Count > 0)
		{
			if(!_process.Peek().MoveNext())
			{
				_process.Dequeue();
			}
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
