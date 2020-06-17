using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ControllerScreens : MonoBehaviour
{
	private ControllerScreensMenu _controllerMenu;
	private IScreen[] _screens;
	private int _indexCurrent = -1;
	private readonly Queue<IEnumerator> _commands = new Queue<IEnumerator>();
	private readonly List<ControllerScreens> _screenGroup = new List<ControllerScreens>();

	public void Awake()
	{
		if(Application.isPlaying)
		{
			_screens = transform
				.Cast<Transform>()
				.Select(_ => _.GetComponent<IScreen>())
				.Where(_ => !ReferenceEquals(_, null))
				.ToArray();

			for(var index = 0; index < _screens.Length; index++)
			{
				var screen = _screens[index];
				screen.Init(this);
			}

			_controllerMenu = transform
				.Cast<Transform>()
				.Select(_ => _.GetComponent<ControllerScreensMenu>())
				.FirstOrDefault(_ => !ReferenceEquals(_, null));

			if(!ReferenceEquals(null, _controllerMenu))
			{
				_controllerMenu.Initialize();

				for(var index = 0; index < _screens.Length; index++)
				{
					_controllerMenu.TryBindButton(_screens[index]);
				}
			}

			var current = transform.parent;
			while(!ReferenceEquals(null, current))
			{
				var controller = current.GetComponent<ControllerScreens>();
				if(!ReferenceEquals(null, controller))
				{
					controller._screenGroup.Add(this);
					break;
				}
				current = current.transform.parent;
			}

			if(ReferenceEquals(null, current))
			{
				Composition.ControllerScreens = this;
			}
		}
	}

	public void Show<TScreen>() where TScreen : IScreen
	{
		_screenGroup.ForEach(_ => _commands.Enqueue(_.CommandHide()));
		_commands.Enqueue(CommandHide());
		_commands.Enqueue(CommandShow<TScreen>());
	}

	private IEnumerator CommandHide()
	{
		if(_indexCurrent < 0 || _indexCurrent > _screens.Length)
		{
			yield break;
		}

		var screen = _screens.GetBy(_indexCurrent);

		if(ReferenceEquals(null, screen))
		{
			yield break;
		}

		screen.OnDisableControls();

		var transition = screen.GetFadeOut();
		while(transition?.MoveNext() ?? false)
		{
			yield return new WaitForEndOfFrame();
		}

		screen.OnDeactivate();
	}

	private IEnumerator CommandShow<TScreen>() where TScreen : IScreen
	{
		var index = _screens.IndexOf<TScreen>();
		_indexCurrent = index < 0 ? _indexCurrent : index;

		_screens[_indexCurrent].OnActivate();

		var transition = _screens[_indexCurrent].GetFadeIn();
		while(transition.MoveNext())
		{
			yield return new WaitForEndOfFrame();
		}

		_screens[_indexCurrent].OnEnableControls();
	}

	private void Update()
	{
		if(_commands.Count > 0)
		{
			if(!_commands.Peek().MoveNext())
			{
				_commands.Dequeue();
			}
		}
	}
}
