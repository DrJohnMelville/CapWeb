using System.Security.Claims;
using IdentityModel;
using TokenService.Controllers.Users;
using Xunit;

namespace TokenServiceTest.Controllers.Users
{
    public class EditUserModelTest
    {
        [Fact]
        public void InitializeFromClaims()
        {
            var claims = new[]
            {
                new Claim(JwtClaimTypes.Email, "email"),
                new Claim(JwtClaimTypes.Name, "Name"),
            };

            var sut = new EditUserModel(claims);
            Assert.Null(sut.Password);
            Assert.Null(sut.PasswordVerification);
            Assert.Equal("email", sut.Email);
            Assert.Equal("Name", sut.FullName);
            
        }
    }
}