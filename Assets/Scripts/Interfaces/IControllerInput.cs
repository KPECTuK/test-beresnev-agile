public interface IControllerInput
{
	bool HasChange { get; }
	bool IsMovingLeft { get; }
	bool IsMovingRight { get; }
	void Update();
}
