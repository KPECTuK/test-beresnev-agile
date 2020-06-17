using UnityEngine;

public class ControllerSimLobby : IControllerSim
{
	// апдейт положения камеры или мяча может быть здесь (не в экране)

	public DataFrameState Frame { get; }
	public Color BallColor { get => Composition.DataMeta.BallColor; set => Composition.DataMeta.BallColor = value; }
	public float BallRadius { get => Composition.DataMeta.BallRadius; set => Composition.DataMeta.BallRadius = value; }

	public ControllerSimLobby()
	{
		Frame = Composition.FrameDefault(Composition.DataMeta);
	}

	public bool IsSimComplete()
	{
		return true;
	}

	public IObstacleFinisher GetFirstCompleteFinisher()
	{
		return null;
	}

	public void PushRemote(DataFrameState frame) { }

	public void Update(IControllerInput controllerInput) { }
}
