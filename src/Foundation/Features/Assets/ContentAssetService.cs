﻿using EPiServer.DataAccess;
using EPiServer.Security;

namespace Foundation.Features.Assets;

public interface IContentAssetService
{
    ContentReference GetOrCreateGlobalAssetFolder(string name);
}

public class ContentAssetService : IContentAssetService
{
    private readonly IContentRepository _contentRepository;

    public ContentAssetService(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository ?? throw new ArgumentNullException(nameof(contentRepository));
    }

    public ContentReference GetOrCreateGlobalAssetFolder(string name)
    {
        var contentFolders = _contentRepository.GetChildren<ContentFolder>(SystemDefinition.Current.GlobalAssetsRoot).ToList();
        
        var dir = contentFolders.FirstOrDefault(x => x.Name == name);
        if (dir != null && !ContentReference.IsNullOrEmpty(dir.ContentLink))
        {
            return dir.ContentLink;
        }

        var newDirectory = _contentRepository.GetDefault<ContentFolder>(SystemDefinition.Current.GlobalAssetsRoot);
        
        newDirectory.Name = name;

        var assetFolder = _contentRepository.Save(newDirectory, SaveAction.Publish,
            AccessLevel.NoAccess);

        return assetFolder;
    }
}
