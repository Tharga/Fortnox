namespace Tharga.Fortnox;

/// <summary>
/// Specification for different access scopes.
/// https://www.fortnox.se/developer/guides-and-good-to-know/scopes
/// </summary>
[Flags]
public enum FortnoxScope
{
    /// <summary>
    /// License: Any
    /// Resouce: Company Information
    /// </summary>
    CompanyInformation = 1,

    /// <summary>
    /// License: Kundfaktura or Order
    /// Resouce: Customers
    /// </summary>
    Customer = 2,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Articles, Article URL Connections
    /// </summary>
    Article = 4,

    /// <summary>
    /// License: Order
    /// Resouce: Offers
    /// </summary>
    Offer = 8,

    /// <summary>
    /// License: Order
    /// Resouce: Orders
    /// </summary>
    Order = 16,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Contract Accruals, Contract Templates, Contracts, Invoice Accruals, Invoices, Tax Reductions
    /// </summary>
    Invoice = 32,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Print Templates
    /// </summary>
    Print = 64,

    /// <summary>
    /// License: Bokföring or Kundfaktura
    /// Resouce: Account Charts, Accounts, Financial Years, SIE, Voucher External URL Connections, Voucher Series, Vouchers.
    /// </summary>
    Bookkeeping = 128,

    /// <summary>
    /// License: Any
    /// Resouce: Company Settings, Labels, Mode Of Payments, PredefinedAccounts, Terms Of Deliveries, Terms Of Payments, Units, Way Of Deliveries
    /// </summary>
    Settings = 256,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Price Lists, Prices
    /// </summary>
    Price = 512,

    /// <summary>
    /// License: Any
    /// Resouce: Archive
    /// </summary>
    Archive = 1024,

    /// <summary>
    /// License: Bokföring or Anläggning or Arkivplats
    /// Resouce: Article File Connections, Supplier Invoice File Connections, Voucher File Connections
    /// </summary>
    ConnectFile = 2048,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Cost Centers
    /// </summary>
    CostCenter = 4096,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Currencies
    /// </summary>
    Currency = 8192,

    /// <summary>
    /// License: Any
    /// Resouce: Inbox
    /// </summary>
    Inbox = 16384,

    /// <summary>
    /// License: Kundfaktura
    /// Resouce: Nox Finans Invoice
    /// </summary>
    NoxFinansInvoice = 32768,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Invoice Payments, Supplier Invoice Payments
    /// </summary>
    Payment = 65536,

    /// <summary>
    /// License: Any
    /// Resouce: Profile
    /// </summary>
    Profile = 131072,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Projects
    /// </summary>
    Project = 262144,

    /// <summary>
    /// License: Lön
    /// Resouce: Absence transactions, Attendance transactions, Employees, Expenses, Salary transactions, Schedule times, Vacation Debt Basis
    /// </summary>
    Salary = 524288,

    /// <summary>
    /// License: Bokföring
    /// Resouce: Suppliers
    /// </summary>
    Supplier = 1048576,

    /// <summary>
    /// License: Bokföring
    /// Resouce: Supplier Invoice Accruals, Supplier Invoice External URL Connections, Supplier Invoices
    /// </summary>
    SupplierInvoice = 2097152,

    /// <summary>
    /// License: Tidredovisning
    /// Resouce: Time Reporting
    /// </summary>
    TimeReporting = 4194304,

    //TODO: Try to use this. Check if the numbers are the same
    /*
       CompanyInformation   = 1 << 0,
       Customer             = 1 << 1,
       Article              = 1 << 2,
       Offer                = 1 << 3,
       Order                = 1 << 4,
       Invoice              = 1 << 5,
       Print                = 1 << 6,
       Bookkeeping          = 1 << 7,
       Settings             = 1 << 8,
       Price                = 1 << 9,
       Archive              = 1 << 10,
       ConnectFile          = 1 << 11,
       CostCenter           = 1 << 12,
       Currency             = 1 << 13,
       Inbox                = 1 << 14,
       NoxFinansInvoice     = 1 << 15,
       Payment              = 1 << 16,
       Profile              = 1 << 17,
       Project              = 1 << 18,
       Salary               = 1 << 19,
       Supplier             = 1 << 20,
       SupplierInvoice      = 1 << 21,
       TimeReporting        = 1 << 22
     */
}