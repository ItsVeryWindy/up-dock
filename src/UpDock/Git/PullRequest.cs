namespace UpDock.Git
{
    public class PullRequest
    {
        public string Title { get; }
        public string Branch { get; }
        public string Body { get; }

        public PullRequest(string title, string branch, string body)
        {
            Title = title;
            Branch = branch;
            Body = body;
        }
    }
}
