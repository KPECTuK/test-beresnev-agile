using System;
using UnityEngine;

public class ControllerSim : IControllerSim
{
	private DataFrameState _frame;

	private ControllerSimPhase _phase;

	public DataFrameState Frame => _frame;
	public Color BallColor { get; set; }
	public float BallRadius { get; set; }

	private readonly DataFrameState[] _framesRemote = new DataFrameState[8];
	private int _indexCurrent;
	//
	private DataBallState[] _frameBallTrail;
	private IObstacle[] _bounced;
	private int _bouncedTotal;
	private readonly IObstacle[] _bouncers =
	{
		// static
		new ObstacleBoundLeft(),
		new ObstacleBoundRight(),
		// dynamic
		new ObstaclePaddleLocal(),
		new ObstaclePaddleRemote(),
		// finishes
		new ObstacleFinisherOutLocal(),
		new ObstacleFinisherOutRemote(),
		// default, must last one
		new ObstacleDefault(),
	};

	public ControllerSim()
	{
		_frame = Composition.FrameConsent(
			Composition.DataMeta,
			Composition.NetState);
	}

	public IObstacleFinisher GetFirstCompleteFinisher()
	{
		for(var index = 0; index < _bouncedTotal; index++)
		{
			if(_bounced[index] is IObstacleFinisher finisher)
			{
				return finisher;
			}
		}

		return null;
	}

	public void PushRemote(DataFrameState frame)
	{
		lock(_framesRemote)
		{
			var indexNext = (_indexCurrent + 1) % _framesRemote.Length;
			_framesRemote[indexNext] = frame;
			_indexCurrent = indexNext;
		}
	}

	public void Update(IControllerInput controllerInput)
	{
		DataFrameState lastRemote;
		lock(_framesRemote)
		{
			lastRemote = _framesRemote[_indexCurrent];
		}

		var paddleLocalMovement = controllerInput.IsMovingLeft ? -1f : 0f;
		paddleLocalMovement += controllerInput.IsMovingRight ? 1f : 0f;

		var leftBound = -.5f * Composition.DataMeta.CourtRatio;
		var rightBound = .5f * Composition.DataMeta.CourtRatio;

		var lastLocalDelta = _frame.Time;
		Composition.NetState.SetTimeStamp(ref _frame);
		var bufferedDeltaTime = (float)(DateTime.FromBinary(_frame.Time) - DateTime.FromBinary(lastRemote.Time)).TotalSeconds;
		var simDeltaTime = (float)(DateTime.FromBinary(_frame.Time) - DateTime.FromBinary(lastLocalDelta)).TotalSeconds;

		// update paddle local
		_frame.PaddleLocalSpeed = Composition.DataMeta.PaddleSpeed * paddleLocalMovement;
		_frame.PaddleLocalPosition += Frame.PaddleLocalSpeed * simDeltaTime;
		var leftPaddleLocalBound = _frame.PaddleLocalPosition - _frame.PaddleLocalReflectorHalfSize;
		var rightPaddleLocalBound = _frame.PaddleLocalPosition + _frame.PaddleLocalReflectorHalfSize;
		// limit
		_frame.PaddleLocalPosition = leftBound > leftPaddleLocalBound
			? leftBound + _frame.PaddleLocalReflectorHalfSize
			: Frame.PaddleLocalPosition;
		_frame.PaddleLocalPosition = rightBound < rightPaddleLocalBound
			? rightBound - _frame.PaddleLocalReflectorHalfSize
			: Frame.PaddleLocalPosition;

		// update paddle remote
		_frame.PaddleRemoteSpeed = lastRemote.PaddleRemoteSpeed;
		_frame.PaddleRemotePosition = lastRemote.PaddleRemotePosition;
		_frame.PaddleRemotePosition += lastRemote.PaddleRemoteSpeed * bufferedDeltaTime;
		var leftPaddleRemoteBound = _frame.PaddleRemotePosition - _frame.PaddleRemoteReflectorHalfSize;
		var rightPaddleRemoteBound = _frame.PaddleRemotePosition + _frame.PaddleRemoteReflectorHalfSize;
		// limit
		_frame.PaddleRemotePosition = leftBound > leftPaddleRemoteBound
			? leftBound + _frame.PaddleRemoteReflectorHalfSize
			: Frame.PaddleRemotePosition;
		_frame.PaddleRemotePosition = rightBound < rightPaddleRemoteBound
			? rightBound - _frame.PaddleRemoteReflectorHalfSize
			: Frame.PaddleRemotePosition;

		// update ball
		var ballSpeed = new Vector2(Frame.BallSpeedX, Frame.BallSpeedY);
		var ballPosition = new Vector2(Frame.BallPositionX, Frame.BallPositionY);

		// debug
		if(simDeltaTime < bufferedDeltaTime)
		{
			((Vector3)ballPosition * Composition.ModelScaleFactor).DebugDrawPoint(Color.yellow, .01f, 20f);
		}

		// - init bounce calc buffers
		_bouncedTotal = 0;
		_bounced = _bounced ?? new IObstacle[_bouncers.Length];
		_frameBallTrail = _frameBallTrail ?? new DataBallState[_bouncers.Length + 1];
		_frameBallTrail[_bouncedTotal] = new DataBallState
		{
			Position = ballPosition,
			Speed = ballSpeed,
			DeltaTime = simDeltaTime,
		};

		// - calc bounce
		for(var index = 0; index < _bouncers.Length; index++)
		{
			// exclude obstacle been bounced
			var indexBounced = 0;
			for(; indexBounced < _bouncedTotal; indexBounced++)
			{
				if(ReferenceEquals(_bouncers[index], _bounced[indexBounced]))
				{
					break;
				}
			}
			if(indexBounced != _bouncedTotal)
			{
				continue;
			}

			// check bounce (default should the last one)
			if(_bouncers[index].Bounce(_frameBallTrail[_bouncedTotal], ref _frameBallTrail[_bouncedTotal + 1]))
			{
				_bounced[_bouncedTotal] = _bouncers[index];
				index = 0;
				_bouncedTotal++;
			}
		}

		// - calc out, with default move
		//for(var index = 0; index < _finishers.Length; index++)
		//{
		//	if(_finishers[index].Bounce(_frameBallTrail[indexTrail], ref _frameBallTrail[indexTrail + 1]))
		//	{
		//		indexTrail++;
		//	}
		//}

		_frame.BallPositionX = _frameBallTrail[_bouncedTotal].Position.x;
		_frame.BallPositionY = _frameBallTrail[_bouncedTotal].Position.y;
		_frame.BallSpeedX = _frameBallTrail[_bouncedTotal].Speed.x;
		_frame.BallSpeedY = _frameBallTrail[_bouncedTotal].Speed.y;

		//_phase =
		//	_phase ??
		//	new ControllerSimPhase(
		//		_frame,
		//		_frameBallTrail[_bouncedTotal],
		//		_bouncers);
		//if(!_phase.ApplyCorrection(ref _frame))
		//{
		//	_phase = new ControllerSimPhase(
		//		_frame,
		//		_frameBallTrail[_bouncedTotal],
		//		_bouncers);
		//}
	}
}

