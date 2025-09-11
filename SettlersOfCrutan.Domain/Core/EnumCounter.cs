namespace SettlersOfCrutan.Domain.Core;

sealed class EnumCounter<TEnum> where TEnum : struct, Enum
{
    // Backing counts per enum value ordinal
    public int[] Counts { get; set; } = new int[Enum.GetValues<TEnum>().Length];

    public int this[TEnum key]
    {
        get => Counts[Convert.ToInt32(key)];
        set => Counts[Convert.ToInt32(key)] = value;
    }

    public void Add(TEnum key, int delta) => Counts[Convert.ToInt32(key)] += delta;

    public IEnumerable<(TEnum Type, int Quantity)> NonZero()
    {
        foreach (var v in Enum.GetValues<TEnum>())
        {
            var q = this[v];
            if (q != 0) yield return (v, q);
        }
    }

    public int Total() => Counts.Sum();
}
