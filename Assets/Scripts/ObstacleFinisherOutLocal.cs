using UnityEngine;

public class ObstacleFinisherOutLocal : IObstacleFinisher
{
	public bool Bounce(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		var increment = comeFrom.Speed * comeFrom.DeltaTime;
		var next = comeFrom.Position + increment;
		if(next.y < -.5f)
		{
			resultBounce.Position = next;
			resultBounce.Speed = comeFrom.Speed;
			resultBounce.DeltaTime = 0f;

			Debug.Log($"finishing {GetType().Name}");

			return true;
		}

		return false;
	}

	public bool Cross(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		return Bounce(comeFrom, ref resultBounce);
	}
}
