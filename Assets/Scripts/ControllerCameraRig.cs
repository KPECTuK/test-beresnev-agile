using System.Collections;
using UnityEngine;

public class ControllerCameraRig : MonoBehaviour
{
	// ReSharper disable InconsistentNaming
	public GameObject CameraSetMain;
	public GameObject CameraSetMenu;
	public AnimationCurve Curve;
	// ReSharper restore InconsistentNaming

	private struct Rig
	{
		public Transform Target;
		public Transform Camera;
	}

	private Rig _source;
	private Rig _target;
	private Rig _cameraRig;

	private void Awake()
	{
		Composition.ControllerCameraRig = this;
		_cameraRig = new Rig
		{
			Target = transform,
			Camera = transform.GetComponentInChildren<Camera>().transform,
		};
	}

	private IEnumerator _current;
	private float _currentKey;
	private float _speed;

	public void SetSource(Transform rig)
	{
		_source = new Rig
		{
			Target = rig,
			Camera = rig.GetChild(0).transform
		};
	}

	public void SetTarget(Transform rig)
	{
		_target = new Rig
		{
			Target = rig,
			Camera = rig.GetChild(0).transform
		};
	}

	public void UpdateRig(float keyNormalized)
	{
		var key = Curve.Evaluate(keyNormalized);
		_cameraRig.Target.rotation = Quaternion.Lerp(
			_source.Target.transform.rotation,
			_target.Target.transform.rotation,
			key);
		_cameraRig.Target.position = Vector3.Lerp(
			_source.Target.transform.position,
			_target.Target.transform.position,
			key);
		_cameraRig.Camera.localPosition = Vector3.Lerp(
			_source.Camera.transform.localPosition,
			_target.Camera.transform.localPosition,
			key);
	}
}
