using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

public sealed class NetState : IDisposable
{
	private const string KEY_BALL_COLOR_R = "ball-color-r";
	private const string KEY_BALL_COLOR_G = "ball-color-g";
	private const string KEY_BALL_COLOR_B = "ball-color-b";
	private const string KEY_BALL_RADIUS = "ball-radius";
	private const string KEY_BALL_SPEED_X = "ball-speed-y";
	private const string KEY_BALL_SPEED_Y = "ball-speed-x";
	private const string KEY_SEED = "seed";
	private const long OP_CODE_TIME_REQUEST = 1;
	private const long OP_CODE_TIME_RESPONSE = 2;
	private const long OP_CODE_STATE = 3;

	private ISession _netSession;
	private IClient _netClient;
	private ISocket _netSocket;

	//sync
	private volatile IMatch _netMatch;
	private readonly List<IUserPresence> _users = new List<IUserPresence>();

	private readonly byte[] _bufferTimeOut;
	private readonly byte[] _bufferFrameOut;
	private readonly DataTimeState[] _timeProbe = new DataTimeState[16];
	private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(200);
	private DateTime _time = DateTime.UtcNow;
	private int _timeProbesReceived;
	//
	private TimeSpan _approxClockLag;

	public bool IsLoggedIn { get; private set; }
	public bool IsConsent
	{
		get
		{
			lock(_users)
			{
				return _users.Count == 2;
			}
		}
	}
	public bool IsTimeSync =>
		Interlocked.CompareExchange(
			ref _timeProbesReceived,
			_timeProbe.Length,
			_timeProbe.Length) ==
		_timeProbe.Length;
	public bool IsLead => Local.Seed < Remote.Seed;
	public int TimeProbeSize => _timeProbe.Length;
	public DateTime SimTime => IsLead ? DateTime.UtcNow : DateTime.UtcNow + _approxClockLag;
	public DataMatchStatus Local { get; }
	public DataMatchStatus Remote { get; }

	public NetState()
	{
		// assume LE all
		_bufferFrameOut = new byte[Marshal.SizeOf<DataFrameState>()];
		_bufferTimeOut = new byte[Marshal.SizeOf<DataTimeState>()];

		Local = new DataMatchStatus();
		Remote = new DataMatchStatus();
	}

	public async void Dispose()
	{
		var task = Interlocked.Exchange(ref _netSocket, null)?.CloseAsync();
		if(!ReferenceEquals(null, task))
		{
			await task;
		}
		_netSession = null;
		_netClient = null;
	}

	public async void SessionStart(string address, int port, string key, string deviceId)
	{
		_netClient = new Client("http", address, port, key) { Logger = new NetLogger() };
		ISession session = null;

		if(!string.IsNullOrWhiteSpace(Composition.DataAccount.SessionToken))
		{
			session = Session.Restore(Composition.DataAccount.SessionToken);
		}

		if(session?.IsExpired ?? true)
		{
			Debug.Log(">> authenticating expired..");

			await OnSessionEstablished(await _netClient.AuthenticateDeviceAsync(deviceId));
		}
		else
		{
			Debug.Log(">> authenticating restored..");

			await OnSessionEstablished(session);
		}
	}

	private async Task OnSessionEstablished(ISession session)
	{
		_netSession = session;

		Composition.DataAccount.IdUser = _netSession.UserId;
		Composition.DataAccount.NameUser = _netSession.Username;
		Composition.DataAccount.SessionToken = _netSession.AuthToken;

		_netSocket = _netClient.NewSocket();
		//
		_netSocket.Closed += () =>
		{
			Debug.Log(">> Socket disconnected..");

			var state = Interlocked.CompareExchange(ref Composition.NetState, null, null);
			state?._netSocket.Dispose();
		};

		_netSocket.Connected += () => { Debug.Log(">> Socket connected.."); };

		_netSocket.ReceivedMatchmakerMatched += _ =>
		{
			Debug.Log(">> Opponent found..");

			MatchJoin(_);
		};

		_netSocket.ReceivedMatchPresence += MatchPresence;
		_netSocket.ReceivedMatchState += OnReceive;
		//
		_netSocket.ReceivedError += e => Debug.LogErrorFormat("Socket error: {0}", e.Message);

		Debug.Log($">> Session established for UserName: {Composition.DataAccount.NameUser} UserId:{Composition.DataAccount.IdUser}");

		await _netSocket.ConnectAsync(session);

		IsLoggedIn = true;
	}

