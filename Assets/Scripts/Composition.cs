using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;

public static class Composition
{
	// immutable
	public static DataMeta DataMeta;
	public static DataAccount DataAccount;
	public static IControllerSim ControllerSim;
	public static ControllerScreens ControllerScreens;
	public static ControllerCameraRig ControllerCameraRig;
	public static IControllerInput ControllerInput;
	// volatile
	public static volatile NetState NetState;
	public static volatile DataMatchStatus[] Matches;
	//
	public static readonly ConcurrentQueue<Action> Marshal = new ConcurrentQueue<Action>();

	public static float ModelScaleFactor;

#if UNITY_EDITOR
	[MenuItem("test-beresnev/Clear application settings")]
	public static void ClearApplicationPrefs()
	{
		PlayerPrefs.DeleteKey(Application.installerName);
	}

	[MenuItem("test-beresnev/Clear account settings")]
	public static void ClearAccountPrefs()
	{
		PlayerPrefs.DeleteKey($"{Application.companyName}.account");
	}
#endif

	//

	public static DataAccount LoadAccount()
	{
		try
		{
			var value = PlayerPrefs.GetString($"{Application.companyName}.account");
			var result = JsonUtility.FromJson<DataAccount>(value);
			return result;
		}
		catch(Exception exception)
		{
			Debug.Log(exception);
		}
		return null;
	}

	public static void SaveAccount(DataAccount data)
	{
		PlayerPrefs.SetString(
			$"{Application.companyName}.account",
			JsonUtility.ToJson(data));
		PlayerPrefs.Save();
	}

	public static DataMeta LoadSettings()
	{
		try
		{
			var value = PlayerPrefs.GetString(Application.installerName);
			var result = JsonUtility.FromJson<DataMeta>(value);
			return result;
		}
		catch(Exception exception)
		{
			Debug.Log(exception);
		}
		return null;
	}

	public static void SaveSettings(DataMeta data)
	{
		PlayerPrefs.SetString(
			Application.installerName,
			JsonUtility.ToJson(data));
		PlayerPrefs.Save();
	}

	//

	public static void StartNetwork(string address, int port, string key, string deviceId)
	{
		// не используется защита,
		// приложение простое,
		// структура максимально упрощена,
		// развитие этого решения не предполагается

		var state = new NetState();
		if(Interlocked.CompareExchange(ref NetState, state, null) == null)
		{
			state.SessionStart(address, port, key, deviceId);
		}
	}

	public static void StopNetwork()
	{
		Interlocked.Exchange(ref NetState, null)?.Dispose();
	}

	//

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetDeviceId()
	{
		return Guid.NewGuid().ToString();
	}

	public static bool IsPreferTouch()
	{
#if UNITY_EDITOR
		return false;
#else
		return Input.touchSupported;
#endif
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DataFrameState FrameDefault(DataMeta dataMeta)
	{
#if UNITY_EDITOR
		var speed =
			Vector2.down *
			dataMeta.BallInitialSpeed;
#else
		var speed =
			UnityEngine.Random.insideUnitCircle.normalized *
			dataMeta.BallInitialSpeed;
#endif
		return new DataFrameState
		{
			Time = 0,
			//
			PaddleLocalPosition = 0f,
			PaddleLocalSpeed = 0f,
			PaddleLocalReflectorOffset = dataMeta.PaddleReflectorOffset,
			PaddleLocalReflectorHalfSize = dataMeta.PaddleReflectorHalfSize,
			//
			PaddleRemotePosition = 0f,
			PaddleRemoteSpeed = 0f,
			PaddleRemoteReflectorOffset = dataMeta.PaddleReflectorOffset,
			PaddleRemoteReflectorHalfSize = dataMeta.PaddleReflectorHalfSize,
			//
			BallPositionX = 0f,
			BallPositionY = 0f,
			BallSpeedX = speed.x,
			BallSpeedY = speed.y,
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DataFrameState FrameConsent(DataMeta dataMeta, NetState network)
	{
		var @default = FrameDefault(dataMeta);
		@default.BallSpeedX = network.IsLead
			? network.Local.BallSpeedX
			: network.Remote.BallSpeedX;
		@default.BallSpeedY = network.IsLead
			? network.Local.BallSpeedY
			: -network.Remote.BallSpeedY;
		return @default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FrameSwap(ref DataFrameState container)
	{
		var temp = new DataFrameState();
		// local to local
		temp.PaddleLocalSpeed = container.PaddleLocalSpeed;
		temp.PaddleLocalPosition = container.PaddleLocalPosition;
		temp.PaddleLocalReflectorOffset = container.PaddleLocalReflectorOffset;
		temp.PaddleLocalReflectorHalfSize = container.PaddleLocalReflectorHalfSize;
		// remote to local
		container.PaddleLocalSpeed = container.PaddleRemoteSpeed;
		container.PaddleLocalPosition = container.PaddleRemotePosition;
		container.PaddleLocalReflectorOffset = container.PaddleRemoteReflectorOffset;
		container.PaddleLocalReflectorHalfSize = container.PaddleRemoteReflectorHalfSize;
		// local to remote
		container.PaddleRemoteSpeed = temp.PaddleLocalSpeed;
		container.PaddleRemotePosition = temp.PaddleLocalPosition;
		container.PaddleRemoteReflectorOffset = temp.PaddleLocalReflectorOffset;
		container.PaddleRemoteReflectorHalfSize = temp.PaddleLocalReflectorHalfSize;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Color BallColorConsent()
	{
		return NetState.IsLead
			? NetState.Local.BallColor
			: NetState.Remote.BallColor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float BallRadiusConsent()
	{
		return NetState.IsLead
			? NetState.Local.BallRadius
			: NetState.Remote.BallRadius;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 GetLocalPaddleReflectorPos(this IControllerSim source)
	{
		return new Vector2(
			source?.Frame.PaddleLocalPosition ?? 0,
			-.5f + source?.Frame.PaddleLocalReflectorOffset ?? 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 GetRemotePaddleReflectorPos(this IControllerSim source)
	{
		return new Vector2(
			source?.Frame.PaddleRemotePosition ?? 0f,
			.5f - source?.Frame.PaddleRemoteReflectorOffset ?? 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 GetBallPos(this IControllerSim source)
	{
		return new Vector2(
			source?.Frame.BallPositionX ?? 0f,
			source?.Frame.BallPositionY ?? 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf<T>(this IScreen[] source) where T : IScreen
	{
		var index = 0;
		for(; index < source.Length; index++)
		{
			if(source[index] is T)
			{
				break;
			}
		}
		return index == source.Length ? -1 : index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetBy<T>(this T[] source, int index)
	{
		return index < 0 ? default : source[index];
	}

	//

	public static void UpdateBallColor()
	{
		DataMeta.BallColor = Color.HSVToRGB(
			DataMeta.BallHue,
			DataMeta.BallSaturation,
			DataMeta.BallBrightness);
		ControllerSim.BallColor = DataMeta.BallColor;
	}
}
