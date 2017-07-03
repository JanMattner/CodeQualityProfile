namespace CodeQualityProfile.Client.FileSystem
{
    public interface IFileHelper
    {
        void Copy(string source, string destination, bool overwrite);
    }
}