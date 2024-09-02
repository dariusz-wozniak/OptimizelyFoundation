using EPiServer.PlugIn;
using EPiServer.Scheduler;
using JetBrains.Annotations;
using Mediachase.BusinessFoundation.Data.Meta.Management;
using System.Reflection;
using System.Text;
using MetaClass = Mediachase.MetaDataPlus.Configurator.MetaClass;
using MetaClassCollection = Mediachase.MetaDataPlus.Configurator.MetaClassCollection;

namespace Foundation.Infrastructure.Commerce.ScheduledJobs;

[ScheduledPlugIn(DisplayName = "Remove Commerce obsolete data (list)", 
    Description = "Lists properties that are not present in the model class(es)",
    GUID = "D06262F8-7431-4BCA-9657-5744478F3F70")]
public class RemoveCommerceObsoleteData_List : ScheduledJobBase
{
    private readonly ICommerceObsoleteDataRemover _commerceObsoleteDataRemover;

    public RemoveCommerceObsoleteData_List([NotNull] ICommerceObsoleteDataRemover commerceObsoleteDataRemover)
    {
        IsStoppable = true;
        
        _commerceObsoleteDataRemover = commerceObsoleteDataRemover ?? throw new ArgumentNullException(nameof(commerceObsoleteDataRemover));
    }

    public override string Execute()
    {
        var listPropertiesToBeRemoved = _commerceObsoleteDataRemover.ListPropertiesToBeRemoved();

        if (!listPropertiesToBeRemoved.Any()) return "No properties to be removed";

        var sb = new StringBuilder();
        sb.AppendLine("Properties to be removed: (type.name)");
        listPropertiesToBeRemoved.ForEach(p => sb.Append($"{p.Type.Name}.{p.Name()};"));
        
        return sb.ToString();
    }
}

[ScheduledPlugIn(DisplayName = "Remove Commerce obsolete data (remove)", 
    Description = "Removes properties that are not present in the model class(es). Warning: Please make sure to backup your database before running this job. " +
                  "You may also run the 'list' job first to see what properties will be removed.",
    GUID = "C1B49341-AE96-4FB1-AF25-AC44FC2BB5A5")]
public class RemoveCommerceObsoleteData_Delete : ScheduledJobBase
{
    private readonly ICommerceObsoleteDataRemover _commerceObsoleteDataRemover;

    public RemoveCommerceObsoleteData_Delete([NotNull] ICommerceObsoleteDataRemover commerceObsoleteDataRemover)
    {
        IsStoppable = true;
        
        _commerceObsoleteDataRemover = commerceObsoleteDataRemover ?? throw new ArgumentNullException(nameof(commerceObsoleteDataRemover));
    }

    public override string Execute()
    {
        var listPropertiesToBeRemoved = _commerceObsoleteDataRemover.ListPropertiesToBeRemoved();

        if (!listPropertiesToBeRemoved.Any()) return "No properties to be removed";
        
        _commerceObsoleteDataRemover.Remove(listPropertiesToBeRemoved);

        var sb = new StringBuilder();
        sb.AppendLine("Properties to be removed: (type.name)");
        listPropertiesToBeRemoved.ForEach(p => sb.Append($"{p.Type.Name}.{p.Name()};"));
        
        return sb.ToString();
    }
}

public interface ICommerceObsoleteDataRemover
{
    List<PropertyModel> ListPropertiesToBeRemoved();
    void Remove(List<PropertyModel> listOfPropertiesToRemove);
}

public class CommerceObsoleteDataRemover : ICommerceObsoleteDataRemover
{
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;

    public CommerceObsoleteDataRemover([NotNull] IContentTypeRepository contentTypeRepository,
                                       [NotNull] IPropertyDefinitionRepository propertyDefinitionRepository)
    {
        _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
        _propertyDefinitionRepository = propertyDefinitionRepository ?? throw new ArgumentNullException(nameof(propertyDefinitionRepository));
    }

    public List<PropertyModel> ListPropertiesToBeRemoved()
    {
        var propsToRemove = new List<PropertyModel>();
        var allTypes = _contentTypeRepository.List().ToList();

        // get types that derives from CatalogContentBase:
        var catalogContentBaseTypes = allTypes.Where(t => typeof(CatalogContentBase).IsAssignableFrom(t.ModelType)).ToList();

        foreach (var contentType in catalogContentBaseTypes)
        {
            var objectProps = contentType.ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var pd in contentType.PropertyDefinitions)
            {
                if (objectProps.All(p => p.Name != pd.Name)) propsToRemove.Add(new PropertyModel(contentType.ModelType, pd));
            }
        }

        return propsToRemove;
    }
    
    public void Remove(List<PropertyModel> listOfPropertiesToRemove)
    {
        var ctx = CatalogContext.MetaDataContext;

        var metaClassCollection = MetaClass.GetList(ctx);
        var metaClasses = new List<MetaClass>();
        
        var enumerator = metaClassCollection.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current is MetaClass metaClass) metaClasses.Add(metaClass);
        }

        foreach (var property in listOfPropertiesToRemove)
        {
            var propertyDefinitionType = _propertyDefinitionRepository.Load(property.PropertyDefinition.ID);
            var propertyToRemove = propertyDefinitionType.CreateWritableClone();
            _propertyDefinitionRepository.Delete(propertyToRemove);
            
            var metaClass = metaClasses.FirstOrDefault(x => $"{x.Namespace}.{x.Name}" == property.Type.FullName);
            if (metaClass != null)
            {
                var mc = MetaClass.Load(ctx, metaClass.Id);
                mc.DeleteField(property.Name());
            }
        }
    }
}

public class PropertyModel
{
    public PropertyModel([NotNull] Type type, [NotNull] PropertyDefinition propertyDefinition)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        PropertyDefinition = propertyDefinition ?? throw new ArgumentNullException(nameof(propertyDefinition));
    }

    public Type Type { get; set; }
    public PropertyDefinition PropertyDefinition { get; set; }
    public string Name() => PropertyDefinition.Name;
}