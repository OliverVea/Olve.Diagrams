using Microsoft.Extensions.DependencyInjection;

namespace TaskDrawer.Tests;

public class StoryListParserUnitTests
{
    private static IServiceProvider _services;
    public StoryListParser StoryListParser => _services.GetRequiredService<StoryListParser>();

    [Before(Class)]
    public static void Before()
    {
        ServiceCollection serviceCollection = new();
        
        serviceCollection.AddTransient<StoryListParser>();

        _services = serviceCollection.BuildServiceProvider();
    }
    
    
    [Test]
    public async Task CreateTaskListParser()
    {
        // Arrange
        
        // Act
        StoryListParser storyListParser = new();

        // Assert
        await Assert.That(storyListParser).IsNotEqualTo(null);
    }

    [Test]
    public async Task? ParseTaskList_OnEmptyList_ReturnsEmptyList()
    {
        // Arrange
        string[] storyList = [];

        // Act
        var actual = StoryListParser.ParseStoryList(storyList);
        
        // Assert
        await Assert.That(actual).IsEmpty();
    }
}