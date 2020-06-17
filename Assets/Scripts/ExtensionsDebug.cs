using UnityEngine;

public static class ExtensionsDebug
{
	private const float DIM = 1.0f;
	private const float AXIS_GAP = 0.7f;
	private const float DEFAULT_SIZE = .1f;
	private const float DEFAULT_DURATION = 0f;

	public static void DebugDrawSector(
		this Vector3[] vectors,
		Vector3 center,
		Color color,
		bool isGradient,
		float duration = DEFAULT_DURATION)
	{
		if(vectors.Length == 0)
		{
			return;
		}
		Debug.DrawLine(center, vectors[0], color, duration);
		for(var index = 1; index < vectors.Length; index++)
		{
			var colorTemp = isGradient
				? Color.Lerp(color, color * .3f, (float)index / (vectors.Length - 1))
				: color;
			Debug.DrawLine(center, vectors[index], colorTemp);
			Debug.DrawLine(vectors[index - 1], vectors[index], colorTemp);
		}
	}

	public static void DebugDrawPoint(
		this Vector3 position,
		Color color,
		float size = DEFAULT_SIZE,
		float duration = DEFAULT_DURATION)
	{
		var oneOpposite = new Vector3(-1f, 1f);
		Debug.DrawLine(position + Vector3.one * size, position - Vector3.one * size, color, duration);
		Debug.DrawLine(position + oneOpposite * size, position - oneOpposite * size, color, duration);
	}

	public static void DebugDrawCross(
		this Vector3 position,
		Quaternion rotation,
		Color color,
		float size = DEFAULT_SIZE,
		float duration = DEFAULT_DURATION)
	{
		Debug.DrawLine(position + rotation * Vector3.up * size * AXIS_GAP, position + rotation * Vector3.up * size, Color.green * DIM, duration);
		Debug.DrawLine(position, position + rotation * Vector3.up * size * AXIS_GAP, color * DIM, duration);
		Debug.DrawLine(position, position - rotation * Vector3.up * size, color * DIM, duration);

		Debug.DrawLine(position + rotation * Vector3.right * size * AXIS_GAP, position + rotation * Vector3.right * size, Color.red * DIM, duration);
		Debug.DrawLine(position, position + rotation * Vector3.right * size * AXIS_GAP, color * DIM, duration);
		Debug.DrawLine(position, position - rotation * Vector3.right * size, color * DIM, duration);

		Debug.DrawLine(position + rotation * Vector3.forward * size * AXIS_GAP, position + rotation * Vector3.forward * size, Color.blue * DIM, duration);
		Debug.DrawLine(position, position + rotation * Vector3.forward * size * AXIS_GAP, color * DIM, duration);
		Debug.DrawLine(position, position - rotation * Vector3.forward * size, color * DIM, duration);
	}

	public static void DebugDrawArrow(
		this Vector3 start,
		Vector3 pointTo,
		Color color,
		Color colorArrow,
		float duration = DEFAULT_DURATION)
	{
		var dir = pointTo - start;
		Debug.DrawLine(start, start + dir * .8f, color, duration);
		Debug.DrawLine(start + dir * .8f, start + dir, colorArrow, duration);
	}

	public static void DebugDrawCross(
		this Vector3 position,
		Quaternion rotation,
		Color color,
		float duration = DEFAULT_DURATION)
	{
		position.DebugDrawCross(rotation, color, DEFAULT_SIZE, duration);
	}

	public static void DebugDrawCircle(
		this Vector3 position,
		Quaternion rotation,
		float radius,
		Color color,
		float duration = DEFAULT_DURATION)
	{
		const int HALF_PRECISION = 12;
		var vector = Vector3.forward * radius;
		var step = Quaternion.AngleAxis(Mathf.Rad2Deg * Mathf.PI / HALF_PRECISION, Vector3.up);
		for(var index = 0; index < HALF_PRECISION * 2; index++)
		{
			var next = step * vector;
			Debug.DrawLine(
				ConvertSpaceOf(position, rotation, vector),
				ConvertSpaceOf(position, rotation, next),
				color,
				duration);
			vector = next;
		}
	}

	private static Vector3 ConvertSpaceOf(
		Vector3 position,
		Quaternion rotation,
		Vector3 target)
	{
		return rotation * target + position;
	}
}
