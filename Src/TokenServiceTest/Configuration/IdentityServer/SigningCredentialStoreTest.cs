using System;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Moq;
using TokenService.Configuration.IdentityServer;
using TokenServiceTest.TestTools;
using Xunit;

namespace TokenServiceTest.Configuration.IdentityServer
{
    public class SigningCredentialStoreTest
    {
        private readonly Mock<ISystemClock> clock = new Mock<ISystemClock>();
        private readonly TestDatabase testDb = new TestDatabase();
        private readonly SigningCredentialDatabase scd;
        private readonly ISigningCredentialStore signingStore;
        private readonly IValidationKeysStore validationStore;

        public SigningCredentialStoreTest()
        {
            clock.SetupGet(i => i.UtcNow).Returns(new DateTime(1975, 07, 28));
            scd = new SigningCredentialDatabase(testDb.NewContext, clock.Object);
            signingStore = new SigningCredentialStore(scd);
            validationStore = new ValidationKeysStore(scd);
        }

        [Fact]
        public async Task HasSigningKey()
        {
            Assert.NotNull(await signingStore.GetSigningCredentialsAsync());
        }
    }
}