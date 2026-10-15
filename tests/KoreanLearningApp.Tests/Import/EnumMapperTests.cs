using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Infrastructure.Mapster;

namespace KoreanLearningApp.Tests.Import;

public class EnumMapperTests
{
    [Fact]
    public void PartOfSpeechMapper_EveryValue_SurvivesKoreanRoundTrip()
    {
        foreach (var value in Enum.GetValues<PartOfSpeechEnum>())
            Assert.Equal(value, PartOfSpeechMapper.Parse(value.ToKorean()));
    }

    [Fact]
    public void NiklLevelMapper_EveryValue_SurvivesKoreanRoundTrip()
    {
        foreach (var value in Enum.GetValues<NiklLevelEnum>())
            Assert.Equal(value, NiklLevelMapper.Parse(value.ToKorean()));
    }

    [Fact]
    public void TopikLevelMapper_EveryValue_SurvivesCodeRoundTrip()
    {
        foreach (var value in Enum.GetValues<TopikLevelEnum>())
            Assert.Equal(value, TopikLevelMapper.Parse(value.ToCode()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    public void Parse_UnknownValue_ReturnsEmpty(string? value)
    {
        Assert.Equal(PartOfSpeechEnum.Empty, PartOfSpeechMapper.Parse(value));
        Assert.Equal(NiklLevelEnum.Empty, NiklLevelMapper.Parse(value));
        Assert.Equal(TopikLevelEnum.Empty, TopikLevelMapper.Parse(value));
    }
}
