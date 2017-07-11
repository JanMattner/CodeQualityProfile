namespace CodeQualityProfile.Client.FileSystem
{
    public interface IFileHelper
    {
        /// <summary>Copies an existing file to a new file. Overwriting a file of the same name is allowed.</summary>
        /// <param name="source">The file to copy. </param>
        /// <param name="destination">The name of the destination file. This cannot be a directory. </param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false. </param>
        void Copy(string source, string destination, bool overwrite);
    }
}