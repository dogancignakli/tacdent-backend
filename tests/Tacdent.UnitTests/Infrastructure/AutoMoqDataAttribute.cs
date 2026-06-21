using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit3;

namespace Tacdent.UnitTests.Infrastructure;

public sealed class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() => new Fixture().Customize(new AutoMoqCustomization()))
    {
    }
}

public sealed class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(new AutoMoqDataAttribute(), values)
    {
    }
}
