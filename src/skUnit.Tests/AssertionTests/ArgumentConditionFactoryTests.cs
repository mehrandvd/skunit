using skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;
using System.Text.Json.Nodes;

namespace skUnit.Tests.AssertionTests;

public class ArgumentConditionFactoryTests
{
    [Fact]
    public void Create_NotEmptyArgumentCondition()
    {
        var factory = new ArgumentConditionFactory();
        var condition = factory.Create(JsonNode.Parse("""["NotEmpty"]"""));

        Assert.Equal(typeof(NotEmptyArgumentCondition), condition.GetType());
    }

    [Fact]
    public void Create_NotEmptyArgumentCondition_InvalidValues()
    {
        var factory = new ArgumentConditionFactory();
        Assert.Throws<InvalidOperationException>(
            () => factory.Create(JsonNode.Parse("""["NotEmpty", "value"]""")));
    }

    [Fact]
    public void Create_EqualsArgumentCondition()
    {
        var factory = new ArgumentConditionFactory();
        var condition = factory.Create(JsonNode.Parse("""["Equals", "value"]"""));

        Assert.Equal(typeof(EqualsArgumentCondition), condition.GetType());
    }

    [Fact]
    public void Create_EqualsArgumentCondition_InvalidValues()
    {
        var factory = new ArgumentConditionFactory();
        Assert.Throws<InvalidOperationException>(
            () => factory.Create(JsonNode.Parse("""["Equals", "value1", "value2"]""")));
    }

    [Fact]
    public void Create_IsAnyOfArgumentCondition()
    {
        var factory = new ArgumentConditionFactory();
        var condition = factory.Create(JsonNode.Parse("""["IsAnyOf", "value1", "value2", "value3"]"""));

        Assert.Equal(typeof(IsAnyOfArgumentCondition), condition.GetType());
    }

    [Fact]
    public void Create_IsAnyOfArgumentCondition_InvalidValues()
    {
        var factory = new ArgumentConditionFactory();
        Assert.Throws<InvalidOperationException>(
            () => factory.Create(JsonNode.Parse("""["IsAnyOf"]""")));
    }

    [Fact]
    public void Create_ContainsAnyArgumentCondition()
    {
        var factory = new ArgumentConditionFactory();
        var condition = factory.Create(JsonNode.Parse("""["ContainsAny", "value1", "value2", "value3"]"""));

        Assert.Equal(typeof(ContainsAnyArgumentCondition), condition.GetType());
    }

    [Fact]
    public void Create_ContainsAnyArgumentCondition_InvalidValues()
    {
        var factory = new ArgumentConditionFactory();
        Assert.Throws<InvalidOperationException>(
            () => factory.Create(JsonNode.Parse("""["ContainsAny"]""")));
    }

    [Fact]
    public void Create_SemanticSimilarArgumentCondition()
    {
        var factory = new ArgumentConditionFactory();
        var condition = factory.Create(JsonNode.Parse("""["SemanticSimilar", "value"]"""));

        Assert.Equal(typeof(SemanticSimilarArgumentCondition), condition.GetType());
    }

    [Fact]
    public void Create_SemanticSimilarArgumentCondition_InvalidValues()
    {
        var factory = new ArgumentConditionFactory();
        Assert.Throws<InvalidOperationException>(
            () => factory.Create(JsonNode.Parse("""["SemanticSimilar"]""")));
    }

    [Fact]
    public void Create_NotSupportedCondition()
    {
        var factory = new ArgumentConditionFactory();
        var exception = Assert.Throws<InvalidOperationException>(
            () => factory.Create(JsonNode.Parse("""["NotSupported"]""")));

        Assert.Equal("Failed to create argument condition. Condition `NotSupported` is not supported.", exception.Message);
    }
}
