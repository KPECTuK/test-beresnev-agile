using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider2D))]
public class ControlColorBright : MonoBehaviour
{
	private RectTransform _rectTransform;
	private ScreenBall _screen;
	private BoxCollider2D _collider;

	private void Awake()
	{
		if(Application.isPlaying)
		{
			_rectTransform = GetComponent<RectTransform>();
			_screen = transform.parent.GetComponent<ScreenBall>();
			_collider = transform.GetComponent<BoxCollider2D>();
		}
	}

	public void Update()
	{
		var rect = _rectTransform.rect;
		_collider.size = rect.size;

		if(Application.isPlaying)
		{
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
							Composition.DataMeta.BallBrightness = posLocalPointer.x / _rectTransform.rect.width + .5f;
							Composition.UpdateBallColor();
						}
					}
				}
			}
		}
	}
}
