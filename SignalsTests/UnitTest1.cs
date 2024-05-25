using FluentAssertions;
using Signals;

namespace SignalsTests;

public class Tests
{
    [Test]
    public void Test1()
    {
        var coordinator = new Coordinator();
        var sut = new Signal<int>(coordinator, () => 4);

        sut.Value.Should().Be(4);
        
        sut.UpdateFunc(() => 12);

        sut.Value.Should().Be(12);
    }

    [Test]
    public void Test2()
    {
        var coordinator = new Coordinator();
        var intSource = new Signal<int>(coordinator, () => 7);
        var stringSource = new Signal<string>(coordinator, () => "bears");

        var sut = new Signal<string>(coordinator, () => $"{intSource.Value} {stringSource.Value}");

        sut.Value.Should().Be("7 bears");
        
        intSource.UpdateFunc(() => 2);
        
        sut.Value.Should().Be("2 bears");
    }

    [Test]
    public void Test3()
    {
        var coordinator = new Coordinator();
        var sut = new Signal<int>(coordinator, () => 4);

        sut.IsDirty.Should().BeFalse();
        
        sut.UpdateFunc(() => 12);
        
        sut.IsDirty.Should().BeTrue();

        sut.Value.Should().Be(12);

        sut.IsDirty.Should().BeFalse();
    }

    [Test]
    public void Test4()
    {
        var coordinator = new Coordinator();

        var countSignal = new Signal<int>(coordinator, () => 8);
        var evenSignal = new Signal<bool>(coordinator, () => countSignal.Value % 2 == 0);
        var animalSignal = new Signal<string>(coordinator, () => "terrapins");
        var warningSignal = new Signal<string>(coordinator,
            () => evenSignal.Value ? $"THERE ARE AN EVEN NUMBER OF {animalSignal.Value.ToUpper()}" : "All is well");

        warningSignal.Value.Should().Be("THERE ARE AN EVEN NUMBER OF TERRAPINS");
        
        countSignal.UpdateFunc(() => 3);

        countSignal.IsDirty.Should().BeTrue();
        evenSignal.IsDirty.Should().BeTrue();
        animalSignal.IsDirty.Should().BeFalse();
        warningSignal.IsDirty.Should().BeTrue();

        evenSignal.Value.Should().BeFalse();

        countSignal.IsDirty.Should().BeFalse();
        evenSignal.IsDirty.Should().BeFalse();
        animalSignal.IsDirty.Should().BeFalse();
        warningSignal.IsDirty.Should().BeTrue();
    }
    
    // TODO: Add a new source when updating the func
    [Test]
    public void Test5()
    {
        var coordinator = new Coordinator();

        var countSignal = new Signal<int>(coordinator, () => 8);
        var evenSignal = new Signal<bool>(coordinator, () => countSignal.Value % 2 == 0);
        var animalSignal = new Signal<string>(coordinator, () => "terrapins");
        var warningSignal = new Signal<string>(coordinator,
            () => evenSignal.Value ? $"THERE ARE AN EVEN NUMBER OF {animalSignal.Value.ToUpper()}" : "All is well");

        warningSignal.Value.Should().Be("THERE ARE AN EVEN NUMBER OF TERRAPINS");

        countSignal.IsDirty.Should().BeFalse();
        evenSignal.IsDirty.Should().BeFalse();
        animalSignal.IsDirty.Should().BeFalse();
        warningSignal.IsDirty.Should().BeFalse();
        
        var adjectiveSignal = new Signal<string>(coordinator, () => "curious");
        animalSignal.UpdateFunc(() => $"{adjectiveSignal.Value} terrapins");
        warningSignal.Value.Should().Be("THERE ARE AN EVEN NUMBER OF CURIOUS TERRAPINS");
        
        adjectiveSignal.UpdateFunc(() => "angry");

        countSignal.IsDirty.Should().BeFalse();
        evenSignal.IsDirty.Should().BeFalse();
        adjectiveSignal.IsDirty.Should().BeTrue();
        animalSignal.IsDirty.Should().BeTrue();
        warningSignal.IsDirty.Should().BeTrue();
    }
    
    // TODO: Make sure sources are removed when updating a func
    [Test]
    public void Test6()
    {
        var coordinator = new Coordinator();

        var countSignal = new Signal<int>(coordinator, () => 8);
        var evenSignal = new Signal<bool>(coordinator, () => countSignal.Value % 2 == 0);
        var adjectiveSignal = new Signal<string>(coordinator, () => "curious");
        var animalSignal = new Signal<string>(coordinator, () => $"{adjectiveSignal.Value} terrapins");
        var warningSignal = new Signal<string>(coordinator,
            () => evenSignal.Value ? $"THERE ARE AN EVEN NUMBER OF {animalSignal.Value.ToUpper()}" : "All is well");

        warningSignal.Value.Should().Be("THERE ARE AN EVEN NUMBER OF CURIOUS TERRAPINS");

        countSignal.IsDirty.Should().BeFalse();
        evenSignal.IsDirty.Should().BeFalse();
        adjectiveSignal.IsDirty.Should().BeFalse();
        animalSignal.IsDirty.Should().BeFalse();
        warningSignal.IsDirty.Should().BeFalse();
        
        animalSignal.UpdateFunc(() => "terrapins");
        warningSignal.Value.Should().Be("THERE ARE AN EVEN NUMBER OF TERRAPINS");
        adjectiveSignal.UpdateFunc(() => "angry");
        
        countSignal.IsDirty.Should().BeFalse();
        evenSignal.IsDirty.Should().BeFalse();
        adjectiveSignal.IsDirty.Should().BeTrue();
        animalSignal.IsDirty.Should().BeFalse();
        warningSignal.IsDirty.Should().BeFalse();
    }
    
    // TODO: Add effects
    
    // TODO: Check it works with reference types
}