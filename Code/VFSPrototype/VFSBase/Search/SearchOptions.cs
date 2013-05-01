using VFSBase.Implementation;

namespace VFSBase.Search
{
    /// <summary>
    /// Scenarios:
    /// All: File name Search
    /// 
    /// Settings
    /// * Case sensivity / insensitivity
    /// * Regex / Wildcards
    /// * "Edit distance"
    /// 
    /// Modes
    /// * Restrict search to folder
    /// * Restrict search to folder and subfolders
    /// </summary>
    internal class SearchOptions
    {
        internal string Keyword { get; set; }

        internal Folder RestrictToFolder { get; set; }
        internal int RecursionDistance { get; set; }

        internal bool CaseSensitive { get; set; }
        internal bool UseRegex { get; set; }

        public SearchOptions ()
        {
            CaseSensitive = false;
            UseRegex = false;
            RecursionDistance = -1;
        }
    }
}