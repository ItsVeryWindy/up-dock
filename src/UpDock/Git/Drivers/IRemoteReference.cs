namespace UpDock.Git.Drivers
{
    public interface IRemoteReference
    {
        string FullName { get; }

        void Remove();
    }
}
