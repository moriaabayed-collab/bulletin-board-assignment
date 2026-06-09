using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BulletinBoardAPI.Models;
using BulletinBoardAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BulletinBoardAPI.Tests.Services;

public class TokenServiceTests
{
    private const string Key = "test-super-secret-key-that-is-long-enough-32chars!!";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = Key,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        _sut = new TokenService(config);
    }

    private JwtSecurityToken ParseToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
        };

        new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out var validatedToken);
        return (JwtSecurityToken)validatedToken;
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var user = new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var token = _sut.GenerateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt()
    {
        var user = new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var token = _sut.GenerateToken(user);

        var parsed = ParseToken(token);
        Assert.NotNull(parsed);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectSubClaim()
    {
        var user = new User { Id = 42, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var token = _sut.GenerateToken(user);

        var parsed = ParseToken(token);
        Assert.Equal("42", parsed.Subject);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectEmailClaim()
    {
        var user = new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var token = _sut.GenerateToken(user);

        var parsed = ParseToken(token);
        var emailClaim = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal("john@example.com", emailClaim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsJtiClaim()
    {
        var user = new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var token = _sut.GenerateToken(user);

        var parsed = ParseToken(token);
        var jti = parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jti);
        Assert.False(string.IsNullOrWhiteSpace(jti.Value));
    }

    [Fact]
    public void GenerateToken_TwoCallsProduceDifferentJti()
    {
        var user = new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var token1 = ParseToken(_sut.GenerateToken(user));
        var token2 = ParseToken(_sut.GenerateToken(user));

        var jti1 = token1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = token2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        Assert.NotEqual(jti1, jti2);
    }

    [Fact]
    public void GenerateToken_ExpiresApproximatelyIn60Minutes()
    {
        var user = new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var before = DateTime.UtcNow.AddMinutes(59);
        var token = _sut.GenerateToken(user);
        var after = DateTime.UtcNow.AddMinutes(61);

        var parsed = ParseToken(token);
        Assert.True(parsed.ValidTo >= before && parsed.ValidTo <= after);
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        var user = new User { Id = 1, Email = "john@example.com", FirstName = "John", LastName = "Doe" };

        var token = _sut.GenerateToken(user);

        var parsed = ParseToken(token);
        Assert.Equal(Issuer, parsed.Issuer);
        Assert.Contains(Audience, parsed.Audiences);
    }
}
