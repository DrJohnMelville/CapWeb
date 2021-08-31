using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Tokens;

namespace TokenService.Configuration.IdentityServer
{
    public class SigningCredentialData
    {
        private const int TokenLifeInDays = 14;
        private const int TokenGracePeriodInDays = 2;

        public string KeyId { get; set; } = null!;
        public byte[] Exponent { get; set; } = Array.Empty<byte>();
        public byte[] Modulus { get; set; } = Array.Empty<byte>();
        public byte[] P { get; set; } = Array.Empty<byte>();
        public byte[] Q { get; set; } = Array.Empty<byte>();
        public DateTimeOffset EffectiveDate { get; set; }
        public DateTimeOffset ExpirationDate() => EffectiveDate.AddDays(TokenLifeInDays);
        public DateTimeOffset EndOfGracePeriodDate() => EffectiveDate.AddDays(TokenLifeInDays + TokenGracePeriodInDays);

        
        public void ReadRsaParameters(RSAParameters source)
        {
            Exponent = source.Exponent!;
            Modulus = source.Modulus!;
            P = source.P!;
            Q = source.Q!;
        }

        #region RoundTripWithRsaParameters
        public RSAParameters ToRsaParameters()
        {
            return Create(P, Q, Exponent, Modulus);
        }

        private static RSAParameters Create(byte[] p, byte[] q, byte[] exponent, byte[] modulus)
        {
            BigInteger iP = ToBigInt(p);
            BigInteger iQ = ToBigInt(q);
            var product = iP * iQ;
            var phiOfN = product - iP - iQ + 1; // OR: (p - 1) * (q - 1);
            var d = ModInverse(ToBigInt(exponent), phiOfN);
            return new RSAParameters
            {
                P = p,
                Q = q,
                Exponent = exponent,
                Modulus = modulus,
                D =  ToByteArray(d) ,
                DP = ToByteArray(d % (iP - 1)),
                DQ = ToByteArray(d % (iQ - 1)),
                InverseQ = ToByteArray(ModInverse(iQ, iP)),
            };
        }

        private static BigInteger ToBigInt(byte[] p) => new BigInteger(p, true, true);

        private static byte[] ToByteArray(BigInteger bigInt) => bigInt.ToByteArray(true, true);

        /// <summary>
        /// Calculates the modular multiplicative inverse of <paramref name="a"/> modulo <paramref name="m"/>
        /// using the extended Euclidean algorithm.
        /// </summary>
        /// <remarks>
        /// This implementation comes from the pseudocode defining the inverse(a, n) function at
        /// https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm
        /// </remarks>
        public static BigInteger ModInverse(BigInteger a, BigInteger n)
        {

            if (n < 0)
            {
                n = -n;
            }

            if (a < 0)
            {
                a = n - (-a % n);
            }

            BigInteger t = 0;
            BigInteger newT = 1;
            var remainder = n;
            var newRemainder = a;
            while (newRemainder != 0)
            {
                var quot = remainder / newRemainder;
                (newT, t) = (t - quot * newT, newT);
                (newRemainder, remainder) = (remainder - quot * newRemainder, newRemainder);
            }

            if (remainder > 1) throw new ArgumentException(a + " is not invertable.");
            if (t < 0) t = t + n;
            return t;
        }
        #endregion

        #region RoundTrip RsaSecurityKey

        public void ReadRsaSecurityKey(RsaSecurityKey other)
        {
            KeyId = other.KeyId;
            ReadRsaParameters(GetParameters(other));
        }

        private RSAParameters GetParameters(RsaSecurityKey key) => 
            key.Rsa != null ? key.Rsa.ExportParameters(includePrivateParameters: true) : key.Parameters;

        public RsaSecurityKey ToRsaSecurityKey() =>
            CryptoHelper.CreateRsaSecurityKey(ToRsaParameters(), KeyId);

        public static SigningCredentialData CreateNewCredential(DateTimeOffset effectiveDate)
        {
            var ret = new SigningCredentialData()
            {
                EffectiveDate = effectiveDate
            };
            ret.ReadRsaSecurityKey(CryptoHelper.CreateRsaSecurityKey());
            return ret;
        }

        public SigningCredentials ToSigningCrendentials() => 
            new SigningCredentials(ToRsaSecurityKey(), SecurityAlgorithms.RsaSha256);

        public SecurityKeyInfo ToSecurityKeyInfo() => 
            new SecurityKeyInfo {Key = ToRsaSecurityKey(), SigningAlgorithm = SecurityAlgorithms.RsaSha256};

        #endregion
    }
}