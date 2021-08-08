namespace UpDock.Git
{
    public class PullRequest
    {
        public string Title { get; }
        public string Head { get; }
        public string Branch { get; }
        public string Body { get; }

        public PullRequest(string title, string head, string branch, string body)
        {
            Title = title;
            Head = head;
            Branch = branch;
            Body = body;
        }
    }
}
