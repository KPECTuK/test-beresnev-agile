using UnityEngine;

public class NetLogger : Nakama.ILogger
{
	public void ErrorFormat(string format, params object[] args)
	{
#if UNITY_EDITOR
		Debug.LogError(string.Format(format, args));
#endif
	}

	public void InfoFormat(string format, params object[] args)
	{
#if UNITY_EDITOR
		Debug.Log(string.Format(format, args));
#endif
	}
}
