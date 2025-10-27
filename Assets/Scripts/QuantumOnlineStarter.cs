using System;
using System.Threading.Tasks;
using Photon.Deterministic;
using Photon.Realtime;
using Quantum;
using UnityEngine;

public sealed class QuantumOnlineStarter : MonoBehaviour
{
  const int StartTimeoutSec = 20;

  [field: SerializeField] public string PublicPlayerNickname { get; private set; } = "Player";
  
  [SerializeField] string _roomName = "TestRoom";
  [SerializeField] int _maxPlayers = 2;
  
  [SerializeField] RuntimeConfig _runtimeConfig;
  
  RealtimeClient _client;
  QuantumRunner _runner;
  
  bool _isStarting;

  async void Awake()
  {
    await StartOnlineAsync();
  }

  void OnDestroy()
  {
    QuantumRunner.ShutdownAll(immediate: false);
  }

  public async Task StartOnlineAsync()
  {
    if (_isStarting) return;
    
    _isStarting = true;

    try
    {
      var appSettings = new AppSettings(PhotonServerSettings.Global.AppSettings);
      
      var userId = Guid.NewGuid().ToString();

      var mm = new MatchmakingArguments
      {
        PhotonSettings = appSettings,
        PluginName = "QuantumPlugin",
        RoomName = string.IsNullOrWhiteSpace(_roomName) ? null : _roomName,
        MaxPlayers = Math.Max(2, _maxPlayers),
        CanOnlyJoin = false,
        UserId = userId,
      };

      _client = await MatchmakingExtensions.ConnectToRoomAsync(mm);
      
      var args = new SessionRunner.Arguments
      {
        RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
        GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
        ClientId = _client.UserId,
        RuntimeConfig = _runtimeConfig,
        SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
        GameMode = DeterministicGameMode.Multiplayer,
        PlayerCount = 2, 
        StartGameTimeoutInSeconds = StartTimeoutSec,
        Communicator = new QuantumNetworkCommunicator(_client),
      };

      _runner = (QuantumRunner)await SessionRunner.StartAsync(args);

      QuantumCallback.Subscribe(this, (CallbackGameStarted c) => OnGameStarted(c.Game),
        g => g == QuantumRunner.Default.Game);

      if (_runner?.Game?.Session?.IsRunning == true)
        AddLocalPlayer(_runner.Game);
    }
    catch (Exception e)
    {
      Debug.LogException(e);
    }
    finally
    {
      _isStarting = false;
    }
  }

  void OnGameStarted(QuantumGame game)
  {
    AddLocalPlayer(game);
  }

  void AddLocalPlayer(QuantumGame game)
  {
    var rp = new RuntimePlayer { PlayerNickname = PublicPlayerNickname };
    game.AddPlayer(rp);
  }
}
