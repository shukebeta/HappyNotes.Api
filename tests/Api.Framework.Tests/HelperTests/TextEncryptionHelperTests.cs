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
    public void Decrypt_WithWrongKey_ShouldNotProduceOriginalText()
    {
        var encryptedText = TextEncryptionHelper.Encrypt(TestPlainText, TestKey);

        try
        {
            var decryptedText = TextEncryptionHelper.Decrypt(encryptedText, "wrong_key");
            // Scenario 1: No exception is thrown. This is acceptable as long as the original text is not returned.
            // This handles the flaky case where padding bytes are coincidentally valid.
            Assert.That(decryptedText, Is.Not.EqualTo(TestPlainText));
        }
        catch (CryptographicException)
        {
            // Scenario 2: The expected exception is thrown. This is also a success.
            Assert.Pass();
        }
    }

    [Test]
    public void EncryptDecrypt_WithDifferentKeys_ShouldNotReturnOriginalString()
    {
        var encryptedText = TextEncryptionHelper.Encrypt(TestPlainText, TestKey);
        Assert.Throws<CryptographicException>(() =>
            TextEncryptionHelper.Decrypt(encryptedText, "different_key"));
    }
}
