namespace PhysgunDiscordBot;

public class Cfg(IConfiguration cfg)
{
    public string Address { get; init; } = cfg[$"GmodServer:{nameof(Address)}"]!;
    public ushort Port { get; init; } = ushort.Parse(cfg[$"GmodServer:{nameof(Port)}"]!);
}