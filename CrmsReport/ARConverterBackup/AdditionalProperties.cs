using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsReport {
  public partial class CarQuoteReport
  {
    public string SlipNumber { get; set; }
    public string RevisionNumber { get; set; }
  }
  public partial class CarSalesOrderReport
  {
    public string SlipNumber { get; set; }
    public string RevisionNumber { get; set; }
    public string ReportName { get; set; }
    public bool VisibleCustomer { get; set; }
    public bool FurikomiTesuryoIsVisible { get; set; }
  }
  public partial class CarRegistRequest
  {
    public string SlipNumber { get; set; }
    public string RevisionNumber { get; set; }
  }
  public partial class CarOwnRegistRequest
  {
    public string SlipNumber { get; set; }
    public string RevisionNumber { get; set; }
  }
  public partial class CarDeliveryReport
  {
    public string SlipNumber { get; set; }
    public string RevisionNumber { get; set; }
    public string HikaeTitle { get; set; }
  }
  public partial class CarDeliveryReport_hikae
  {
    public string SlipNumber { get; set; }
    public string RevisionNumber { get; set; }
    public string HikaeTitle { get; set; }
  }

  public class CarReceiptData
  {
    public DateTime? ReceiptDate { get; set; }
    public decimal? ReceiptAmount { get; set; }
    public string ReceiptType { get; set; }
  }

  public partial class CarReceiptReport
  {
    public string SlipNumber { get; set; }
    public string RevisionNumber { get; set; }
  }
  public partial class CarPurchaseAgreementReport
  {
    public string HikaeName { get; set; }
  }
}
