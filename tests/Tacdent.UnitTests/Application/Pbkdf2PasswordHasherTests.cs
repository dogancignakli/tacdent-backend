using Tacdent.Application.Services;

namespace Tacdent.UnitTests.Application;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _sut = new();

    [Fact]
    public void Hash_DoesNotReturnPlaintext()
    {
        const string password = "TacDent-Dev-Admin-2026!";

        var hash = _sut.Hash(password);

        hash.ShouldNotBeNullOrWhiteSpace();
        hash.ShouldNotContain(password);
        hash.ShouldStartWith("pbkdf2-sha256.");
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        const string password = "correct horse battery staple";
        var hash = _sut.Hash(password);

        _sut.Verify(password, hash).ShouldBeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("correct-password");

        _sut.Verify("wrong-password", hash).ShouldBeFalse();
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        const string password = "same-password";

        var first = _sut.Hash(password);
        var second = _sut.Hash(password);

        first.ShouldNotBe(second);
        _sut.Verify(password, first).ShouldBeTrue();
        _sut.Verify(password, second).ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-valid-encoded-hash")]
    [InlineData("pbkdf2-sha256.notanumber.salt.key")]
    public void Verify_WithMalformedHash_ReturnsFalse(string encodedHash)
    {
        _sut.Verify("password", encodedHash).ShouldBeFalse();
    }
}
