using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Domain.UnitTests;

internal static class TestIds
{
    public static UserId User(int seed) =>
        UserId.Create(Guid.Parse($"{seed & 0xFFFFFFFF:X8}-0000-0000-0000-000000000000"));
}
