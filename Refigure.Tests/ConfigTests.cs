using NUnit.Framework;

namespace Refigure.Tests
{
    public class ConfigTests
    {
		[Test]
	    public void GetAsBoolSilent()
	    {
		    Assert.That(Config.GetAsBoolSilent("Tests.GetAsBoolSilent_True"), Is.True);
		    Assert.That(Config.GetAsBoolSilent("Tests.GetAsBoolSilent_False"), Is.False);
		    Assert.That(Config.GetAsBoolSilent("Tests.GetAsBoolSilent_DoesNotExist"), Is.Null);
	    }
    }
}
