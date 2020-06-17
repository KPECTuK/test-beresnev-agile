using UnityEngine;

internal class ControllerBall : MonoBehaviour
{
	private MeshRenderer _renderer;

	private void Awake()
	{
		_renderer = transform.GetComponent<MeshRenderer>();
	}

	private void Update()
	{
		if(Application.isPlaying)
		{
			var color = Composition.ControllerSim?.BallColor ?? Color.black;
			if(!_renderer.sharedMaterial.color.Equals(color))
			{
				_renderer.sharedMaterial.color = color;
			}

			transform.position =
				Composition.ControllerSim.GetBallPos() *
				Composition.ModelScaleFactor;
			var screenDiameter = Composition.ModelScaleFactor *
				(Composition.ControllerSim?.BallRadius ?? .2f) *
				2f;
			transform.localScale = new Vector3(screenDiameter, screenDiameter, .01f);
		}
	}
}
