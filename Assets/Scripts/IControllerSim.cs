using UnityEngine;

public interface IControllerSim
{
	DataFrameState Frame { get; }
	Color BallColor { get; set; }
	float BallRadius { get; set; }
	IObstacleFinisher GetFirstCompleteFinisher();
	void PushRemote(DataFrameState frame);
	void Update(IControllerInput controllerInput);
}
