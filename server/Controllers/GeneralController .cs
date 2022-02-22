using app.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System;

namespace app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GeneralController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(new GeneralResponse
            {
                Points = await _context.Points.ToListAsync(),
                PetitionTypes = await _context.PetitionTypes.OrderByDescending(a=>a.Title).ToListAsync()
            });
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<Petition>> GetDoc(Guid id)
        {
            List<string> arr_point;
            PointChief point_chief;
            string strtmp = "";

            var petition = await _context.Petitions
                            .Include(a => a.Points).ThenInclude(c => c.PointChief)
                            .Include(b => b.Units)
                            .FirstOrDefaultAsync(c => c.Id == id);

            if (petition == null)
            {
                return NotFound();
            }

            var pointsGroupe = petition.Points.GroupBy(a => a.PointChiefId).ToList();

            using (MemoryStream stream = new MemoryStream()) 
            {

                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document)) 
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    foreach (var point in pointsGroupe)
                    {
                        arr_point = new List<string>();
                        point_chief = point.First().PointChief;

                        foreach (var _point in point.AsEnumerable())
                        {
                            arr_point.Add(_point.Title);
                        }

                        string points = String.Join(", ", arr_point);

                        #region Header
                        Table tableHeader = new Table();
                        // Create a TableProperties object and specify its border information.
                        TableProperties tblHeaderProp = new TableProperties(
                            new TableBorders(
                                new TopBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.None),
                                    Size = 6
                                },
                                new BottomBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.None),
                                    Size = 6
                                }
                            )
                        );

                        tableHeader.AppendChild<TableProperties>(tblHeaderProp);

                        TableRow trh1 = new TableRow();
                        TableCell tch11 = new TableCell();
                        tch11.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "5400" }));
                        tch11.Append(new Paragraph(new Run(new Text("от _________ исх. № _______"))));
                        trh1.Append(tch11);
                        TableCell tch12 = new TableCell();
                        tch12.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3400" }));
                        tch12.Append(new Paragraph(new Run(new Text($"Командиру войсковой части { point_chief.Title }"))));
                        trh1.Append(tch12);
                        tableHeader.Append(trh1);

                        TableRow trh2 = new TableRow();
                        TableCell tch21 = new TableCell();
                        tch21.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "5400" }));
                        tch21.Append(new Paragraph(new Run(new Text("на № _________ от _______"))));
                        trh2.Append(tch21);
                        TableCell tch22 = new TableCell();
                        tch22.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3400" }));
                        tch22.Append(new Paragraph(new Run(new Text($"полковнику { point_chief.ChiefFullName }"))));
                        trh2.Append(tch22);
                        tableHeader.Append(trh2);

                        body.AppendChild(tableHeader);
                        #endregion

                        #region Body

                        body.AppendChild(
                            new Paragraph(
                                new Run(
                                    new Text("")
                                )
                            )
                        );


                        body.AppendChild(
                            new Paragraph(
                                    new ParagraphProperties(
                                        new Justification() { Val = JustificationValues.Center }
                                        ),
                                    new Run(
                                        new Text("Ходатайство")
                                    )
                                )
                        );

                        body.AppendChild(
                           new Paragraph(
                               new Run(
                                   new Text("")
                               )
                           )
                        );

                        if (petition.Units.Count > 1 && point.AsEnumerable().Count() > 1)
                        {
                            strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пунктах пропуска «{ points }» через Государственную границу Республики Беларусь должностным лицам, указанным в прилагаемом списке.";
                        }
                        else if (petition.Units.Count > 1 && point.AsEnumerable().Count() == 1)
                        {
                            strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пункте пропуска «{ points }» через Государственную границу Республики Беларусь должностным лицам, указанным в прилагаемом списке.";
                        }
                        else if (petition.Units.Count == 1 && point.AsEnumerable().Count() > 1)
                        {
                            strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пунктах пропуска «{ points }» через Государственную границу Республики Беларусь должностному лицу, указанному в прилагаемом списке.";
                        }
                        else if (petition.Units.Count == 1 && point.AsEnumerable().Count() == 1)
                        {
                            strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пункте пропуска «{ points }» через Государственную границу Республики Беларусь должностному лицу, указанному в прилагаемом списке.";
                        }


                        body.AppendChild(
                            new Paragraph(
                                new ParagraphProperties(
                                    new Justification() { Val = JustificationValues.Both }
                                    ),
                                new Run(
                                    new Text(strtmp)
                                )
                            )
                        );

                        if (petition.Points.Count() > 1 && petition.Vehicle.Length != 0)
                        {
                            strtmp = $"Потребность нахождения в пунктах { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри» на автомобиле {petition.Vehicle}.";
                        }
                        else if (petition.Points.Count() == 1 && petition.Vehicle.Length != 0)
                        {
                            strtmp = $"Потребность нахождения в пункте { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри» на автомобиле {petition.Vehicle}.";
                        }
                        else if (petition.Points.Count() > 1 && petition.Vehicle.Length == 0)
                        {
                            strtmp = $"Потребность нахождения в пунктах { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри».";
                        }
                        else if (petition.Points.Count() == 1 && petition.Vehicle.Length == 0)
                        {
                            strtmp = $"Потребность нахождения в пункте { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри».";
                        }

                        body.AppendChild(
                            new Paragraph(
                                new ParagraphProperties(
                                    new Justification() { Val = JustificationValues.Both }
                                    ),
                                new Run(
                                    new Text(strtmp)
                                )
                            )
                        );

                        Table tableBody = new Table();
                        // Create a TableProperties object and specify its border information.
                        TableProperties tblBodyProp = new TableProperties(
                            new TableBorders(
                                new TopBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.Single),
                                    Size = 6
                                },
                                new BottomBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.Single),
                                    Size = 6
                                },
                                new LeftBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.Single),
                                    Size = 6
                                },
                                new RightBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.Single),
                                    Size = 6
                                },
                                new InsideHorizontalBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.Single),
                                    Size = 6
                                },
                                new InsideVerticalBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.Single),
                                    Size = 6
                                }
                            )
                        );

                        // Append the TableProperties object to the empty table.
                        tableBody.AppendChild<TableProperties>(tblBodyProp);

                        // Create a row.
                        TableRow tr1 = new TableRow();

                        TableCell tc1 = new TableCell();
                        tc1.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc1.Append(new Paragraph(new Run(new Text("п/п"))));
                        tr1.Append(tc1);

                        TableCell tc2 = new TableCell();
                        tc2.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc2.Append(new Paragraph(new Run(new Text("Пункт пропуска"))));
                        tr1.Append(tc2);

                        TableCell tc3 = new TableCell();
                        tc3.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc3.Append(new Paragraph(new Run(new Text("Фамилия, собственное имя, отчество (если таковое имеется)"))));
                        tr1.Append(tc3);

                        TableCell tc4 = new TableCell();
                        tc4.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc4.Append(new Paragraph(new Run(new Text("Дата и место рождения"))));
                        tr1.Append(tc4);

                        TableCell tc5 = new TableCell();
                        tc5.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc5.Append(new Paragraph(new Run(new Text("Данные документа, удостоверяющего его личность (серия (при наличии), номер, кем и когда выдан, личный номер)"))));
                        tr1.Append(tc5);

                        TableCell tc6 = new TableCell();
                        tc6.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc6.Append(new Paragraph(new Run(new Text("Должность"))));
                        tr1.Append(tc6);

                        TableCell tc7 = new TableCell();
                        tc7.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc7.Append(new Paragraph(new Run(new Text("Домашний адрес"))));
                        tr1.Append(tc7);

                        tableBody.Append(tr1);

                        // Create a row.
                        TableRow tr2 = new TableRow();

                        TableCell tc11 = new TableCell();
                        tc11.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc11.Append(new Paragraph(new Run(new Text("1"))));
                        tr2.Append(tc11);

                        TableCell tc12 = new TableCell();
                        tc12.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc12.Append(new Paragraph(new Run(new Text("2"))));
                        tr2.Append(tc12);

                        TableCell tc13 = new TableCell();
                        tc13.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc13.Append(new Paragraph(new Run(new Text("3"))));
                        tr2.Append(tc13);

                        TableCell tc14 = new TableCell();
                        tc14.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc14.Append(new Paragraph(new Run(new Text("4"))));
                        tr2.Append(tc14);

                        TableCell tc15 = new TableCell();
                        tc15.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc15.Append(new Paragraph(new Run(new Text("5"))));
                        tr2.Append(tc15);

                        TableCell tc16 = new TableCell();
                        tc16.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc16.Append(new Paragraph(new Run(new Text("6"))));
                        tr2.Append(tc16);

                        TableCell tc17 = new TableCell();
                        tc17.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc17.Append(new Paragraph(new Run(new Text("7"))));
                        tr2.Append(tc17);

                        tableBody.Append(tr2);


                        foreach (var unit in petition.Units)
                        {
                            TableRow tr3 = new TableRow();

                            TableCell tc21 = new TableCell();
                            tc21.Append(new TableCellProperties(
                                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                            tc21.Append(new Paragraph(new Run(new Text(""))));
                            tr3.Append(tc21);

                            TableCell tc22 = new TableCell();
                            tc22.Append(new TableCellProperties(
                                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                            tc22.Append(new Paragraph(new Run(new Text(points))));
                            tr3.Append(tc22);

                            TableCell tc23 = new TableCell();
                            tc23.Append(new TableCellProperties(
                                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                            tc23.Append(new Paragraph(new Run(new Text(unit.FullName))));
                            tr3.Append(tc23);

                            TableCell tc24 = new TableCell();
                            tc24.Append(new TableCellProperties(
                                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                            tc24.Append(new Paragraph(new Run(new Text(unit.BirthDay.ToShortDateString() + unit.BirthPlace))));
                            tr3.Append(tc24);

                            TableCell tc25 = new TableCell();
                            tc25.Append(new TableCellProperties(
                                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                            tc25.Append(new Paragraph(new Run(new Text(unit.DocumentIdentity))));
                            tr3.Append(tc25);

                            TableCell tc26 = new TableCell();
                            tc26.Append(new TableCellProperties(
                                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                            tc26.Append(new Paragraph(new Run(new Text(unit.Position))));
                            tr3.Append(tc26);

                            TableCell tc27 = new TableCell();
                            tc27.Append(new TableCellProperties(
                                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                            tc27.Append(new Paragraph(new Run(new Text(unit.HomeAdress))));
                            tr3.Append(tc27);

                            tableBody.Append(tr3);
                        }

                        // Append the table to the document.
                        body.AppendChild(tableBody);
                        #endregion

                        #region Footer
                        Table tablefooter = new Table();
                        // Create a TableProperties object and specify its border information.
                        TableProperties tblFooterProp = new TableProperties(
                            new TableBorders(
                                new TopBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.None),
                                    Size = 6
                                },
                                new BottomBorder()
                                {
                                    Val =
                                    new EnumValue<BorderValues>(BorderValues.None),
                                    Size = 6
                                }
                            )
                        );

                        Paragraph paragraphf = body.AppendChild(new Paragraph());
                        Run runf = paragraphf.AppendChild(new Run(
                            new Text("")
                        ));

                        Paragraph paragraphf1 = body.AppendChild(new Paragraph());
                        Run runf1 = paragraphf1.AppendChild(new Run(
                            new Text("")
                        ));


                        // Append the TableProperties object to the empty table.
                        tablefooter.AppendChild<TableProperties>(tblFooterProp);

                        // Create a row.
                        TableRow trf1 = new TableRow();
                        TableCell tcf11 = new TableCell();
                        tcf11.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "6400" }));
                        tcf11.Append(new Paragraph(new Run(new Text("Заместитель генерального директора"))));
                        trf1.Append(tcf11);

                        TableCell tcf12 = new TableCell();
                        tcf12.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tcf12.Append(new Paragraph(new Run(new Text("О.Н. Селиванов"))));
                        trf1.Append(tcf12);
                        tablefooter.Append(trf1);

                        TableRow trf2 = new TableRow();
                        TableCell tcf21 = new TableCell();
                        tcf21.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "6400" }));
                        tcf21.Append(new Paragraph(new Run(new Text("СООО «БЕЛАМАРКЕТ дьюти фри»"))));
                        trf2.Append(tcf21);
                        tablefooter.Append(trf2);

                        body.AppendChild(tablefooter);
                        #endregion

                        #region NextPage
                        Paragraph PageBreakParagraph = new Paragraph(
                            new Run(
                                new Break() { Type = BreakValues.Page }));

                        body.AppendChild(PageBreakParagraph);
                        #endregion
                    }

                }

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
            
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> GenerateDocuments(List<Petition> petitions)
        {
            var petition = await _context.Petitions
                            .Include(a => a.Points)
                            .Include(b => b.Units)
                            .FirstOrDefaultAsync();

            string strtmp = "";
            List<string> arr_point = new List<string>();

            foreach (var point in petition.Points)
            {
                arr_point.Add(point.Title);
            }

            var points = String.Join(", ", arr_point);

            using (MemoryStream stream = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument =
                    WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    RunProperties rPr1 = new RunProperties(new FontSize() { Val = "28" });
                    RunProperties rPr2 = new RunProperties(new FontSize() { Val = "28" });


                    Table tableHeader = new Table();
                    // Create a TableProperties object and specify its border information.
                    TableProperties tblHeaderProp = new TableProperties(
                        new TableBorders(
                            new TopBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.None),
                                Size = 6
                            },
                            new BottomBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.None),
                                Size = 6
                            }
                        )
                    );

                    tableHeader.AppendChild<TableProperties>(tblHeaderProp);

                    TableRow trh1 = new TableRow();
                    TableCell tch11 = new TableCell();
                    tch11.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "5400" }));
                    tch11.Append(new Paragraph(new Run(new Text("от _________ исх. № _______"))));
                    trh1.Append(tch11);
                    TableCell tch12 = new TableCell();
                    tch12.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3400" }));
                    tch12.Append(new Paragraph(new Run(new Text("Командиру войсковой части 1234"))));
                    trh1.Append(tch12);
                    tableHeader.Append(trh1);

                    TableRow trh2 = new TableRow();
                    TableCell tch21 = new TableCell();
                    tch21.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "5400" }));
                    tch21.Append(new Paragraph(new Run(new Text("на № _________ от _______"))));
                    trh2.Append(tch21);
                    TableCell tch22 = new TableCell();
                    tch22.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3400" }));
                    tch22.Append(new Paragraph(new Run(new Text("полковнику Давидюку А.Б."))));
                    trh2.Append(tch22);
                    tableHeader.Append(trh2);

                    body.AddChild(tableHeader);


                    Paragraph paragraphP1 = body.AppendChild(new Paragraph());
                    Run runP1 = paragraphP1.AppendChild(new Run(new Text("")));

                    Paragraph paragraphP1T = body.AppendChild(new Paragraph());
                    Run runP1T = paragraphP1T.AppendChild(new Run(new Text("Ходатайство")));

                    Paragraph paragraphP2 = body.AppendChild(new Paragraph());
                    Run runP2 = paragraphP2.AppendChild(new Run(new Text("")));

                    if (petition.Units.Count > 1 && petition.Points.Count > 1)
                    {
                        strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пунктах пропуска «{ points }» через Государственную границу Республики Беларусь должностным лицам, указанным в прилагаемом списке.";
                    }
                    else if (petition.Units.Count > 1 && petition.Points.Count == 1)
                    {
                        strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пункте пропуска «{ points }» через Государственную границу Республики Беларусь должностным лицам, указанным в прилагаемом списке.";
                    }
                    else if (petition.Units.Count == 1 && petition.Points.Count > 1)
                    {
                        strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пунктах пропуска «{ points }» через Государственную границу Республики Беларусь должностному лицу, указанному в прилагаемом списке.";
                    }
                    else if (petition.Units.Count == 1 && petition.Points.Count == 1)
                    {
                        strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пункте пропуска «{ points }» через Государственную границу Республики Беларусь должностному лицу, указанному в прилагаемом списке.";
                    }

                    Paragraph paragraph1 = body.AppendChild(new Paragraph());
                    Run run1 = paragraph1.AppendChild(new Run(
                    new Text(strtmp)));
                    //run1.PrependChild<RunProperties>(rPr1);

                    if (petition.Points.Count() > 1 && petition.Vehicle.Length != 0)
                    {
                        strtmp = $"Потребность нахождения в пунктах { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри» на автомобиле {petition.Vehicle}.";
                    }
                    else if (petition.Points.Count() == 1 && petition.Vehicle.Length != 0)
                    {
                        strtmp = $"Потребность нахождения в пункте { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри» на автомобиле {petition.Vehicle}.";
                    }
                    else if (petition.Points.Count() > 1 && petition.Vehicle.Length == 0)
                    {
                        strtmp = $"Потребность нахождения в пунктах { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри».";
                    }
                    else if (petition.Points.Count() == 1 && petition.Vehicle.Length == 0)
                    {
                        strtmp = $"Потребность нахождения в пункте { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри».";
                    }

                    Paragraph paragraph2 = body.AppendChild(new Paragraph());
                    Run run2 = paragraph2.AppendChild(new Run(
                    new Text(strtmp)));
                    //run2.PrependChild<RunProperties>(rPr2);

                    Table tableBody = new Table();
                    // Create a TableProperties object and specify its border information.
                    TableProperties tblBodyProp = new TableProperties(
                        new TableBorders(
                            new TopBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 6
                            },
                            new BottomBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 6
                            },
                            new LeftBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 6
                            },
                            new RightBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 6
                            },
                            new InsideHorizontalBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 6
                            },
                            new InsideVerticalBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 6
                            }
                        )
                    );

                    // Append the TableProperties object to the empty table.
                    tableBody.AppendChild<TableProperties>(tblBodyProp);

                    // Create a row.
                    TableRow tr1 = new TableRow();

                    TableCell tc1 = new TableCell();
                    tc1.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc1.Append(new Paragraph(new Run(new Text("п/п"))));
                    tr1.Append(tc1);

                    TableCell tc2 = new TableCell();
                    tc2.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc2.Append(new Paragraph(new Run(new Text("Пункт пропуска"))));
                    tr1.Append(tc2);

                    TableCell tc3 = new TableCell();
                    tc3.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc3.Append(new Paragraph(new Run(new Text("Фамилия, собственное имя, отчество (если таковое имеется)"))));
                    tr1.Append(tc3);

                    TableCell tc4 = new TableCell();
                    tc4.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc4.Append(new Paragraph(new Run(new Text("Дата и место рождения"))));
                    tr1.Append(tc4);

                    TableCell tc5 = new TableCell();
                    tc5.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc5.Append(new Paragraph(new Run(new Text("Данные документа, удостоверяющего его личность (серия (при наличии), номер, кем и когда выдан, личный номер)"))));
                    tr1.Append(tc5);

                    TableCell tc6 = new TableCell();
                    tc6.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc6.Append(new Paragraph(new Run(new Text("Должность"))));
                    tr1.Append(tc6);

                    TableCell tc7 = new TableCell();
                    tc7.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc7.Append(new Paragraph(new Run(new Text("Домашний адрес"))));
                    tr1.Append(tc7);

                    tableBody.Append(tr1);

                    // Create a row.
                    TableRow tr2 = new TableRow();

                    TableCell tc11 = new TableCell();
                    tc11.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc11.Append(new Paragraph(new Run(new Text("1"))));
                    tr2.Append(tc11);

                    TableCell tc12 = new TableCell();
                    tc12.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc12.Append(new Paragraph(new Run(new Text("2"))));
                    tr2.Append(tc12);

                    TableCell tc13 = new TableCell();
                    tc13.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc13.Append(new Paragraph(new Run(new Text("3"))));
                    tr2.Append(tc13);

                    TableCell tc14 = new TableCell();
                    tc14.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc14.Append(new Paragraph(new Run(new Text("4"))));
                    tr2.Append(tc14);

                    TableCell tc15 = new TableCell();
                    tc15.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc15.Append(new Paragraph(new Run(new Text("5"))));
                    tr2.Append(tc15);

                    TableCell tc16 = new TableCell();
                    tc16.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc16.Append(new Paragraph(new Run(new Text("6"))));
                    tr2.Append(tc16);

                    TableCell tc17 = new TableCell();
                    tc17.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tc17.Append(new Paragraph(new Run(new Text("7"))));
                    tr2.Append(tc17);

                    tableBody.Append(tr2);


                    foreach (var unit in petition.Units)
                    {
                        TableRow tr3 = new TableRow();

                        TableCell tc21 = new TableCell();
                        tc21.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc21.Append(new Paragraph(new Run(new Text(""))));
                        tr3.Append(tc21);

                        TableCell tc22 = new TableCell();
                        tc22.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc22.Append(new Paragraph(new Run(new Text(points))));
                        tr3.Append(tc22);

                        TableCell tc23 = new TableCell();
                        tc23.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc23.Append(new Paragraph(new Run(new Text(unit.FullName))));
                        tr3.Append(tc23);

                        TableCell tc24 = new TableCell();
                        tc24.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc24.Append(new Paragraph(new Run(new Text(unit.BirthDay.ToShortDateString() + unit.BirthPlace))));
                        tr3.Append(tc24);

                        TableCell tc25 = new TableCell();
                        tc25.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc25.Append(new Paragraph(new Run(new Text(unit.DocumentIdentity))));
                        tr3.Append(tc25);

                        TableCell tc26 = new TableCell();
                        tc26.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc26.Append(new Paragraph(new Run(new Text(unit.Position))));
                        tr3.Append(tc26);

                        TableCell tc27 = new TableCell();
                        tc27.Append(new TableCellProperties(
                            new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                        tc27.Append(new Paragraph(new Run(new Text(unit.HomeAdress))));
                        tr3.Append(tc27);

                        tableBody.Append(tr3);
                    }

                    // Append the table to the document.
                    body.Append(tableBody);

                    Table tablefooter = new Table();
                    // Create a TableProperties object and specify its border information.
                    TableProperties tblFooterProp = new TableProperties(
                        new TableBorders(
                            new TopBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.None),
                                Size = 6
                            },
                            new BottomBorder()
                            {
                                Val =
                                new EnumValue<BorderValues>(BorderValues.None),
                                Size = 6
                            }
                        )
                    );

                    Paragraph paragraphf = body.AppendChild(new Paragraph());
                    Run runf = paragraphf.AppendChild(new Run(
                        new Text("")
                    ));

                    Paragraph paragraphf1 = body.AppendChild(new Paragraph());
                    Run runf1 = paragraphf1.AppendChild(new Run(
                        new Text("")
                    ));


                    // Append the TableProperties object to the empty table.
                    tablefooter.AppendChild<TableProperties>(tblFooterProp);

                    // Create a row.
                    TableRow trf1 = new TableRow();
                    TableCell tcf11 = new TableCell();
                    tcf11.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "6400" }));
                    tcf11.Append(new Paragraph(new Run(new Text("Заместитель генерального директора"))));
                    trf1.Append(tcf11);

                    TableCell tcf12 = new TableCell();
                    tcf12.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
                    tcf12.Append(new Paragraph(new Run(new Text("О.Н. Селиванов"))));
                    trf1.Append(tcf12);
                    tablefooter.Append(trf1);

                    TableRow trf2 = new TableRow();
                    TableCell tcf21 = new TableCell();
                    tcf21.Append(new TableCellProperties(
                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "6400" }));
                    tcf21.Append(new Paragraph(new Run(new Text("СООО «БЕЛАМАРКЕТ дьюти фри»"))));
                    trf2.Append(tcf21);
                    tablefooter.Append(trf2);

                    body.Append(tablefooter);


                    ////MainDocumentPart mainPart2 = wordDocument.AddMainDocumentPart();
                    //mainPart.Document = new Document();
                    //Body body2 = mainPart.Document.AppendChild(new Body());

                    //Paragraph paragraphG = body2.AppendChild(new Paragraph());
                    //Run runG = paragraphG.AppendChild(new Run(
                    //new Text("123123123")));
                }

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            }
        }

        //public Doc DoFile(Petition petition)
        //{
        //    //List<Point> _oops = pointsGroupe.Distinct();
        //    //RunProperties rPr1 = new RunProperties(new FontSize() { Val = "28" });
        //    //RunProperties rPr2 = new RunProperties(new FontSize() { Val = "28" });

        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        using (WordprocessingDocument wordDocument =
        //            WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        //        {
        //            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
        //            mainPart.Document = new Document();
        //            Body body = mainPart.Document.AppendChild(new Body());

        //            //разделить документ на поддокументы 

        //            foreach (var point in pointsGroupe)
        //            {
        //                string strtmp = "";

        //                var pointChief = point.First().PointChief;
                        
        //                List<string> arr_point = new List<string>();

        //                foreach (var _point in point.AsEnumerable())
        //                {
        //                    arr_point.Add(_point.Title);
        //                }

        //                var points = String.Join(", ", arr_point);

        //                #region Header
        //                Table tableHeader = new Table();
        //                // Create a TableProperties object and specify its border information.
        //                TableProperties tblHeaderProp = new TableProperties(
        //                    new TableBorders(
        //                        new TopBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.None),
        //                            Size = 6
        //                        },
        //                        new BottomBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.None),
        //                            Size = 6
        //                        }
        //                    )
        //                );

        //                tableHeader.AppendChild<TableProperties>(tblHeaderProp);

        //                TableRow trh1 = new TableRow();
        //                TableCell tch11 = new TableCell();
        //                tch11.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "5400" }));
        //                tch11.Append(new Paragraph(new Run(new Text("от _________ исх. № _______"))));
        //                trh1.Append(tch11);
        //                TableCell tch12 = new TableCell();
        //                tch12.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3400" }));
        //                tch12.Append(new Paragraph(new Run(new Text($"Командиру войсковой части { pointChief.Title }"))));
        //                trh1.Append(tch12);
        //                tableHeader.Append(trh1);

        //                TableRow trh2 = new TableRow();
        //                TableCell tch21 = new TableCell();
        //                tch21.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "5400" }));
        //                tch21.Append(new Paragraph(new Run(new Text("на № _________ от _______"))));
        //                trh2.Append(tch21);
        //                TableCell tch22 = new TableCell();
        //                tch22.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "3400" }));
        //                tch22.Append(new Paragraph(new Run(new Text($"полковнику { pointChief.ChiefFullName }"))));
        //                trh2.Append(tch22);
        //                tableHeader.Append(trh2);

        //                body.AddChild(tableHeader);
        //                #endregion

        //                #region Body
        //                Paragraph paragraphP1 = body.AppendChild(new Paragraph());
        //                Run runP1 = paragraphP1.AppendChild(new Run(new Text("")));

        //                Paragraph paragraphP1T = body.AppendChild(new Paragraph());
        //                Run runP1T = paragraphP1T.AppendChild(new Run(new Text("Ходатайство")));

        //                Paragraph paragraphP2 = body.AppendChild(new Paragraph());
        //                Run runP2 = paragraphP2.AppendChild(new Run(new Text("")));

        //                if (petition.Units.Count > 1 && petition.Points.Count > 1)
        //                {
        //                    strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пунктах пропуска «{ points }» через Государственную границу Республики Беларусь должностным лицам, указанным в прилагаемом списке.";
        //                }
        //                else if (petition.Units.Count > 1 && petition.Points.Count == 1)
        //                {
        //                    strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пункте пропуска «{ points }» через Государственную границу Республики Беларусь должностным лицам, указанным в прилагаемом списке.";
        //                }
        //                else if (petition.Units.Count == 1 && petition.Points.Count > 1)
        //                {
        //                    strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пунктах пропуска «{ points }» через Государственную границу Республики Беларусь должностному лицу, указанному в прилагаемом списке.";
        //                }
        //                else if (petition.Units.Count == 1 && petition.Points.Count == 1)
        //                {
        //                    strtmp = $"СООО «БЕЛАМАРКЕТ дьюти фри» просит Вас разрешить нахождение c { petition.DateFrom.ToShortDateString() } года по { petition.DateTo.ToShortDateString() } года в пункте пропуска «{ points }» через Государственную границу Республики Беларусь должностному лицу, указанному в прилагаемом списке.";
        //                }

        //                Paragraph paragraph1 = body.AppendChild(new Paragraph());
        //                Run run1 = paragraph1.AppendChild(new Run(
        //                new Text(strtmp)));
        //                //run1.PrependChild<RunProperties>(rPr1);

        //                if (petition.Points.Count() > 1 && petition.Vehicle.Length != 0)
        //                {
        //                    strtmp = $"Потребность нахождения в пунктах { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри» на автомобиле {petition.Vehicle}.";
        //                }
        //                else if (petition.Points.Count() == 1 && petition.Vehicle.Length != 0)
        //                {
        //                    strtmp = $"Потребность нахождения в пункте { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри» на автомобиле {petition.Vehicle}.";
        //                }
        //                else if (petition.Points.Count() > 1 && petition.Vehicle.Length == 0)
        //                {
        //                    strtmp = $"Потребность нахождения в пунктах { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри».";
        //                }
        //                else if (petition.Points.Count() == 1 && petition.Vehicle.Length == 0)
        //                {
        //                    strtmp = $"Потребность нахождения в пункте { petition.Reason.ToLower() } в магазинах беспошлинной торговли СООО «БЕЛАМАРКЕТ дьюти фри».";
        //                }

        //                Paragraph paragraph2 = body.AppendChild(new Paragraph());
        //                Run run2 = paragraph2.AppendChild(new Run(
        //                new Text(strtmp)));
        //                //run2.PrependChild<RunProperties>(rPr2);

        //                Table tableBody = new Table();
        //                // Create a TableProperties object and specify its border information.
        //                TableProperties tblBodyProp = new TableProperties(
        //                    new TableBorders(
        //                        new TopBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.Single),
        //                            Size = 6
        //                        },
        //                        new BottomBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.Single),
        //                            Size = 6
        //                        },
        //                        new LeftBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.Single),
        //                            Size = 6
        //                        },
        //                        new RightBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.Single),
        //                            Size = 6
        //                        },
        //                        new InsideHorizontalBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.Single),
        //                            Size = 6
        //                        },
        //                        new InsideVerticalBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.Single),
        //                            Size = 6
        //                        }
        //                    )
        //                );

        //                // Append the TableProperties object to the empty table.
        //                tableBody.AppendChild<TableProperties>(tblBodyProp);

        //                // Create a row.
        //                TableRow tr1 = new TableRow();

        //                TableCell tc1 = new TableCell();
        //                tc1.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc1.Append(new Paragraph(new Run(new Text("п/п"))));
        //                tr1.Append(tc1);

        //                TableCell tc2 = new TableCell();
        //                tc2.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc2.Append(new Paragraph(new Run(new Text("Пункт пропуска"))));
        //                tr1.Append(tc2);

        //                TableCell tc3 = new TableCell();
        //                tc3.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc3.Append(new Paragraph(new Run(new Text("Фамилия, собственное имя, отчество (если таковое имеется)"))));
        //                tr1.Append(tc3);

        //                TableCell tc4 = new TableCell();
        //                tc4.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc4.Append(new Paragraph(new Run(new Text("Дата и место рождения"))));
        //                tr1.Append(tc4);

        //                TableCell tc5 = new TableCell();
        //                tc5.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc5.Append(new Paragraph(new Run(new Text("Данные документа, удостоверяющего его личность (серия (при наличии), номер, кем и когда выдан, личный номер)"))));
        //                tr1.Append(tc5);

        //                TableCell tc6 = new TableCell();
        //                tc6.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc6.Append(new Paragraph(new Run(new Text("Должность"))));
        //                tr1.Append(tc6);

        //                TableCell tc7 = new TableCell();
        //                tc7.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc7.Append(new Paragraph(new Run(new Text("Домашний адрес"))));
        //                tr1.Append(tc7);

        //                tableBody.Append(tr1);

        //                // Create a row.
        //                TableRow tr2 = new TableRow();

        //                TableCell tc11 = new TableCell();
        //                tc11.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc11.Append(new Paragraph(new Run(new Text("1"))));
        //                tr2.Append(tc11);

        //                TableCell tc12 = new TableCell();
        //                tc12.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc12.Append(new Paragraph(new Run(new Text("2"))));
        //                tr2.Append(tc12);

        //                TableCell tc13 = new TableCell();
        //                tc13.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc13.Append(new Paragraph(new Run(new Text("3"))));
        //                tr2.Append(tc13);

        //                TableCell tc14 = new TableCell();
        //                tc14.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc14.Append(new Paragraph(new Run(new Text("4"))));
        //                tr2.Append(tc14);

        //                TableCell tc15 = new TableCell();
        //                tc15.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc15.Append(new Paragraph(new Run(new Text("5"))));
        //                tr2.Append(tc15);

        //                TableCell tc16 = new TableCell();
        //                tc16.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc16.Append(new Paragraph(new Run(new Text("6"))));
        //                tr2.Append(tc16);

        //                TableCell tc17 = new TableCell();
        //                tc17.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tc17.Append(new Paragraph(new Run(new Text("7"))));
        //                tr2.Append(tc17);

        //                tableBody.Append(tr2);


        //                foreach (var unit in petition.Units)
        //                {
        //                    TableRow tr3 = new TableRow();

        //                    TableCell tc21 = new TableCell();
        //                    tc21.Append(new TableCellProperties(
        //                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                    tc21.Append(new Paragraph(new Run(new Text(""))));
        //                    tr3.Append(tc21);

        //                    TableCell tc22 = new TableCell();
        //                    tc22.Append(new TableCellProperties(
        //                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                    tc22.Append(new Paragraph(new Run(new Text(points))));
        //                    tr3.Append(tc22);

        //                    TableCell tc23 = new TableCell();
        //                    tc23.Append(new TableCellProperties(
        //                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                    tc23.Append(new Paragraph(new Run(new Text(unit.FullName))));
        //                    tr3.Append(tc23);

        //                    TableCell tc24 = new TableCell();
        //                    tc24.Append(new TableCellProperties(
        //                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                    tc24.Append(new Paragraph(new Run(new Text(unit.BirthDay.ToShortDateString() + unit.BirthPlace))));
        //                    tr3.Append(tc24);

        //                    TableCell tc25 = new TableCell();
        //                    tc25.Append(new TableCellProperties(
        //                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                    tc25.Append(new Paragraph(new Run(new Text(unit.DocumentIdentity))));
        //                    tr3.Append(tc25);

        //                    TableCell tc26 = new TableCell();
        //                    tc26.Append(new TableCellProperties(
        //                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                    tc26.Append(new Paragraph(new Run(new Text(unit.Position))));
        //                    tr3.Append(tc26);

        //                    TableCell tc27 = new TableCell();
        //                    tc27.Append(new TableCellProperties(
        //                        new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                    tc27.Append(new Paragraph(new Run(new Text(unit.HomeAdress))));
        //                    tr3.Append(tc27);

        //                    tableBody.Append(tr3);
        //                }

        //                // Append the table to the document.
        //                body.Append(tableBody);
        //                #endregion

        //                #region Footer
        //                Table tablefooter = new Table();
        //                // Create a TableProperties object and specify its border information.
        //                TableProperties tblFooterProp = new TableProperties(
        //                    new TableBorders(
        //                        new TopBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.None),
        //                            Size = 6
        //                        },
        //                        new BottomBorder()
        //                        {
        //                            Val =
        //                            new EnumValue<BorderValues>(BorderValues.None),
        //                            Size = 6
        //                        }
        //                    )
        //                );

        //                Paragraph paragraphf = body.AppendChild(new Paragraph());
        //                Run runf = paragraphf.AppendChild(new Run(
        //                    new Text("")
        //                ));

        //                Paragraph paragraphf1 = body.AppendChild(new Paragraph());
        //                Run runf1 = paragraphf1.AppendChild(new Run(
        //                    new Text("")
        //                ));


        //                // Append the TableProperties object to the empty table.
        //                tablefooter.AppendChild<TableProperties>(tblFooterProp);

        //                // Create a row.
        //                TableRow trf1 = new TableRow();
        //                TableCell tcf11 = new TableCell();
        //                tcf11.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "6400" }));
        //                tcf11.Append(new Paragraph(new Run(new Text("Заместитель генерального директора"))));
        //                trf1.Append(tcf11);

        //                TableCell tcf12 = new TableCell();
        //                tcf12.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));
        //                tcf12.Append(new Paragraph(new Run(new Text("О.Н. Селиванов"))));
        //                trf1.Append(tcf12);
        //                tablefooter.Append(trf1);

        //                TableRow trf2 = new TableRow();
        //                TableCell tcf21 = new TableCell();
        //                tcf21.Append(new TableCellProperties(
        //                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "6400" }));
        //                tcf21.Append(new Paragraph(new Run(new Text("СООО «БЕЛАМАРКЕТ дьюти фри»"))));
        //                trf2.Append(tcf21);
        //                tablefooter.Append(trf2);

        //                body.Append(tablefooter);
        //                #endregion

        //            }
        //        }

        //        return new Doc
        //        {
        //            FileContent = stream.ToArray(),
        //            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        //        };
        //    }
        //}
    }

    public class GeneralResponse
    {
        public List<Point> Points { get; set; }
        public List<PetitionType> PetitionTypes { get; set; }
    }

    public class Doc
    {
        public string ContentType { get; set; }
        public byte[] FileContent { get; set; }
    }
}


//public class DocsResponse
//{
//    public List<Doc> Docs { get; set; }
//}



