using System.Text;

namespace SettlersOfCrutan.Presentation.Identity;

public static class RandomPasswordGenerator
{
    public static string Generate(int length = 12)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string specials = "!@#$%";
        const string all = upper + lower + digits + specials;

        var rnd = new Random();
        var sb = new StringBuilder();

        // Ensure at least one uppercase, one lowercase, and one digit
        sb.Append(upper[rnd.Next(upper.Length)]);
        sb.Append(lower[rnd.Next(lower.Length)]);
        sb.Append(digits[rnd.Next(digits.Length)]);

        // Fill the rest with random characters
        for (int i = 3; i < length; i++)
            sb.Append(all[rnd.Next(all.Length)]);

        // Shuffle the result so required chars aren't always at the start
        return new string(sb.ToString().OrderBy(_ => rnd.Next()).ToArray());
    }
}
