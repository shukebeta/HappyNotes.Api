using System.Security.Cryptography;
using Api.Framework.Helper;

namespace Api.Framework.Tests.HelperTests;

[TestFixture]
public class TextEncryptionHelperTests
{
    private const string TestKey = "my_secure_key";
    private const string TestPlainText = "This is a test string.";

    [Test]
    public void Encrypt_ShouldReturnNonEmptyString()
    {
        var encryptedText = TextEncryptionHelper.Encrypt(TestPlainText, TestKey);
        Assert.IsFalse(string.IsNullOrEmpty(encryptedText), "Encrypted text should not be null or empty.");
    }

    [Test]
    public void EncryptDecrypt_ShouldReturnOriginalString()
    {
        var encryptedText = TextEncryptionHelper.Encrypt(TestPlainText, TestKey);
        var decryptedText = TextEncryptionHelper.Decrypt(encryptedText, TestKey);
        Assert.That(decryptedText, Is.EqualTo(TestPlainText), "Decrypted text should match the original plain text.");
    }

    [Test]
    public void Decrypt_WithWrongKey_ShouldThrowException()
    {
        var encryptedText = TextEncryptionHelper.Encrypt(TestPlainText, TestKey);
        Assert.Throws<CryptographicException>(
            () => TextEncryptionHelper.Decrypt(encryptedText, "wrong_key"),
            "Decrypting with the wrong key should throw an exception.");
    }

    [Test]
    public void EncryptDecrypt_WithDifferentKeys_ShouldNotReturnOriginalString()
    {
        var encryptedText = TextEncryptionHelper.Encrypt(TestPlainText, TestKey);
        Assert.Throws<CryptographicException>(() =>
            TextEncryptionHelper.Decrypt(encryptedText, "different_key"));
    }
}
