using skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

namespace skUnit.Tests.AssertionTests;

public class FunctionArgumentConditionTests
{
    [Fact]
    public void NotEmptyArgumentCondition_IsMatch_ReturnsTrue()
    {
        var condition = new NotEmptyArgumentCondition();
        Assert.True(condition.IsMatch("value"));
        Assert.Equal("NotEmpty", condition.Name);
        Assert.Empty(condition.ConditionValues);
    }

    [Fact]
    public void NotEmptyArgumentCondition_IsMatch_ReturnsFalse()
    {
        var condition = new NotEmptyArgumentCondition();
        Assert.False(condition.IsMatch(""));
        Assert.Equal("NotEmpty", condition.Name);
        Assert.Empty(condition.ConditionValues);
    }

    [Fact]
    public void EqualsArgumentCondition_IsMatch_ReturnsTrue()
    {
        var condition = new EqualsArgumentCondition("value1");

        Assert.True(condition.IsMatch("value1"));
        Assert.Equal("Equals", condition.Name);

        Assert.Single(condition.ConditionValues);
        Assert.Equal(expected: "value1", condition.ConditionValues[0]);
    }

    [Fact]
    public void EqualsArgumentCondition_IsMatch_ReturnsFalse()
    {
        var condition = new EqualsArgumentCondition("value1");

        Assert.False(condition.IsMatch("value2"));
        Assert.Equal("Equals", condition.Name);

        Assert.Single(condition.ConditionValues);
        Assert.Equal(expected: "value1", condition.ConditionValues[0]);
    }

    [Fact]
    public void IsAnyOfArgumentCondition_IsMatch_ReturnsTrue()
    {
        var condition = new IsAnyOfArgumentCondition(["value1", "value2", "value3"]);

        Assert.True(condition.IsMatch("value1"));
        Assert.Equal("IsAnyOf", condition.Name);

        Assert.Equal(expected: 3, condition.ConditionValues.Length);
        Assert.Equal(expected: "value1", condition.ConditionValues[0]);
        Assert.Equal(expected: "value2", condition.ConditionValues[1]);
        Assert.Equal(expected: "value3", condition.ConditionValues[2]);
    }

    [Fact]
    public void IsAnyOfArgumentCondition_IsMatch_ReturnsFalse()
    {
        var condition = new IsAnyOfArgumentCondition(["value1", "value2", "value3"]);
        
        Assert.False(condition.IsMatch("value"));
        Assert.Equal("IsAnyOf", condition.Name);

        Assert.Equal(expected: 3, condition.ConditionValues.Length);
        Assert.Equal(expected: "value1", condition.ConditionValues[0]);
        Assert.Equal(expected: "value2", condition.ConditionValues[1]);
        Assert.Equal(expected: "value3", condition.ConditionValues[2]);
    }

    [Fact]
    public void ContainsAnyArgumentCondition_IsMatch_ReturnsTrue()
    {
        var condition = new ContainsAnyArgumentCondition(["value1", "value2", "value3"]);

        Assert.True(condition.IsMatch("actual: value1"));
        Assert.Equal("ContainsAny", condition.Name);

        Assert.Equal(expected: 3, condition.ConditionValues.Length);
        Assert.Equal(expected: "value1", condition.ConditionValues[0]);
        Assert.Equal(expected: "value2", condition.ConditionValues[1]);
        Assert.Equal(expected: "value3", condition.ConditionValues[2]);
    }

    [Fact]
    public void ContainsAnyArgumentCondition_IsMatch_ReturnsFalse()
    {
        var condition = new ContainsAnyArgumentCondition(["value1", "value2", "value3"]);

        Assert.False(condition.IsMatch("actual: value4"));
        Assert.Equal("ContainsAny", condition.Name);

        Assert.Equal(expected: 3, condition.ConditionValues.Length);
        Assert.Equal(expected: "value1", condition.ConditionValues[0]);
        Assert.Equal(expected: "value2", condition.ConditionValues[1]);
        Assert.Equal(expected: "value3", condition.ConditionValues[2]);
    }
}
