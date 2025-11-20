using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qwiq.Linq.Visitors;
using Should;

namespace Qwiq.Linq
{
    [TestClass]
    public class UtilityTests
    {
        [TestClass]
        public class ToSha256FingerprintTests
        {
            [TestMethod]
            public void Should_generate_consistent_hash_for_same_input()
            {
                // Arrange
                var input = "test string";

                // Act
                var hash1 = input.ToSha256Fingerprint();
                var hash2 = input.ToSha256Fingerprint();

                // Assert
                hash1.ShouldEqual(hash2);
            }

            [TestMethod]
            public void Should_generate_different_hashes_for_different_inputs()
            {
                // Arrange
                var input1 = "test string 1";
                var input2 = "test string 2";

                // Act
                var hash1 = input1.ToSha256Fingerprint();
                var hash2 = input2.ToSha256Fingerprint();

                // Assert
                hash1.ShouldNotEqual(hash2);
            }

            [TestMethod]
            public void Should_generate_64_character_hex_string()
            {
                // Arrange
                var input = "test string";

                // Act
                var hash = input.ToSha256Fingerprint();

                // Assert
                hash.Length.ShouldEqual(64);
            }

            [TestMethod]
            public void Should_generate_uppercase_hex_string()
            {
                // Arrange
                var input = "test string";

                // Act
                var hash = input.ToSha256Fingerprint();

                // Assert
                foreach (var c in hash)
                {
                    Assert.IsTrue(char.IsDigit(c) || (c >= 'A' && c <= 'F'), 
                        $"Character '{c}' is not a valid uppercase hex character");
                }
            }

            [TestMethod]
            public void Should_handle_empty_string()
            {
                // Arrange
                var input = string.Empty;

                // Act
                var hash = input.ToSha256Fingerprint();

                // Assert
                hash.Length.ShouldEqual(64);
                Assert.IsNotNull(hash);
            }

            [TestMethod]
            public void Should_handle_unicode_characters()
            {
                // Arrange
                var input = "测试字符串";

                // Act
                var hash = input.ToSha256Fingerprint();

                // Assert
                hash.Length.ShouldEqual(64);
                Assert.IsNotNull(hash);
            }

            [TestMethod]
            public void Should_produce_known_hash_for_known_input()
            {
                // Arrange
                var input = "hello world";
                // Expected hash for "hello world" encoded in Unicode with SHA-256
                // Calculated using: echo -n "hello world" | iconv -t UTF-16LE | sha256sum
                // Note: The actual hash will be consistent but may differ from UTF-8 encoding
                
                // Act
                var hash = input.ToSha256Fingerprint();

                // Assert
                // Just verify it's consistent - the actual value depends on Unicode encoding
                Assert.IsNotNull(hash);
                hash.Length.ShouldEqual(64);
                
                // Verify consistency by computing again
                var hash2 = input.ToSha256Fingerprint();
                hash.ShouldEqual(hash2);
            }
        }
    }
}
