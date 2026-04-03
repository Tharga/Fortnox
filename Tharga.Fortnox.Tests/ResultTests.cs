namespace Tharga.Fortnox.Tests;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_ReturnsTrue()
    {
        var result = Result.Success;

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Success_Message_IsNull()
    {
        var result = Result.Success;

        Assert.Null(result.Message);
    }

    [Fact]
    public void Success_Code_IsNull()
    {
        var result = Result.Success;

        Assert.Null(result.Code);
    }

    [Fact]
    public void Fail_IsSuccess_ReturnsFalse()
    {
        var result = Result.Fail("error", "500");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Fail_Message_IsSet()
    {
        var result = Result.Fail("something went wrong", "400");

        Assert.Equal("something went wrong", result.Message);
    }

    [Fact]
    public void Fail_Code_IsSet()
    {
        var result = Result.Fail("error", "404");

        Assert.Equal("404", result.Code);
    }
}

public class ResultOfTTests
{
    [Fact]
    public void Success_IsSuccess_ReturnsTrue()
    {
        var result = Result<string>.Success("data");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Success_Value_IsSet()
    {
        var result = Result<int>.Success(42);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Fail_IsSuccess_ReturnsFalse()
    {
        var result = Result<string>.Fail("error", "500");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Fail_Value_IsDefault()
    {
        var result = Result<string>.Fail("error", "500");

        Assert.Null(result.Value);
    }

    [Fact]
    public void Fail_Message_And_Code_AreSet()
    {
        var result = Result<string>.Fail("not found", "404");

        Assert.Equal("not found", result.Message);
        Assert.Equal("404", result.Code);
    }
}
