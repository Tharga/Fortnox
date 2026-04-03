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
    CompanyInformation = 1 << 0,

    /// <summary>
    /// License: Kundfaktura or Order
    /// Resouce: Customers
    /// </summary>
    Customer = 1 << 1,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Articles, Article URL Connections
    /// </summary>
    Article = 1 << 2,

    /// <summary>
    /// License: Order
    /// Resouce: Offers
    /// </summary>
    Offer = 1 << 3,

    /// <summary>
    /// License: Order
    /// Resouce: Orders
    /// </summary>
    Order = 1 << 4,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Contract Accruals, Contract Templates, Contracts, Invoice Accruals, Invoices, Tax Reductions
    /// </summary>
    Invoice = 1 << 5,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Print Templates
    /// </summary>
    Print = 1 << 6,

    /// <summary>
    /// License: Bokföring or Kundfaktura
    /// Resouce: Account Charts, Accounts, Financial Years, SIE, Voucher External URL Connections, Voucher Series, Vouchers.
    /// </summary>
    Bookkeeping = 1 << 7,

    /// <summary>
    /// License: Any
    /// Resouce: Company Settings, Labels, Mode Of Payments, PredefinedAccounts, Terms Of Deliveries, Terms Of Payments, Units, Way Of Deliveries
    /// </summary>
    Settings = 1 << 8,

    /// <summary>
    /// License: Order or Kundfaktura
    /// Resouce: Price Lists, Prices
    /// </summary>
    Price = 1 << 9,

    /// <summary>
    /// License: Any
    /// Resouce: Archive
    /// </summary>
    Archive = 1 << 10,

    /// <summary>
    /// License: Bokföring or Anläggning or Arkivplats
    /// Resouce: Article File Connections, Supplier Invoice File Connections, Voucher File Connections
    /// </summary>
    ConnectFile = 1 << 11,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Cost Centers
    /// </summary>
    CostCenter = 1 << 12,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Currencies
    /// </summary>
    Currency = 1 << 13,

    /// <summary>
    /// License: Any
    /// Resouce: Inbox
    /// </summary>
    Inbox = 1 << 14,

    /// <summary>
    /// License: Kundfaktura
    /// Resouce: Nox Finans Invoice
    /// </summary>
    NoxFinansInvoice = 1 << 15,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Invoice Payments, Supplier Invoice Payments
    /// </summary>
    Payment = 1 << 16,

    /// <summary>
    /// License: Any
    /// Resouce: Profile
    /// </summary>
    Profile = 1 << 17,

    /// <summary>
    /// License: Bokföring or Order or Kundfaktura
    /// Resouce: Projects
    /// </summary>
    Project = 1 << 18,

    /// <summary>
    /// License: Lön
    /// Resouce: Absence transactions, Attendance transactions, Employees, Expenses, Salary transactions, Schedule times, Vacation Debt Basis
    /// </summary>
    Salary = 1 << 19,

    /// <summary>
    /// License: Bokföring
    /// Resouce: Suppliers
    /// </summary>
    Supplier = 1 << 20,

    /// <summary>
    /// License: Bokföring
    /// Resouce: Supplier Invoice Accruals, Supplier Invoice External URL Connections, Supplier Invoices
    /// </summary>
    SupplierInvoice = 1 << 21,

    /// <summary>
    /// License: Tidredovisning
    /// Resouce: Time Reporting
    /// </summary>
    TimeReporting = 1 << 22,
}