using System.Security.Cryptography;
using TokenService.Configuration.IdentityServer;
using Xunit;

namespace TokenServiceTest.Configuration.IdentityServer
{
    public class SigningCredentialDataTest
    {
        private readonly SigningCredentialData sut = new SigningCredentialData();

        [Fact]
        public void SimpleRsaTest()
        {
            var rsa = new RSACryptoServiceProvider(512);
            var parameters = rsa.ExportParameters(true);
            
            sut.ReadRsaParameters(parameters);

            var recreatedParameters = sut.ToRsaParameters();

            Assert.Equal(parameters.P, recreatedParameters.P);
            Assert.Equal(parameters.Q, recreatedParameters.Q);
            Assert.Equal(parameters.Modulus, recreatedParameters.Modulus);
            Assert.Equal(parameters.Exponent, recreatedParameters.Exponent);
            Assert.Equal(parameters.D, recreatedParameters.D);
            Assert.Equal(parameters.DP, recreatedParameters.DP);
            Assert.Equal(parameters.DQ, recreatedParameters.DQ);
            Assert.Equal(parameters.InverseQ, recreatedParameters.InverseQ);
        }
    }
}