namespace AmorousEditor.Readers
{
    /// <summary>
    /// Base Reader class containing some headers
    /// </summary>
    public class BaseReader
    {
        /// <summary>
        /// XNB Target Header
        /// <list type="bullet">
        /// <listheader>
        /// <term>Target</term>
        /// <term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>w</term>
        /// <term>Microsoft Windows</term>
        /// </item>
        /// <item>
        /// <term>m</term>
        /// <term>Windows Phone 7</term>
        /// </item>
        /// <item>
        /// <term>x</term>
        /// <term>Xbox 360</term>
        /// </item>
        /// </list>
        /// </summary>
        public char target;

        /// <summary>
        /// XNB Format Version
        /// <list type="bullet">
        /// <listheader>
        /// <term>Version</term>
        /// <term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>5</term>
        /// <term>XNA Game Studio 4.0</term>
        /// </item>
        /// </list>
        /// </summary>
        public int formatVersion;

        /// <summary>
        /// Content is for HiDef profile (otherwise Reach)
        /// </summary>
        public bool hidef;

        /// <summary>
        /// Asset data is compressed
        /// </summary>
        public bool compressed;

        /// <summary>
        /// XNB Reader
        /// </summary>
        public string xnbReader;
    }
}
