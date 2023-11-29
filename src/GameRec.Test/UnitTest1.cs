namespace GameRec.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var wf = new gamerec.WeatherForecast();
        Assert.That(wf.TemperatureC, Is.EqualTo(1));
        Assert.Pass();
    }
}