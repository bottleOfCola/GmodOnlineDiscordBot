using NetCord.Gateway;

namespace PhysgunDiscordBot;

public class WaitingPlayersCountEventSender
{
    private SortedDictionary<int,List<ulong>> _waitingPeople = new();
    private GatewayClient _client;
    private OnlineLocator _onlineLocator;

    public WaitingPlayersCountEventSender(OnlineLocator ol, GatewayClient dsClient)
    {
        _client = dsClient;
        _onlineLocator = ol;
        _onlineLocator.SubscribeOnPlayerChanges(PlayerCountChanged);
    }

    private void PlayerCountChanged(int count)
    {
        List<ulong> acceptedPeople = new();
        List<ulong> idkPeople = new();
        foreach (var waiting in _waitingPeople.Reverse())
        {
            if (waiting.Key <= count)
            {
                acceptedPeople.AddRange(waiting.Value);
                _waitingPeople.Remove(waiting.Key);
            }
            else if (waiting.Key <= count + GetWaitersCount(waiting.Key)) idkPeople.AddRange(waiting.Value);
        }
        foreach (var user in acceptedPeople) _client.Rest.SendMessageAsync(user, new()
        {
            Content = "На сервере, прямо сейчас, играет столько людей, сколько вы ожидаете!"
        });
        foreach(var user in idkPeople) _client.Rest.SendMessageAsync(user, new()
        {
            Content = "Если на сервер зайдет несколько ожидающих человек, то на сервере будет столько онлайна, сколько вы ожидаете!"
        });
    }
    public int GetWaitersCount(int count) => _waitingPeople.Where(x=> x.Key <= count).SelectMany(x=> x.Value).Count();
    public void SetWaiting(int count, ulong discordId)
    {
        try
        {
            var a = _waitingPeople.First(x => x.Value.Contains(discordId));
            if (a.Key == count) return;
            if (a.Value.Count == 1) _waitingPeople.Remove(a.Key);
            else a.Value.Remove(discordId);
        }
        catch { }

        if (_waitingPeople.TryGetValue(count, out List<ulong> waitingUsers))
            waitingUsers.Add(discordId);
        else
            _waitingPeople.Add(count, [discordId]);
        _onlineLocator.UpdateOnlineCount();
        PlayerCountChanged(_onlineLocator.PlayerCount);
    }
}