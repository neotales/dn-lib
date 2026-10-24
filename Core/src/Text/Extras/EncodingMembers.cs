using System.Text;

namespace Neotales.Text.Extras
{

    /// <summary>
    /// Represents the EncodingMembers class.
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code lang="csharp">
    /// var instance = default(object);
    /// </code>
    /// </example>
    /// </remarks>
    public static class EncodingMembers
    {
        extension(Encoding)
        {
            /// <summary>
            ///  Gets a UTF-8 encoding without a byte order mark (BOM).
            /// </summary>
            public static Encoding UTF8NoBom => utf8NoBom;
        }

        private static readonly Encoding utf8NoBom = new UTF8Encoding(false, true);
    }
}
