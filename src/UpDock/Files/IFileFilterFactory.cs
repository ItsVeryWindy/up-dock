﻿namespace UpDock.Files
{
    public interface IFileFilterFactory
    {
        IFileFilter Create(IConfigurationOptions options);
    }
}
