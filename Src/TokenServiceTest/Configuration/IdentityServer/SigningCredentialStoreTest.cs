using System;
using System.Linq;
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
            clock.SetupGet(i => i.UtcNow).Returns(Time(0));
            scd = CreateSut();
            signingStore = new SigningCredentialStore(scd);
            validationStore = new ValidationKeysStore(scd);
        }

        private SigningCredentialDatabase CreateSut() => 
            new SigningCredentialDatabase(testDb.NewContext, clock.Object);

        public DateTimeOffset Time(double days) => 
            new DateTimeOffset(1975, 07, 28, 0, 0,0, TimeSpan.Zero).AddDays(days);
        
        [Fact]
        public async Task PersistsSigningKey()
        {
            var cred1 = await signingStore.GetSigningCredentialsAsync();
            var cred2 = await CreateSut().GetSigningCredentialsAsync();
            Assert.Equal(cred1.Kid, cred2.Kid);
            Assert.Equal(Time(14), scd.CacheExpiresAt);
            
        }

        [Theory]
        [InlineData(0,1,14)]
        [InlineData(2,1,14)]
        [InlineData(8,1,14)]
        [InlineData(13,1, 14)]
        [InlineData(13.9,1,14)]
        [InlineData(14,2, 16)]
        [InlineData(15,2, 16)]
        [InlineData(16,2, 16)]
        [InlineData(16.1,1, 30.1)]
        public async Task RollKey(double days, int keys, double nextExpiration)
        {
            await signingStore.GetSigningCredentialsAsync();
            clock.SetupGet(i => i.UtcNow).Returns(Time(days));
            var c2 = await validationStore.GetValidationKeysAsync();            
            Assert.Equal(keys, c2.Count());
            Assert.Equal(Time(nextExpiration), scd.CacheExpiresAt);
        }
    }
}