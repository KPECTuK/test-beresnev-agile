using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataAccount
{
	public string IdUser;
	public string NameUser;
	public string SessionToken;
}

[Serializable]
public class DataMeta
{
	// ReSharper disable InconsistentNaming
	public string DeviceId;
	public float CourtRatio;
	public float PaddleSpeed;
	public float PaddleReflectorOffset;
	public float PaddleReflectorHalfSize;
	public float BallRadius;
	public Color BallColor;
	public float BallInitialSpeed;
	public DataScore[] Scores;
	[NonSerialized]
	public float BallHue;
	[NonSerialized]
	public float BallSaturation;
	[NonSerialized]
	public float BallBrightness;
	// ReSharper restore InconsistentNaming
}

[Serializable]
public struct DataScore
{
	// ReSharper disable InconsistentNaming
	public string IdUser;
	public string Name;
	public int Value;
	// ReSharper restore InconsistentNaming
}

public class ComparerDataScore : IComparer<DataScore>
{
	public int Compare(DataScore x, DataScore y)
	{
		return Comparer<int>.Default.Compare(x.Value, y.Value);
	}
}