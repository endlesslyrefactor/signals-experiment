using FluentAssertions;
using Signals;

namespace SignalsTests;

public class Tests
{
    [Test]
    public void FunctionUpdated_ValueChanges()
    {
        var coordinator = new Coordinator();
        var sut = new Signal<int>(coordinator, () => 4);

        sut.Value.Should().Be(4);
        
        sut.UpdateFunc(() => 12);

        sut.Value.Should().Be(12);
    }

    [Test]
    public void FunctionUpdated_ValueChangesForSinks()
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
    public void FunctionUpdated_MarkedAsDirty()
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
    public void FunctionUpdated_SinksAreMarkedDirty()
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
    
    [Test]
    public void FunctionUpdated_NewSourcesAdded()
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
    
    [Test]
    public void FunctionUpdated_UnusedSourcesAreRemoved()
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
    
    [Test]
    public void FunctionUpdated_EffectTriggered()
    {
        var coordinator = new Coordinator();
        
        var testSignal = new Signal<int>(coordinator, () => 8);

        (int?, int?) effectResult = (null, null);
        testSignal.AddEffect((x, y) => effectResult = (x, y));

        testSignal.UpdateFunc(() => 9);
        
        effectResult.Should().Be((8, 9));
    }

    [Test]
    public void SignalIsDirty_ValueComputed_EffectTriggered()
    {
        var coordinator = new Coordinator();
        
        var firstSignal = new Signal<int>(coordinator, () => 8);
        var secondSignal = new Signal<int>(coordinator, () => firstSignal.Value * 2);

        (int?, int?) effectResult = (null, null);
        secondSignal.AddEffect((x, y) => effectResult = (x, y));

        firstSignal.UpdateFunc(() => 9);

        secondSignal.IsDirty.Should().BeTrue();
        secondSignal.Value.Should().Be(18);
        effectResult.Should().Be((16, 18));

        effectResult = (null, null);
        secondSignal.Value.Should().Be(18);
        effectResult.Should().Be((null, null));
    }
    
    // TODO: Check it works with reference types
}