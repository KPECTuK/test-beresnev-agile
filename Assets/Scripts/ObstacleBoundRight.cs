using UnityEngine;

public class ObstacleBoundRight : IObstacle
{
	public bool Bounce(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		return Cross(comeFrom, ref resultBounce);
	}

	public bool Cross(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		var increment = comeFrom.Speed * comeFrom.DeltaTime;
		var next = comeFrom.Position + increment;
		var bounce = .5f * Composition.DataMeta.CourtRatio - Composition.ControllerSim.BallRadius;
		// positives
		var distanceNoBounce = next.x - comeFrom.Position.x;
		var distanceBounce = bounce - comeFrom.Position.x;
		distanceNoBounce = distanceNoBounce > 0f ? distanceNoBounce : 0f;
		distanceBounce = distanceBounce > 0f ? distanceBounce : 0f;
		if(distanceBounce < distanceNoBounce)
		{
			// no zero here, cause: zero is vertical movement (both are positives and is upper)
			var factor = distanceBounce / distanceNoBounce;
			resultBounce.Position = new Vector2(
				comeFrom.Position.x + distanceBounce,
				comeFrom.Position.y + increment.y * factor);
			resultBounce.Speed = new Vector2(
				-comeFrom.Speed.x,
				comeFrom.Speed.y);
			resultBounce.DeltaTime = comeFrom.DeltaTime / (1f - factor);

			Debug.Log($"bouncing {GetType().Name}");

			return true;
		}

		return false;
	}
}
