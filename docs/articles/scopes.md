# Scopes

`FortnoxScope` is a `[Flags]` enum — combine the resources your app needs with `|` and pass the result to `BuildConnectUriAsync`. Fortnox includes those scopes in the consent screen the user sees.

```csharp
var scopes =
    FortnoxScope.CompanyInformation
    | FortnoxScope.Bookkeeping
    | FortnoxScope.Customer
    | FortnoxScope.Invoice;

var uri = await connectionService.BuildConnectUriAsync(requestKey, scopes);
```

## Reference

Authoritative list: [Fortnox scopes](https://www.fortnox.se/developer/guides-and-good-to-know/scopes). Each scope maps to one or more Fortnox resources and requires the listed license.

| Scope | License | Resource |
|---|---|---|
| `CompanyInformation` | Any | Company Information |
| `Customer` | Kundfaktura or Order | Customers |
| `Article` | Order or Kundfaktura | Articles, Article URL Connections |
| `Offer` | Order | Offers |
| `Order` | Order | Orders |
| `Invoice` | Order or Kundfaktura | Contract Accruals, Contract Templates, Contracts, Invoice Accruals, Invoices, Tax Reductions |
| `Print` | Order or Kundfaktura | Print Templates |
| `Bookkeeping` | Bokföring or Kundfaktura | Account Charts, Accounts, Financial Years, SIE, Voucher External URL Connections, Voucher Series, Vouchers |
| `Settings` | Any | Company Settings, Labels, Modes of Payment, Predefined Accounts, Terms of Delivery, Terms of Payment, Units, Way of Delivery |
| `Price` | Order or Kundfaktura | Price Lists, Prices |
| `Archive` | Any | Archive |
| `ConnectFile` | Bokföring or Anläggning or Arkivplats | Article File Connections, Supplier Invoice File Connections, Voucher File Connections |
| `CostCenter` | Bokföring or Order or Kundfaktura | Cost Centers |
| `Currency` | Bokföring or Order or Kundfaktura | Currencies |
| `Inbox` | Any | Inbox |
| `NoxFinansInvoice` | Kundfaktura | Nox Finans Invoice |
| `Payment` | Bokföring or Order or Kundfaktura | Invoice Payments, Supplier Invoice Payments |
| `Profile` | Any | Profile |
| `Project` | Bokföring or Order or Kundfaktura | Projects |
| `Salary` | Lön | Absence transactions, Attendance transactions, Employees, Expenses, Salary transactions, Schedule times, Vacation Debt Basis |
| `Supplier` | Bokföring | Suppliers |
| `SupplierInvoice` | Bokföring | Supplier Invoice Accruals, Supplier Invoice External URL Connections, Supplier Invoices |
| `TimeReporting` | Tidredovisning | Time Reporting |

## Picking scopes

Request the **smallest set you can**. Two reasons:

- **User trust.** Fortnox shows the requested scopes on the consent screen. Asking for `Salary` when you only read invoices makes users (rightly) suspicious.
- **License gating.** If the user's Fortnox subscription doesn't include the licensed component a scope requires, granting fails. Asking for `Salary` from a customer without the Lön module means they cannot complete the connection at all.

If your app has optional features (e.g. "import bookkeeping vouchers"), consider building separate connect URIs for the base set and the optional set, and reconnecting only when a user opts into the larger feature.
