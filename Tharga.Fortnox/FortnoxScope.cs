namespace Tharga.Fortnox;

//NOTE: Florida kräver licenser för Order. (Det skulle gå att ha Kundfaktura istället, fast då kan man inte skapa följesedlar och ordrar)
//https://www.fortnox.se/developer/guides-and-good-to-know/scopes

[Flags]
public enum FortnoxScope
{
    //Scope                     Required Licence in Fortnox
    CompanyInformation = 1,
    Customer = 2,               //Order eller Kundfaktura
    Article = 4,                //Order eller Kundfaktura
    Offer = 8,                  //Order
    Order = 16,                 //Order
    Invoice = 32,               //Order eller Kundfaktura
    Print = 64,                 //Order eller Kundfaktura
    Bookkeeping = 128,          //Bokföring eller Kundfaktura
    Settings = 256,             //Any
    Price = 512,                //Order eller Kundfaktura
    Archive = 1024,
    ConnectFile = 2048,
    CostCenter = 4096,
    Currency = 8192,
    Inbox = 16384,
    NoxFinansInvoice = 32768,
    Payment = 65536,
    Profile = 131072,
    Project = 262144,
    Salary = 524288,
    Supplier = 1048576,
    SupplierInvoice = 2097152,
    TimeReporting = 4194304
}