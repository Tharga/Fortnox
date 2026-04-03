namespace Tharga.Fortnox.Tests;

public class FortnoxScopeTests
{
    [Fact]
    public void All_Values_Are_Distinct_Powers_Of_Two()
    {
        var values = Enum.GetValues<FortnoxScope>();

        foreach (var value in values)
        {
            var intValue = (int)value;
            Assert.True(intValue > 0, $"{value} must be positive");
            Assert.True((intValue & (intValue - 1)) == 0, $"{value} ({intValue}) is not a power of two");
        }
    }

    [Fact]
    public void All_Values_Are_Unique()
    {
        var values = Enum.GetValues<FortnoxScope>().Select(v => (int)v).ToList();

        Assert.Equal(values.Count, values.Distinct().Count());
    }

    [Fact]
    public void Combined_Flags_HasFlag_Works()
    {
        var combined = FortnoxScope.Customer | FortnoxScope.Invoice;

        Assert.True(combined.HasFlag(FortnoxScope.Customer));
        Assert.True(combined.HasFlag(FortnoxScope.Invoice));
        Assert.False(combined.HasFlag(FortnoxScope.Order));
    }

    [Theory]
    [InlineData(FortnoxScope.CompanyInformation, 1)]
    [InlineData(FortnoxScope.Customer, 2)]
    [InlineData(FortnoxScope.Article, 4)]
    [InlineData(FortnoxScope.Offer, 8)]
    [InlineData(FortnoxScope.Order, 16)]
    [InlineData(FortnoxScope.Invoice, 32)]
    [InlineData(FortnoxScope.Print, 64)]
    [InlineData(FortnoxScope.Bookkeeping, 128)]
    [InlineData(FortnoxScope.Settings, 256)]
    [InlineData(FortnoxScope.Price, 512)]
    [InlineData(FortnoxScope.Archive, 1024)]
    [InlineData(FortnoxScope.ConnectFile, 2048)]
    [InlineData(FortnoxScope.CostCenter, 4096)]
    [InlineData(FortnoxScope.Currency, 8192)]
    [InlineData(FortnoxScope.Inbox, 16384)]
    [InlineData(FortnoxScope.NoxFinansInvoice, 32768)]
    [InlineData(FortnoxScope.Payment, 65536)]
    [InlineData(FortnoxScope.Profile, 131072)]
    [InlineData(FortnoxScope.Project, 262144)]
    [InlineData(FortnoxScope.Salary, 524288)]
    [InlineData(FortnoxScope.Supplier, 1048576)]
    [InlineData(FortnoxScope.SupplierInvoice, 2097152)]
    [InlineData(FortnoxScope.TimeReporting, 4194304)]
    public void Scope_Has_Expected_Value(FortnoxScope scope, int expected)
    {
        Assert.Equal(expected, (int)scope);
    }
}
