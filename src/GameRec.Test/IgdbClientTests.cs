using GameRec.Api.Clients;
using Moq;

namespace GameRec.Test
{
    [TestFixture]
    public class IgdbClientTests
    {
        private IgdbClient _igdbClient;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;

        [SetUp]
        public void Setup()
        {
            var clientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET");
            var client = new HttpClient();

            Assert.That(clientId, Is.Not.Null, "TWITCH_CLIENT_ID envrionment variable is required.");
            Assert.That(clientSecret, Is.Not.Null, "TWITCH_CLIENT_SECRET environment variable is required.");

            _httpClientFactoryMock = new();
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

            _igdbClient = new IgdbClient(_httpClientFactoryMock.Object, clientId, clientSecret);

        }

        [Test]
        public async Task RefreshAuth_ValidResponse_UpdateAuth()
        {
            await _igdbClient.RefreshAuth();
            Assert.That(_igdbClient.HasValidAuth(), "RefreshAuth failed to produce valid auth.");
        }
    }
}