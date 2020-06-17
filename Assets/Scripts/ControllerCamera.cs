using UnityEditor;
using UnityEngine;

public class ControllerCamera : MonoBehaviour
{
	private readonly Ray[] _frustumCast = new Ray[4];
	private readonly Vector3[] _frustumCorners = new Vector3[4];
	private readonly Vector3[] _courtCorners = new Vector3[4];

	private void OnEnable()
	{
		if(Application.isPlaying)
		{
			if(InitializeFrustumBuffers())
			{
				_courtCorners[0] = new Vector3(-Composition.DataMeta.CourtRatio * .5f, -.5f) * Composition.ModelScaleFactor;
				_courtCorners[1] = new Vector3(-Composition.DataMeta.CourtRatio * .5f, .5f) * Composition.ModelScaleFactor;
				_courtCorners[2] = new Vector3(Composition.DataMeta.CourtRatio * .5f, .5f) * Composition.ModelScaleFactor;
				_courtCorners[3] = new Vector3(Composition.DataMeta.CourtRatio * .5f, -.5f) * Composition.ModelScaleFactor;
			}
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if(InitializeFrustumBuffers())
		{
			for(var index = 0; index < _frustumCorners.Length; index++)
			{
				var indexNext = (index + 1) % _frustumCorners.Length;
				Debug.DrawLine(
					_frustumCorners[index],
					_frustumCorners[indexNext],
					Color.black);
			}

			Handles.Label(_frustumCorners[0], $"scale factor: {Composition.ModelScaleFactor}");
		}

		if(Application.isPlaying)
		{
			for(var index = 0; index < _courtCorners.Length; index++)
			{
				var indexNext = (index + 1) % _courtCorners.Length;
				Debug.DrawLine(
					_courtCorners[index],
					_courtCorners[indexNext],
					Color.white);
			}

			if(ReferenceEquals(null, Composition.ControllerSim))
			{
				return;
			}

			// ball
			((Vector3)Composition.ControllerSim.GetBallPos() * Composition.ModelScaleFactor)
				.DebugDrawCircle(
					Quaternion.Euler(90f, 0f, 0f),
					Composition.ControllerSim.BallRadius * Composition.ModelScaleFactor,
					Composition.ControllerSim.BallColor);

			// paddle local
			var paddleLocal = (Vector3)Composition.ControllerSim.GetLocalPaddleReflectorPos() * Composition.ModelScaleFactor;
			var paddleRemote = (Vector3)Composition.ControllerSim.GetRemotePaddleReflectorPos() * Composition.ModelScaleFactor;
			var halfSize = Composition.ControllerSim.Frame.PaddleLocalReflectorHalfSize * Composition.ModelScaleFactor;
			Debug.DrawLine(
				paddleLocal + Vector3.left * halfSize,
				paddleLocal,
				Composition.ControllerInput.IsMovingLeft ? Color.blue : Color.green);
			Debug.DrawLine(
				paddleLocal,
				paddleLocal + Vector3.right * halfSize,
				Composition.ControllerInput.IsMovingRight ? Color.blue : Color.green);

			// paddle local
			Debug.DrawLine(
				paddleRemote + Vector3.left * halfSize,
				paddleRemote,
				Color.green);
			Debug.DrawLine(
				paddleRemote,
				paddleRemote + Vector3.right * halfSize,
				Color.green);
		}
	}
#endif

	private bool InitializeFrustumBuffers()
	{
		var cameraComponent = GetComponent<Camera>();
		if(ReferenceEquals(null, cameraComponent))
		{
			return false;
		}

		var courtPlane = new Plane(Vector3.back, 0);
		_frustumCast[0] = cameraComponent.ViewportPointToRay(new Vector3(0f, 0f));
		_frustumCast[1] = cameraComponent.ViewportPointToRay(new Vector3(1f, 0f));
		_frustumCast[2] = cameraComponent.ViewportPointToRay(new Vector3(1f, 1f));
		_frustumCast[3] = cameraComponent.ViewportPointToRay(new Vector3(0f, 1f));

		for(var index = 0; index < _frustumCast.Length; index++)
		{
			if(courtPlane.Raycast(_frustumCast[index], out var enter))
			{
				_frustumCorners[index] =
					_frustumCast[index].origin +
					_frustumCast[index].direction * enter;
			}
		}

		// vertical 1f in model to value on screen
		Composition.ModelScaleFactor = (_frustumCorners[1] - _frustumCorners[2]).magnitude;

		return true;
	}
}
