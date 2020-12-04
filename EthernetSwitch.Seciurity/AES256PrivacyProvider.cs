using System.Diagnostics.CodeAnalysis;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Security;

namespace EthernetSwitch.Security
{
    /// <summary>
    ///     Privacy provider for AES 256.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "AES",
        Justification = "definition")]
    public sealed class AES256PrivacyProvider : AESPrivacyProviderBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AES256PrivacyProvider" /> class.
        /// </summary>
        /// <param name="phrase">The phrase.</param>
        /// <param name="auth">The authentication provider.</param>
        public AES256PrivacyProvider(OctetString phrase, IAuthenticationProvider auth)
            : base(32, phrase, auth)
        {
        }

        /// <summary>
        ///     Returns a string that represents this object.
        /// </summary>
        public override string ToString() => "AES 256 privacy provider";
    }
}