	public async void MatchMakingStart()
	{
		_timeProbesReceived = 0;
		lock(_users)
		{
			_users.Clear();
		}

		Local.NameUser = Composition.DataAccount.NameUser;
		Local.Seed = UnityEngine.Random.value;
		Local.BallColor = Composition.DataMeta.BallColor;
		Local.BallRadius = Composition.DataMeta.BallRadius;
		Local.BallSpeedX = Composition.ControllerSim.Frame.BallSpeedX;
		Local.BallSpeedY = Composition.ControllerSim.Frame.BallSpeedY;

		await _netSocket.AddMatchmakerAsync(
			"*",
			2,
			2,
			new Dictionary<string, string>(),
			new Dictionary<string, double>
			{
				{ KEY_SEED, Local.Seed },
				{ KEY_BALL_COLOR_R, Local.BallColor.r },
				{ KEY_BALL_COLOR_G, Local.BallColor.g },
				{ KEY_BALL_COLOR_B, Local.BallColor.b },
				{ KEY_BALL_RADIUS, Local.BallRadius },
				{ KEY_BALL_SPEED_X, Local.BallSpeedX },
				{ KEY_BALL_SPEED_Y, Local.BallSpeedY },
			});
	}

	private async void MatchJoin(IMatchmakerMatched matched)
	{
		var remote = matched.Users.FirstOrDefault(_1 => !_1.Presence.UserId.Equals(Composition.DataAccount.IdUser));
		Remote.Seed = (float)remote.NumericProperties[KEY_SEED];
		Remote.NameUser = remote.Presence.Username;
		Remote.BallColor = new Color(
			(float)remote.NumericProperties[KEY_BALL_COLOR_R],
			(float)remote.NumericProperties[KEY_BALL_COLOR_G],
			(float)remote.NumericProperties[KEY_BALL_COLOR_B]);
		Remote.BallRadius = (float)remote.NumericProperties[KEY_BALL_RADIUS];
		Remote.BallSpeedX = (float)remote.NumericProperties[KEY_BALL_SPEED_X];
		Remote.BallSpeedY = (float)remote.NumericProperties[KEY_BALL_SPEED_Y];

		if(Local.Seed == Remote.Seed)
		{
			throw new Exception("seeds are equals");
		}

		Debug.Log($">> match found for: {string.Join(", ", matched.Users.Select(_ => _.Presence.Username))}");

		_netMatch = await _netSocket.JoinMatchAsync(matched);
		lock(_users)
		{
			_users.AddRange(_netMatch.Presences);
		}

		Debug.Log($">> match state (joining): {string.Join(", ", _netMatch.Presences.Select(_ => _.Username))}");
	}

	private void MatchPresence(IMatchPresenceEvent @event)
	{
		bool consent;
		string users;

		lock(_users)
		{
			_users.RemoveAll(_ => @event.Leaves.Any(_1 => _.UserId.Equals(_1.UserId)));
			_users.AddRange(@event.Joins);

			// log
			users = string.Join(", ", _users.Select(_ => _.Username));
			consent = IsConsent;
		}

		var joined = string.Join(", ", @event.Joins.Select(_ => _.Username));
		var leaved = string.Join(", ", @event.Leaves.Select(_ => _.Username));

		// TODO: network state switch

		Debug.Log($">> match diff-joins: [{joined}], leaves: [{leaved}]");
		Debug.Log($"users: {users}");
		Debug.Log(consent ? ">> start match" : ">> awaiting for players");
	}

	public async void Send(DataFrameState state, bool urgent)
	{
		// только изменения бит и пинг
		if(!urgent && DateTime.UtcNow - _time < _delay)
		{
			return;
		}

		// только изменения бит
		//if (!urgent)
		//{
		//	return;
		//}

		_time = DateTime.UtcNow;
		state.Time = _time.ToBinary();
		var handle = GCHandle.Alloc(_bufferFrameOut, GCHandleType.Pinned);
		Marshal.StructureToPtr(state, handle.AddrOfPinnedObject(), false);
		handle.Free();

		await _netSocket.SendMatchStateAsync(
			_netMatch.Id,
			OP_CODE_STATE,
			Convert.ToBase64String(_bufferFrameOut));
	}

	private async Task Send(DataTimeState state, long opCode)
	{
		var handle = GCHandle.Alloc(_bufferTimeOut, GCHandleType.Pinned);
		Marshal.StructureToPtr(state, handle.AddrOfPinnedObject(), false);
		handle.Free();

		_time = DateTime.UtcNow;
		await _netSocket.SendMatchStateAsync(
			_netMatch.Id,
			opCode,
			Convert.ToBase64String(_bufferTimeOut));
	}

