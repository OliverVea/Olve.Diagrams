using Microsoft.Extensions.DependencyInjection;

namespace Olve.Diagrams.Timelines;

public static class ServiceExtensions
{
    public static IServiceCollection AddTimelines(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<NewlineProvider>();
        
        serviceCollection.AddSingleton<TimelineSerializer>();
        serviceCollection.AddSingleton<TimelineDeserializer>();
        serviceCollection.AddSingleton<ITimelineSerializer, TimelineSerializationService>();

        return serviceCollection;
    }
}