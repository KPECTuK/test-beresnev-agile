using System;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
	public const string ADDRESS = "g-node.kpectuk.ddns.net";
	public const int PORT = 7350;
	public const string SERVER_KEY = "nakama-key";
	public const float MATCH_START_DELAY_SEC = 3f;

	public void Awake()
	{
		if(Application.isPlaying)
		{
			//Composition.DataMeta = Composition.LoadSettings() ??
			Composition.DataMeta =
				new DataMeta
				{
					CourtRatio = 1.1f,
					PaddleSpeed = .5f,
					PaddleReflectorOffset = .05f,
					PaddleReflectorHalfSize = .1f,
					BallColor = Color.white,
					BallRadius = .01f,
					BallInitialSpeed = .5f,
					DeviceId = Composition.GetDeviceId(),
					Scores = new[]
					{
						new DataScore(),
						new DataScore(),
						new DataScore(),
						new DataScore(),
						new DataScore(),
					},
				};
			Composition.DataAccount = Composition.LoadAccount() ?? new DataAccount();
			Composition.StartNetwork(
				ADDRESS,
				PORT,
				SERVER_KEY,
				Composition.DataMeta.DeviceId);

			// randomize ball
			Composition.DataMeta.BallColor = UnityEngine.Random.ColorHSV(0f, 1f, .5f, 1f, .7f, 1f);
			Color.RGBToHSV(
				Composition.DataMeta.BallColor,
				out Composition.DataMeta.BallHue,
				out Composition.DataMeta.BallSaturation,
				out Composition.DataMeta.BallBrightness);
			Composition.DataMeta.BallRadius = .01f + UnityEngine.Random.value * .04f;

			Composition.ControllerInput = Composition.IsPreferTouch()
				? new ControllerInputTouch() as IControllerInput
				: new ControllerInputKeyboard();

			Composition.ControllerScreens.Show<ScreenMeta>();

			Debug.Log($"system info: {(BitConverter.IsLittleEndian ? "LE" : "BE")}");
		}
	}

	private void Update()
	{
		if(Composition.Marshal.TryDequeue(out var action))
		{
			try
			{
				action();
			}
			catch(Exception exception)
			{
				Debug.LogError($"exception in marshal: {exception}");
			}
		}

		Composition.ControllerInput.Update();
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		Composition.SaveSettings(Composition.DataMeta);
		Composition.SaveAccount(Composition.DataAccount);
	}

	public void OnApplicationQuit()
	{
		Composition.StopNetwork();
		Composition.SaveSettings(Composition.DataMeta);
		Composition.SaveAccount(Composition.DataAccount);
	}
}
