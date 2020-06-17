using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CircleCollider2D))]
public class ControlColorSelect : MonoBehaviour
{
	private RectTransform _rectTransform;
	private ScreenBall _screen;
	private CircleCollider2D _collider;

	private void Awake()
	{
		if(Application.isPlaying)
		{
			_rectTransform = GetComponent<RectTransform>();
			_screen = transform.parent.GetComponent<ScreenBall>();
			_collider = transform.GetComponent<CircleCollider2D>();
		}
	}

	private void Update()
	{
		if(Application.isPlaying)
		{
			var radius = _rectTransform.rect.width * .5f;
			_collider.radius = radius;
			_collider.offset = Vector2.down * radius;

			if(_screen.IsControlActive)
			{
				var posScreenPointer = Vector2.zero;
				var isActive = false;
				if(Input.mousePresent && Input.GetMouseButton(0))
				{
					posScreenPointer = Input.mousePosition;
					isActive = true;
				}
				if(Input.touchCount > 0)
				{
					posScreenPointer = Input.touches[0].position;
					isActive = true;
				}
				if(isActive)
				{
					var ray = Camera.main.ScreenPointToRay(posScreenPointer);
					var hit = Physics2D.Raycast(ray.origin, ray.direction);
					if(ReferenceEquals(hit.collider, _collider))
					{
						if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
							_rectTransform,
							posScreenPointer,
							Camera.main,
							out var posLocalPointer))
						{
							var dist = (Vector2.up + posLocalPointer / radius) * .5f;
							var angle = Mathf.Atan2(dist.y, dist.x);

							Composition.DataMeta.BallHue = .5f + angle * .5f / Mathf.PI;
							Composition.DataMeta.BallSaturation = dist.magnitude * 2f;
							Composition.UpdateBallColor();
						}
					}
				}
			}
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if(Application.isPlaying)
		{
			Handles.Label(
				_rectTransform.position,
				$"s: {Composition.DataMeta.BallSaturation:##.###}, h: {Composition.DataMeta.BallHue:##.###}",
				new GUIStyle(GUI.skin.box)
				{
					active = { textColor = Color.white },
					normal = { background = Texture2D.blackTexture }
				});
		}
	}
#endif
}
