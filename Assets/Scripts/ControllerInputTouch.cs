using UnityEngine;

public class ControllerInputTouch : IControllerInput
{
	private bool _prevKeyStateLeft;
	private bool _prevKeyStateRight;

	public bool HasChange { get; private set; }
	public bool IsMovingLeft { get; private set; }
	public bool IsMovingRight { get; private set; }

	public void Update()
	{
		IsMovingLeft = (Input.touchCount > 0 ? Input.touches[0].position.x : Screen.width) < Screen.width / 2;

		var changedLeft = _prevKeyStateLeft != IsMovingLeft;
		_prevKeyStateLeft = IsMovingLeft;

		IsMovingRight = (Input.touchCount > 0 ? Input.touches[0].position.x : 0) > Screen.width / 2;
		var changedRight = _prevKeyStateRight != IsMovingRight;
		_prevKeyStateRight = IsMovingRight;

		HasChange = changedLeft || changedRight;
	}
}
