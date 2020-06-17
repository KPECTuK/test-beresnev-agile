using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct DataFrameState
{
	public long Time;
	//
	public float PaddleLocalPosition;
	public float PaddleLocalSpeed;
	public float PaddleLocalReflectorOffset;
	public float PaddleLocalReflectorHalfSize;
	//
	public float PaddleRemotePosition;
	public float PaddleRemoteSpeed;
	public float PaddleRemoteReflectorOffset;
	public float PaddleRemoteReflectorHalfSize;
	//
	public float BallPositionX;
	public float BallPositionY;
	public float BallSpeedX;
	public float BallSpeedY;
}

[StructLayout(LayoutKind.Sequential)]
public struct DataTimeState
{
	public int Sequence;
	public long Roundtrip;
	public long TimeLead;
	public long TimeFollow;
	public long Compensation;
}
