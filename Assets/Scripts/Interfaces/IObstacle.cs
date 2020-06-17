public interface IObstacle
{
	bool Bounce(DataBallState comeFrom, ref DataBallState resultBounce);
	bool Cross(DataBallState comeFrom, ref DataBallState resultBounce);
}

public interface IObstacleDynamic : IObstacle { }

public interface IObstacleFinisher : IObstacle { }
