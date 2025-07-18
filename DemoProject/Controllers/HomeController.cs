using DemoLibrary;
using DemoProject.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using Rotativa;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace DemoProject.Controllers
{
    public class HomeController : Controller
    {
        BALDemo demo = new BALDemo();
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetUsersJson()
        {
            var users = await Task.Run(()=> demo.GetUsers());
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadTablePartial()  
        {
            return PartialView("_UserTablePartial");
        }

        public async Task<ActionResult> LoadUsersPartial()
        {
            var users = await demo.GetUsers();
            return PartialView("_UserTablePartial", users);
        }

        public async Task<ActionResult> UpdateInsert(int id = 0)
        {
            Demo model;
            if (id != 0)
            {
                model = await demo.GetUserById(id);
            }
            else
            {
                model = new Demo();
            }

            ViewBag.countries = await demo.GetCountries();
            ViewBag.states = await demo.GetStates();
            ViewBag.cities = await demo.GetCities();

            return PartialView("_InsertUpdate", model);
        }

        public async Task<JsonResult> GetStates(int CountryId)
        {
            var states = await demo.GetStates(CountryId);
            return Json(states, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetCities(int StateId)
        {
            var cities = await demo.GetCities(StateId);
            return Json(cities, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> InsertUpdate(Demo d)
        {
            if (d.Id != 0)
            {
                await demo.UpdateUser(d);
            }
            else
            {
                await demo.SaveUser(d);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await demo.DeleteUser(id);
            return Json(new { success = true });
        }

        private string SaveExcelFileToDisk(DataTable dt, string name)
        {
            string folderPath = Server.MapPath("~/export/documents/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{name}_{timestamp}.xlsx";
            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new MemoryStream())
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    SheetData sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Users"
                    };
                    sheets.Append(sheet);

                    uint colCount = (uint)dt.Columns.Count;

                    // 1. Add heading row (merged cells across all columns)
                    Row headingRow = new Row() { RowIndex = 1 };
                    Cell headingCell = CreateTextCell("User Data Report");
                    headingCell.CellReference = "A1";
                    headingRow.Append(headingCell);
                    sheetData.Append(headingRow);

                    // 2. Add datetime row (merged cells across all columns)
                    Row datetimeRow = new Row() { RowIndex = 2 };
                    string generatedOnText = "Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Cell datetimeCell = CreateTextCell(generatedOnText);
                    datetimeCell.CellReference = "A2";
                    datetimeRow.Append(datetimeCell);
                    sheetData.Append(datetimeRow);

                    // 3. Merge those cells for heading and datetime rows
                    MergeCells mergeCells;

                    if (worksheetPart.Worksheet.Elements<MergeCells>().Count() > 0)
                    {
                        mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().First();
                    }
                    else
                    {
                        mergeCells = new MergeCells();
                        // Insert mergeCells after SheetData (if any)
                        worksheetPart.Worksheet.InsertAfter(mergeCells, sheetData);
                    }

                    string lastColumn = GetExcelColumnName((int)colCount);

                    // Merge A1:lastColumn1
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A1:{lastColumn}1") });

                    // Merge A2:lastColumn2
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A2:{lastColumn}2") });

                    // 4. Now add header row (row 3)
                    Row headerRow = new Row() { RowIndex = 3 };
                    uint colIndex = 0;
                    foreach (DataColumn column in dt.Columns)
                    {
                        colIndex++;
                        string cellRef = GetExcelColumnName((int)colIndex) + "3";
                        var cell = CreateTextCell(column.ColumnName);
                        cell.CellReference = cellRef;
                        headerRow.Append(cell);
                    }
                    sheetData.Append(headerRow);

                    // 5. Add data rows (start at row 4)
                    uint rowIndex = 4;
                    foreach (DataRow dr in dt.Rows)
                    {
                        Row row = new Row() { RowIndex = rowIndex };
                        colIndex = 0;
                        foreach (var item in dr.ItemArray)
                        {
                            colIndex++;
                            string cellRef = GetExcelColumnName((int)colIndex) + rowIndex.ToString();
                            var cell = CreateTextCell(item?.ToString());
                            cell.CellReference = cellRef;
                            row.Append(cell);
                        }
                        sheetData.Append(row);
                        rowIndex++;
                    }

                    workbookPart.Workbook.Save();
                }

                System.IO.File.WriteAllBytes(fullPath, stream.ToArray());
                return fileName;
            }
        }

        private Cell CreateTextCell(string text)
        {
            return new Cell()
            {
                DataType = CellValues.String,
                CellValue = new CellValue(text ?? string.Empty)
            };
        }

        // Helper method to convert column number to Excel column letters (1 -> A, 2 -> B, 27 -> AA, etc.)
        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        [HttpPost]
        public async Task<ActionResult> ExportSelectedToExcel()
        {
            string jsonData;
            using (var reader = new StreamReader(Request.InputStream))
            {
                jsonData = await reader.ReadToEndAsync();
            }

            var selectedIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(jsonData);

            if (selectedIds == null || !selectedIds.Any())
                return new HttpStatusCodeResult(400, "No users selected.");

            //var dt = await GetSelectedUsersData(selectedIds);
            BALDemo demo = new BALDemo();
            var allUsers = await demo.GetUsers();
            var selectedUsers = allUsers.Where(u => selectedIds.Contains(u.Id)).ToList();

            var dt = new DataTable("Users");
            dt.Columns.Add("Name");
            dt.Columns.Add("Email");
            dt.Columns.Add("Contact");
            dt.Columns.Add("Gender");
            dt.Columns.Add("Address");
            dt.Columns.Add("Country");
            dt.Columns.Add("State");
            dt.Columns.Add("City");

            foreach (var user in selectedUsers)
            {
                dt.Rows.Add(user.Name, user.Email, user.Contact, user.Gender, user.Address,
                            user.CountryName, user.StateName, user.CityName);
            }

            if (dt == null || dt.Rows.Count == 0)
                return new HttpStatusCodeResult(404, "No user data found.");

            var savedFilename = SaveExcelFileToDisk(dt, "Demo");

            // Construct URL to the saved file - adjust based on your app's base URL
            string fileUrl = Url.Content("~/export/documents/" + savedFilename);

            return Json(new { url = fileUrl });
        }

        // Action to handle PDF export from any table
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> ExportPdfByIdsAndColumns(string ids, string columns)
        {
            try
            {
                var selectedIds = JsonConvert.DeserializeObject<List<int>>(ids);
                var selectedColumns = JsonConvert.DeserializeObject<List<string>>(columns);

                var demo = new BALDemo();
                var allUsers = await demo.GetUsers();
                var selectedUsers = allUsers.Where(u => selectedIds.Contains(u.Id)).ToList();

                string tableHtml = GenerateDynamicHtmlTable(selectedUsers, selectedColumns);

                var model = new HtmlPdfViewModel
                {
                    PageName = "Selected Users",
                    HtmlContent = tableHtml,
                    PageTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                return new ViewAsPdf("RawHtmlView", model)
                {
                    //FileName = "SelectedUsers.pdf",
                    PageSize = Rotativa.Options.Size.A4,
                    PageOrientation = Rotativa.Options.Orientation.Portrait,
                    CustomSwitches = "--no-outline"
                };
            }
            catch (Exception ex)
            {
                return Content("Error generating PDF: " + ex.Message + "\n" + ex.StackTrace, "text/plain");
            }
        }

        private string GenerateDynamicHtmlTable(List<Demo> users, List<string> columns)
        {
            var sb = new StringBuilder();
            sb.Append("<table border='1' cellspacing='0' cellpadding='5' style='border-collapse:collapse;width:100%;text-align:center'>");

            // Table header
            sb.Append("<thead><tr>");
            sb.Append("<th>Sr. No</th>");
            foreach (var col in columns)
            {
                sb.Append($"<th>{col.Replace("Name", " Name")}</th>");
            }
            sb.Append("</tr></thead><tbody>");
            int srNo = 1;

            // Table body
            foreach (var user in users)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{srNo++}</td>");
                foreach (var col in columns)
                {
                    var value = user.GetType().GetProperty(col)?.GetValue(user, null)?.ToString() ?? "";
                    sb.Append($"<td>{value}</td>");
                }
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }

    }
}