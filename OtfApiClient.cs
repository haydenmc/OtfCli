// Thanks to Jonathan Miller for unearthing and exposing a lot of the API details
// https://github.com/jonmill/otf-tracker

using System.IdentityModel.Tokens.Jwt;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using OtfCli.Models.Responses;

namespace OtfCli
{
    public class OtfApiClient
    {
        // Not sure where this originated - probably sourced from the OTF app?
        private const string CLIENT_ID = "3dt9jpd58ej69f4183rqjrsu7c"; 
        private const string MANAGEMENT_API_BASE = "https://api.orangetheory.co/";
        private const string WORKOUT_API_BASE = "https://performance.orangetheory.co/";

        public LoginResponse? Login { get; private set; }

        public static async Task<OtfApiClient> CreateAndLoginAsync(string username, string password)
        {
            var client = new OtfApiClient();
            await client.LoginAsync(username, password);
            return client;
        }

        private OtfApiClient() { }

        private async Task LoginAsync(string username, string password)
        {
            var authRequest = new InitiateAuthRequest()
            {
                ClientId = CLIENT_ID,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = {
                    { "USERNAME", username },
                    { "PASSWORD", password },
                }
            };

            var loginClient = new AmazonCognitoIdentityProviderClient(
                new AnonymousAWSCredentials(), Amazon.RegionEndpoint.USEast1);
            
            var authResponse = await loginClient.InitiateAuthAsync(authRequest);

            if (authResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken? parsedToken = 
                    tokenHandler.ReadToken(authResponse.AuthenticationResult.IdToken)
                    as JwtSecurityToken;
                if (parsedToken == null)
                {
                    throw new ApplicationException("Error parsing JWT token");
                }
                Login = new LoginResponse(
                    Email: parsedToken.Claims.Single(c => c.Type == "email").Value,
                    EmailVerified: bool.Parse(
                        parsedToken.Claims.Single(c => c.Type == "email_verified").Value),
                    MemberId: parsedToken.Claims.Single(c => c.Type == "cognito:username").Value,
                    GivenName: parsedToken.Claims.Single(c => c.Type == "given_name").Value,
                    Locale: parsedToken.Claims.Single(c => c.Type == "locale").Value,
                    HomeStudioId: parsedToken.Claims.Single(
                        c => c.Type == "custom:home_studio_id").Value,
                    FamilyName: parsedToken.Claims.Single(c => c.Type == "family_name").Value,
                    IsMigration: bool.Parse(
                        parsedToken.Claims.Single(c => c.Type == "custom:isMigration").Value),
                    JwtToken: authResponse.AuthenticationResult.IdToken,
                    Expiration: parsedToken.ValidTo,
                    IssuedOn: parsedToken.ValidFrom
                );
            }
            else
            {
                throw new UnauthorizedException(
                    "Could not authenticate with specified username and password.");
            }
        }
    }
}