using UnityEngine;

public class ObstaclePaddleLocal : IObstacle
{
	public bool Bounce(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		var increment = comeFrom.Speed * comeFrom.DeltaTime;
		var next = comeFrom.Position + increment;
		var paddlePos = Composition.ControllerSim.GetLocalPaddleReflectorPos();
		var bounce = paddlePos.y + Composition.ControllerSim.BallRadius;
		// positives
		var distanceNoBounce = comeFrom.Position.y - next.y;
		var distanceBounce = comeFrom.Position.y - bounce;
		distanceNoBounce = distanceNoBounce > 0f ? distanceNoBounce : 0f;
		distanceBounce = distanceBounce > 0f ? distanceBounce : 0f;
		if(distanceBounce < distanceNoBounce)
		{
			// no zero here, cause: zero is vertical movement (both are positives and is upper)
			var factor = distanceBounce / distanceNoBounce;
			var bouncePos = new Vector2(
				comeFrom.Position.x + increment.x * factor,
				comeFrom.Position.y + distanceBounce);
			var paddleBoundDistance = bouncePos.x > paddlePos.x
				? bouncePos.x - paddlePos.x
				: paddlePos.x - bouncePos.x;


			if(paddleBoundDistance < Composition.ControllerSim.Frame.PaddleLocalReflectorHalfSize)
			{
				resultBounce.Position = bouncePos;
				resultBounce.Speed = new Vector2(
					comeFrom.Speed.x,
					-comeFrom.Speed.y);
				resultBounce.DeltaTime = comeFrom.DeltaTime / (1f  - factor);

				//var screenBouncePos = ((Vector3)bouncePos + Vector3.down * Composition.ControllerSim.BallRadius) * Composition.ModelScaleFactor;
				//var screenPaddlePos = (Vector3)paddlePos * Composition.ModelScaleFactor;
				//Debug.DrawLine(screenPaddlePos, screenBouncePos, Color.magenta, 20f);

				Debug.Log($"bouncing {GetType().Name}");

				return true;
			}
		}

		return false;
	}

	public bool Cross(DataBallState comeFrom, ref DataBallState resultBounce)
	{
		var increment = comeFrom.Speed * comeFrom.DeltaTime;
		var next = comeFrom.Position + increment;
		var paddlePos = Composition.ControllerSim.GetLocalPaddleReflectorPos();
		var bounce = paddlePos.y + Composition.ControllerSim.BallRadius;
		// positives
		var distanceNoBounce = comeFrom.Position.y - next.y;
		var distanceBounce = comeFrom.Position.y - bounce;
		distanceNoBounce = distanceNoBounce > 0f ? distanceNoBounce : 0f;
		distanceBounce = distanceBounce > 0f ? distanceBounce : 0f;
		if(distanceBounce < distanceNoBounce)
		{
			// no zero here, cause: zero is vertical movement (both are positives and is upper)
			var factor = distanceBounce / distanceNoBounce;
			var bouncePos = new Vector2(
				comeFrom.Position.x + increment.x * factor,
				comeFrom.Position.y + distanceBounce);
			resultBounce.Position = bouncePos;
			resultBounce.Speed = new Vector2(
				comeFrom.Speed.x,
				-comeFrom.Speed.y);
			resultBounce.DeltaTime = comeFrom.DeltaTime / (1f - factor);

			Debug.Log($"bouncing {GetType().Name}");

			return true;
		}

		return false;
	}
}
