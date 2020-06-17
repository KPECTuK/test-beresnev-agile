using UnityEngine;

public class ControllerInputKeyboard : IControllerInput
{
	private bool _prevKeyStateLeft;
	private bool _prevKeyStateRight;

	public bool HasChange { get; private set; }
	public bool IsMovingLeft { get; private set; }
	public bool IsMovingRight { get; private set; }

	public void Update()
	{
		IsMovingLeft = Input.GetKey(KeyCode.LeftArrow);
		var changedLeft = _prevKeyStateLeft != IsMovingLeft;
		_prevKeyStateLeft = IsMovingLeft;

		IsMovingRight = Input.GetKey(KeyCode.RightArrow);
		var changedRight = _prevKeyStateRight != IsMovingRight;
		_prevKeyStateRight = IsMovingRight;

		HasChange = changedLeft || changedRight;
	}
}