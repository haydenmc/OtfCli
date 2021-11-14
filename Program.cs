namespace OtfCli
{
    public class Program
    {
        public async static Task<int> Main(string[] args)
        {
            var otfClient = await OtfApiClient.CreateAndLoginAsync(args[0], args[1]);
            if (otfClient.Login != null)
            {
                Console.WriteLine(
                    $"Name: {otfClient.Login.GivenName} {otfClient.Login.FamilyName}");
                Console.WriteLine($"Email: {otfClient.Login.Email}");
                Console.WriteLine($"Home Studio ID: {otfClient.Login.HomeStudioId}");
            }
            return 0;
        }
    }
}