	public async void Send(int timeProbeIndex)
	{
		var data = new DataTimeState
		{
			Sequence = timeProbeIndex
		};
		SetTimeStamp(ref data);
		await Send(data, OP_CODE_TIME_REQUEST);
	}

	private void OnReceive(IMatchState state)
	{
		if(state.OpCode == OP_CODE_TIME_REQUEST)
		{
			var base64 = Convert.FromBase64String(Encoding.UTF8.GetString(state.State));
			var handle = GCHandle.Alloc(base64, GCHandleType.Pinned);
			var buffer = (DataTimeState)Marshal.PtrToStructure(
				handle.AddrOfPinnedObject(),
				typeof(DataTimeState));
			handle.Free();
			SetTimeStamp(ref buffer);
			Send(buffer, OP_CODE_TIME_RESPONSE);
		}
		else if(state.OpCode == OP_CODE_TIME_RESPONSE)
		{
			var base64 = Convert.FromBase64String(Encoding.UTF8.GetString(state.State));
			var handle = GCHandle.Alloc(base64, GCHandleType.Pinned);
			var buffer = (DataTimeState)Marshal.PtrToStructure(
				handle.AddrOfPinnedObject(),
				typeof(DataTimeState));
			handle.Free();

			var local = IsLead
				? DateTime.FromBinary(buffer.TimeLead)
				: DateTime.FromBinary(buffer.TimeFollow);
			var remote = IsLead
				? DateTime.FromBinary(buffer.TimeFollow)
				: DateTime.FromBinary(buffer.TimeLead);

			_timeProbe[buffer.Sequence] = buffer;
			_timeProbe[buffer.Sequence].Roundtrip = (DateTime.UtcNow - local).Ticks;
			_timeProbe[buffer.Sequence].Compensation = (local - remote).Ticks;

			Interlocked.Increment(ref _timeProbesReceived);
		}
		else if(state.OpCode == OP_CODE_STATE)
		{
			var base64 = Convert.FromBase64String(Encoding.UTF8.GetString(state.State));
			var handle = GCHandle.Alloc(base64, GCHandleType.Pinned);
			var receiver = (DataFrameState)Marshal.PtrToStructure(
				handle.AddrOfPinnedObject(),
				typeof(DataFrameState));
			handle.Free();
			// transform (was set at sim update)
			//var packetTime = DateTime.FromBinary(receiver.Time);
			//packetTime += IsLead ? TimeSpan.Zero : _approxClockLag;
			//receiver.Time = packetTime.ToBinary();
			Composition.FrameSwap(ref receiver);
			// push
			Composition.ControllerSim.PushRemote(receiver);
		}
	}

	public TimeSpan CalculateLag()
	{
		for(var index = 0; index < _timeProbe.Length; index++)
		{
			var builder = new StringBuilder()
				.Append($"{(IsLead ? "ld: " : "fw: ")}")
				.Append($"{_timeProbe[index].Sequence:00}")
				.Append(" (me: ")
				.Append($"{(IsLead ? DateTime.FromBinary(_timeProbe[index].TimeLead) : DateTime.FromBinary(_timeProbe[index].TimeFollow)):O}")
				.Append(") (op: ")
				.Append($"{(IsLead ? DateTime.FromBinary(_timeProbe[index].TimeFollow) : DateTime.FromBinary(_timeProbe[index].TimeLead)):O}")
				.Append(") : ")
				.Append(TimeSpan.FromTicks(_timeProbe[index].Roundtrip))
				.Append(" : ")
				.Append(TimeSpan.FromTicks(_timeProbe[index].Compensation));
			Debug.Log(builder.ToString());
		}

		var approx = 0L;
		for(var index = 0; index < _timeProbe.Length; index++)
		{
			approx += _timeProbe[index].Compensation;
		}
		_approxClockLag = TimeSpan.FromTicks(approx / _timeProbe.Length);
		return _approxClockLag;
	}

	public void SetTimeStamp(ref DataTimeState target)
	{
		if(IsLead)
		{
			target.TimeLead = DateTime.UtcNow.ToBinary();
		}
		else
		{
			target.TimeFollow = DateTime.UtcNow.ToBinary();
		}
	}

	public void SetTimeStamp(ref DataFrameState target)
	{
		target.Time = IsLead
			? DateTime.UtcNow.ToBinary()
			: (DateTime.UtcNow + _approxClockLag).ToBinary();
	}
}
