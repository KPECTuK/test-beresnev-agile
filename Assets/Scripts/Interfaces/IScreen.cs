using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public interface IScreen
{
	Transform Transform { get; }
	// initialization
	void Init(ControllerScreens machine);
	UnityAction GetShowAction();
	// activation sequence
	void OnActivate();
	IEnumerator GetFadeIn();
	void OnEnableControls();
	// deactivation sequence
	void OnDisableControls();
	IEnumerator GetFadeOut();
	void OnDeactivate();
}
