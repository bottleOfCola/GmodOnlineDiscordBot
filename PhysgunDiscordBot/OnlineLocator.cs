using QueryMaster;
using QueryMaster.GameServer;
using System.Timers;
using Timer =  System.Timers.Timer;

namespace PhysgunDiscordBot;

public class OnlineLocator
{
    public int PlayerCount { get; private set; }
    public bool IsServerAvailable { get; private set; }

    private List<Action<int>> _actions = new();
    private Timer _timer = new(TimeSpan.FromMinutes(5));
    private Cfg _cfg;

    public OnlineLocator(Cfg cfg)
    {
        _timer.Elapsed += (object? sender, ElapsedEventArgs e) => UpdateOnlineCount();
        _timer.Start();
        _cfg = cfg;
    }
    public void UpdateOnlineCount()
    {
        lock(_timer)
        {
            try
            {
                using var server = ServerQuery.GetServerInstance(Game.Garrys_Mod, _cfg.Address, _cfg.Port);
                var p = server.GetPlayers();
                int oldOnline = PlayerCount;
                PlayerCount = p.Count;
                IsServerAvailable = true;
                if (PlayerCount > oldOnline) foreach (var action in _actions) action(PlayerCount);
            }
            catch
            {
                IsServerAvailable = false;
            }
        }
    }

    public void SubscribeOnPlayerChanges(Action<int> action) => _actions.Add(action);
}