namespace OrderSystem.Modules.Orders.Services;

public class MockTrackingNumberGenerator : ITrackingNumberGenerator
{
    public string Generate()
    {
        return $"SHIP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
