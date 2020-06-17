using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenScores : MonoBehaviour, IScreen
{
	public Text[] Labels;

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
		return () => { _controllerScreens.Show<ScreenScores>(); };
	}

	public void OnActivate()
	{
		Array.Sort(Composition.DataMeta.Scores, new ComparerDataScore());
		var indexLabel = 0;
		for(var index = 0; indexLabel < Labels.Length && index < Composition.DataMeta.Scores.Length; index++)
		{
			if(string.IsNullOrWhiteSpace(Composition.DataMeta.Scores[index].IdUser))
			{
				continue;
			}

			Labels[index].text = $"{Composition.DataMeta.Scores[index].Name} : {Composition.DataMeta.Scores[index].Value:0000}";
			indexLabel++;
		}
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
