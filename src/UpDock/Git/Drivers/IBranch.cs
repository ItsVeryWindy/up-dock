namespace UpDock.Git.Drivers
{
    public interface IBranch
    {
        string Name { get; }
        string FullName { get; }
        bool IsRemote { get; }

        void Checkout();
        void Checkout(bool force);
        public IRemoteBranch Track(IRemote remote);
    }
}
