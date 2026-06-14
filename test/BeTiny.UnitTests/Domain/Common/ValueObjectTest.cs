using BeTiny.Domain.Common.ValueObjects;

namespace BeTiny.UnitTests.Domain.Common;

public class ValueObjectTest
{
    [Fact]
    public void TestValueObjectEquality()
    {
        var valueObject1 = new TestValueObject("value");
        var valueObject2 = new TestValueObject("value");

        var equalMethodResult = valueObject1.Equals(valueObject2);
        var equalityOperatorResult = valueObject1 == valueObject2;
        var inequalityOperatorResult = valueObject1 != valueObject2;

        equalMethodResult.Should().BeTrue();
        equalityOperatorResult.Should().BeTrue();
        inequalityOperatorResult.Should().BeFalse();
    }

    [Fact]
    public void TestValueObjectInequality()
    {
        var valueObject1 = new TestValueObject("value1");
        var valueObject2 = new TestValueObject("value2");

        var equalMethodResult = valueObject1.Equals(valueObject2);
        var equalityOperatorResult = valueObject1 == valueObject2;
        var inequalityOperatorResult = valueObject1 != valueObject2;

        equalMethodResult.Should().BeFalse();
        equalityOperatorResult.Should().BeFalse();
        inequalityOperatorResult.Should().BeTrue();
    }

    [Fact]
    public void TestValueObjectHashCode()
    {
        var valueObject1 = new TestValueObject("value");
        var valueObject2 = new TestValueObject("value");

        valueObject1.GetHashCode().Should().Be(valueObject2.GetHashCode());
    }

    [Fact]
    public void TestValueObjectNullEquality()
    {
        TestValueObject? valueObject1 = null;
        TestValueObject? valueObject2 = null;

        var equalityOperatorResult = valueObject1 == valueObject2;
        var inequalityOperatorResult = valueObject1 != valueObject2;

        equalityOperatorResult.Should().BeTrue();
        inequalityOperatorResult.Should().BeFalse();
    }

    [Fact]
    public void TestValueObjectNullInequality()
    {
        TestValueObject? valueObject1 = null;
        var valueObject2 = new TestValueObject("value");

        var equalityOperatorResult = valueObject1 == valueObject2;
        var inequalityOperatorResult = valueObject1 != valueObject2;

        equalityOperatorResult.Should().BeFalse();
        inequalityOperatorResult.Should().BeTrue();
    }

    [Fact]
    public void TestValueObjectSameReferenceEquality()
    {
        var valueObject1 = new TestValueObject("value");
        var valueObject2 = valueObject1;

        var equalMethodResult = valueObject1.Equals(valueObject2);
        var equalityOperatorResult = valueObject1 == valueObject2;
        var inequalityOperatorResult = valueObject1 != valueObject2;

        equalMethodResult.Should().BeTrue();
        equalityOperatorResult.Should().BeTrue();
        inequalityOperatorResult.Should().BeFalse();
    }

    [Fact]
    public void TestValueObjectSameReferenceHashCode()
    {
        var valueObject1 = new TestValueObject("value");
        var valueObject2 = valueObject1;

        valueObject1.GetHashCode().Should().Be(valueObject2.GetHashCode());
    }

    [Fact]
    public void TestValueObjectDifferentTypesInequality()
    {
        var valueObject1 = new TestValueObject("value");
        var valueObject2 = new AnotherTestValueObject("value");

        var equalMethodResult = valueObject1.Equals(valueObject2);
        var equalityOperatorResult = valueObject1 == valueObject2;
        var inequalityOperatorResult = valueObject1 != valueObject2;

        equalMethodResult.Should().BeFalse();
        equalityOperatorResult.Should().BeFalse();
        inequalityOperatorResult.Should().BeTrue();
    }
}

internal class TestValueObject : ValueObject
{
    public string Value { get; }

    public TestValueObject(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

internal class AnotherTestValueObject : ValueObject
{
    public string Value { get; }

    public AnotherTestValueObject(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}