using EPiServer.ContentApi.Core.Serialization;
using EPiServer.ContentApi.Core.Serialization.Internal;
using EPiServer.ContentApi.Core.Serialization.Models;
using EPiServer.SpecializedProperties;
using JetBrains.Annotations;

namespace Foundation.BlogRelatedCode;

public interface IXhtmlStringPropertyService
{
    string DoSomething(ConverterContext converterContext);
}

public class XhtmlStringPropertyService : IXhtmlStringPropertyService
{
    private readonly IXhtmlStringPropertyRenderer _xhtmlStringPropertyRenderer;

    public XhtmlStringPropertyService([NotNull] IXhtmlStringPropertyRenderer xhtmlStringPropertyRenderer)
    {
        _xhtmlStringPropertyRenderer = xhtmlStringPropertyRenderer ?? throw new ArgumentNullException(nameof(xhtmlStringPropertyRenderer));
    }

    public string DoSomething(ConverterContext converterContext) => 
        new XhtmlPropertyModel(new PropertyXhtmlString("something"), converterContext)?.Value;
}