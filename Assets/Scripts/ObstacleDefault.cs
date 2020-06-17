using UnityEngine;

public class ObstacleDefault : IObstacle
{
	public bool Bounce(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		var increment = comeFrom.Speed * comeFrom.DeltaTime;
		var next = comeFrom.Position + increment;
		resultBounce.Position = next;
		resultBounce.Speed = comeFrom.Speed;
		resultBounce.DeltaTime = 0f;

		Debug.Log($"bouncing {GetType().Name}");

		return true;
	}

	public bool Cross(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		var increment = comeFrom.Speed * comeFrom.DeltaTime;
		var next = comeFrom.Position + increment;
		resultBounce.Position = next;
		resultBounce.Speed = comeFrom.Speed;
		resultBounce.DeltaTime = 0f;

		Debug.Log($"bouncing {GetType().Name}");

		return true;
	}
}
