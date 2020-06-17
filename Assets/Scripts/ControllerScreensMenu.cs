using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControllerScreensMenu : MonoBehaviour
{
	public Button[] Buttons;

	private UnityAction[] _actions;
	private string[] _names;

	public void Initialize()
	{
		if(Application.isPlaying)
		{
			_names = Buttons
				.Select(_ => string
					.Join(
						"_",
						_.transform
							.name
							.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
							.Skip(1)))
				.ToArray();
			_actions = new UnityAction[Buttons.Length];
		}
	}

	public void TryBindButton(IScreen screen)
	{
		// это конечно все можно по разному делать
		// я здесь стараюсь, уменьшить количество классов чтобы было проще прочесть
		// и количество компонент, чтобы сделать проще риг
		// тут много упрощений потому, что это тестовое задание и
		// конкретно ui - это, может быть, не очень сложная задача,
		// но всегда требующая существенного относительно других задач времени
		// ну и потом настройка происходит всего 1 раз на запуске,
		// с "замыканием" корневого объекта дерева, что по моему допустимо

		var index = Array.FindIndex(_names, _ => screen.Transform.name.Contains(_));
		if(index != -1)
		{
			_actions[index] = screen.GetShowAction();
			Buttons[index].onClick.AddListener(_actions[index]);
		}
		else
		{
			Debug.Log($"not found for: {screen.Transform.name}");
		}
	}

	private void OnDestroy()
	{
		if(Application.isPlaying)
		{
			for(var index = 0; index < Buttons.Length; index++)
			{
				if(!ReferenceEquals(null, _actions[index]))
				{
					Buttons[index].onClick.RemoveListener(_actions[index]);
				}
			}
		}
	}
}
