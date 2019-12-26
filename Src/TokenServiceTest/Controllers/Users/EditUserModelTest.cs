using System.Security.Claims;
using System.Web;
using IdentityModel;
using TokenService.Controllers.Users;
using Xunit;

namespace TokenServiceTest.Controllers.Users
{
    public class PickPasswordControllerTest
    {
        
    }
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

        [Fact]
        public void HttpUrlEncodeTest()
        {
            Assert.Equal("A+B", HttpUtility.UrlDecode(HttpUtility.UrlEncode("A+B")));
            
        }
    }
}