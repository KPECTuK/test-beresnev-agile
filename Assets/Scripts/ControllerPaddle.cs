using UnityEditor;
using UnityEngine;

internal class ControllerPaddle : MonoBehaviour
{
	// ReSharper disable InconsistentNaming
	public Transform Left;
	public Transform Right;
	public Transform Bounce;
	// ReSharper restore InconsistentNaming

	private bool _isRemote;

	private void Awake()
	{
		_isRemote = name.Contains("remote");
	}

	private void Update()
	{
		if(ReferenceEquals(null, Composition.ControllerSim))
		{
			return;
		}

		if(!_isRemote)
		{
			transform.position =
				Composition.ControllerSim.GetLocalPaddleReflectorPos() *
				Composition.ModelScaleFactor;
			SetSizes();
		}
		else
		{
			transform.position =
				Composition.ControllerSim.GetRemotePaddleReflectorPos() *
				Composition.ModelScaleFactor;
			SetSizes();
		}
	}

	private void SetSizes()
	{
		var modelPaddleHalfSze = Composition.ControllerSim?.Frame.PaddleLocalReflectorHalfSize ?? 1f;
		var screenPaddleHalfSize = modelPaddleHalfSze * Composition.ModelScaleFactor;

		Left.localPosition = Vector3.left * screenPaddleHalfSize;
		Right.localPosition = Vector3.right * screenPaddleHalfSize;
		Bounce.localScale =
			Vector3.forward +
			Vector3.up +
			Vector3.right * 2f * screenPaddleHalfSize;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if(Application.isPlaying && !ReferenceEquals(null, Composition.ControllerSim))
		{
			Handles.Label(
				transform.position,
				_isRemote
					? $"mp: {Composition.ControllerSim.Frame.PaddleRemotePosition}"
					: $"mp: {Composition.ControllerSim.Frame.PaddleLocalPosition}");
		}
	}
#endif
}