public class ControllerSimPhase
{
	private DateTime _timeTarget;
	private Vector2 _targetBallPos;
	private Vector2 _targetBallSpeed;

	public ControllerSimPhase(DataFrameState stateFrame, DataBallState stateBall, IObstacle[] obstacles)
	{
		Debug.Log("predicting");

		var maxPath = Mathf.Sqrt(1f + Composition.DataMeta.CourtRatio) * 2f;
		var currentSpeed = stateBall.Speed.magnitude;
		stateBall.DeltaTime = maxPath / currentSpeed;
		var bounce = stateBall;
		if(FindBounce(ref bounce, obstacles))
		{
			_timeTarget = DateTime.FromBinary(stateFrame.Time) + TimeSpan.FromSeconds(bounce.DeltaTime);
			_targetBallPos = bounce.Position;
			_targetBallSpeed = bounce.Speed;

			Debug.DrawLine(_targetBallPos, stateBall.Position, Color.red, 20f);

		}
		else
		{
			throw new Exception("undef");
		}
	}

	private bool FindBounce(ref DataBallState bounce, IObstacle[] obstacles)
	{
		var index = 0;
		for(; index < obstacles.Length && !obstacles[index].Cross(bounce, ref bounce); index++) { }
		return index != obstacles.Length;
	}

	public bool ApplyCorrection(ref DataFrameState state)
	{
		var delta = _timeTarget - DateTime.FromBinary(state.Time);
		if(delta.TotalSeconds > 0)
		{
			return true;
		}

		// must full path increment sequence (this approx)
		_targetBallPos += _targetBallSpeed * (float)delta.TotalSeconds;

		state.BallPositionX = _targetBallPos.x;
		state.BallPositionY = _targetBallPos.y;
		state.BallSpeedX = _targetBallSpeed.x;
		state.BallSpeedY = _targetBallSpeed.y;

		Debug.Log("apply correction");

		return false;
	}
}
