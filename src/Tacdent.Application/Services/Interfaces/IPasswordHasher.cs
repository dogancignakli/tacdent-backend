namespace Tacdent.Application.Services.Interfaces;

/// <summary>
/// Hashes and verifies passwords. The encoded hash is self-describing so the algorithm and
/// work factor can change over time without a schema migration.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string encodedHash);
}
