using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Tests.Unit.BlogRelatedCode;

public class XhtmlStringPropertyServiceTests
{
    [Fact]
    public void just_a_simple_test()
    {
        IXhtmlStringPropertyService sut = Sut();

        var converterContext = (ConverterContext)FormatterServices.GetUninitializedObject(typeof(ConverterContext));
        
        var something = sut.DoSomething(converterContext);

        something.Should().Be("something different");
    }

    private static IXhtmlStringPropertyService Sut()
    {
        var substituteForRenderer = Substitute.For<IXhtmlStringPropertyRenderer>();
        substituteForRenderer.Render(
                                 Arg.Is<PropertyXhtmlString>(x => x.XhtmlString.ToString() == "something"),
                                 Arg.Is<bool>(x => x == false))
                             .Returns("something different");
        
        var substituteForServiceLocator = Substitute.For<IServiceProvider>();
        substituteForServiceLocator.GetService(typeof(IXhtmlStringPropertyRenderer))
                                   .Returns(substituteForRenderer);
        
        ServiceLocator.SetScopedServiceProvider(substituteForServiceLocator);
        
        // Indirect assertion:
        ServiceLocator.Current.GetRequiredService<IXhtmlStringPropertyRenderer>()
                      .Should().Be(substituteForRenderer);
        
        return new XhtmlStringPropertyService(substituteForRenderer);
    }

    private static IXhtmlStringPropertyService Sut_but_this_does_not_work()
    {
        var substituteForRenderer = Substitute.For<IXhtmlStringPropertyRenderer>();
        substituteForRenderer.Render(
                                 Arg.Is<PropertyXhtmlString>(x => x.XhtmlString.ToString() == "something"),
                                 Arg.Is<bool>(x => x == false))
                             .Returns("something different");

        return new XhtmlStringPropertyService(substituteForRenderer);
    }
}