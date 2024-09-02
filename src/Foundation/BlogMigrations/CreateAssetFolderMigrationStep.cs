using EPiServer.Commerce.Internal.Migration.Steps;
using Foundation.Features.Assets;
using JetBrains.Annotations;
using Mediachase.Commerce.Shared;

namespace Foundation.BlogMigrations;

[ServiceConfiguration(typeof(IMigrationStep))]
public class CreateAssetFolderMigrationStep : IMigrationStep
{
    private readonly IContentAssetService _contentAssetService;
    private readonly ContentAssetHelper _contentAssetHelper;

    public CreateAssetFolderMigrationStep([NotNull] IContentAssetService contentAssetService,
                                          [NotNull] ContentAssetHelper contentAssetHelper)
    {
        _contentAssetService = contentAssetService ?? throw new ArgumentNullException(nameof(contentAssetService));
        _contentAssetHelper = contentAssetHelper ?? throw new ArgumentNullException(nameof(contentAssetHelper));
    }

    public bool Execute(IProgressMessenger progressMessenger)
    {
        try
        {
            var startPage = ContentReference.StartPage;
            _contentAssetHelper.GetOrCreateAssetFolder(startPage);

            _contentAssetService.GetOrCreateGlobalAssetFolder("Video files");
        }
        catch (Exception ex)
        {
            return true;
        }

        return true;
    }

    public int Order => 1000;
    public string Name => "Create asset folder programatically";
    public string Description => Name;
}