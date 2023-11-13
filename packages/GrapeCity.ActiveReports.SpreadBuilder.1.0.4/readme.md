# Introduction

GrapeCity ActiveReports is a unique collection of developer reporting tools that help consume, process, and visualize data in the form of compelling and easy-to-understand reports.

GrapeCity ActiveReports provides a lot of features for developers and end-users, like:
* VisualStudio integration support.
* PaaS support (like Azure Linux App Service).
* Different possibilities to pivot and aggregate data.
* Rich data visualization.
* Popular export formats (like PDF/Excel/Word).
* And a lot more (https://www.grapecity.com/activereportsnet).

# Concept

This package includes the .NET and .NET Core assemblies for reading, manipulating and writing Excel files. So you can create Excel spreadsheets cell by cell.

**Example:**
```c#
// Create a Workbook and add a sheet to its Sheets collection
GrapeCity.SpreadBuilder.Workbook sb = new GrapeCity.SpreadBuilder.Workbook();
sb.Sheets.AddNew();
  
// Set up properties and values for columns, rows and cells as desired
sb.Sheets[0].Name = "Customer Call List";
sb.Sheets[0].Columns(0).Width = 2 * 1440;
sb.Sheets[0].Columns(1).Width = 1440;
sb.Sheets[0].Columns(2).Width = 1440;
sb.Sheets[0].Rows(0).Height = 1440 / 4;
  
// Header row
sb.Sheets[0].Cell(0,0).SetValue("Company Name");
sb.Sheets[0].Cell(0,0).FontBold = true;
sb.Sheets[0].Cell(0,1).SetValue("Contact Name");
sb.Sheets[0].Cell(0,1).FontBold = true;
sb.Sheets[0].Cell(0,2).SetValue("Phone");
sb.Sheets[0].Cell(0,2).FontBold = true;
  
// First row of data
sb.Sheets[0].Cell(1,0).SetValue("GrapeCity");
sb.Sheets[0].Cell(1,1).SetValue("Mortimer");
sb.Sheets[0].Cell(1,2).SetValue("(425) 880-2601");
  
// Save the Workbook to an Excel file
sb.Save("test.xls");
```
# See also
* [GrapeCity.ActiveReports.Viewer.Win](https://www.nuget.org/packages/GrapeCity.ActiveReports.Viewer.Win/)
* [GrapeCity.ActiveReports.Viewer.Wpf](https://www.nuget.org/packages/GrapeCity.ActiveReports.Viewer.Wpf/)
* [GrapeCity.ActiveReports.Web](https://www.nuget.org/packages/GrapeCity.ActiveReports.Web/)
* [GrapeCity.ActiveReports.Aspnetcore.Viewer](https://www.nuget.org/packages/GrapeCity.ActiveReports.Aspnetcore.Viewer/)
* [GrapeCity.ActiveReports.Blazor.Viewer](https://www.nuget.org/packages/GrapeCity.ActiveReports.Blazor.Viewer/)
* [GrapeCity.ActiveReports.Design.Win](https://www.nuget.org/packages/GrapeCity.ActiveReports.Design.Win/)
* [GrapeCity.ActiveReports.Aspnetcore.Designer](https://www.nuget.org/packages/GrapeCity.ActiveReports.Aspnetcore.Designer/)
* [GrapeCity.ActiveReports.Export.Pdf](https://www.nuget.org/packages/GrapeCity.ActiveReports.Export.Pdf/)