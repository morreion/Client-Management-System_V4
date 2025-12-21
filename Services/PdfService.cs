using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Client_Management_System_V4.Models;
using Client_Management_System_V4.Repositories;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Client_Management_System_V4.Services
{
    public class PdfService : IPdfService
    {
        private readonly MedHxRepository _medHxRepo = new();
        private readonly AnthropometricsRepository _anthroRepo = new();
        private readonly DietRepository _dietRepo = new();
        private readonly TreatmentRepository _treatmentRepo = new();
        private readonly EyeAnalysisRepository _eyeRepo = new();
        private readonly PrescriptionRepository _prescriptionRepo = new();
        private readonly BodySystemsOverviewRepository _bodySystemsRepo = new();

        public PdfService()
        {
            Settings.License = LicenseType.Community;
        }

        public async Task GenerateHealthReportAsync(Client client, ReportOptions options, string filePath)
        {
            // Fetch Data based on Options
            IEnumerable<MedHx> medHxList = options.IncludeMedicalHistory ? await _medHxRepo.GetByClientIdAsync(client.ClientID) : Enumerable.Empty<MedHx>();
            IEnumerable<Anthropometrics> anthroList = options.IncludeAnthropometrics ? await _anthroRepo.GetByClientIdAsync(client.ClientID) : Enumerable.Empty<Anthropometrics>();
            IEnumerable<Diet> dietList = options.IncludeDiet ? await _dietRepo.GetByClientIdAsync(client.ClientID) : Enumerable.Empty<Diet>();
            IEnumerable<Treatment> treatmentList = options.IncludeTreatment ? await _treatmentRepo.GetByClientIdAsync(client.ClientID) : Enumerable.Empty<Treatment>();
            IEnumerable<EyeAnalysis> eyeList = options.IncludeEyeAnalysis ? await _eyeRepo.GetByClientIdAsync(client.ClientID) : Enumerable.Empty<EyeAnalysis>();
            IEnumerable<BodySystemsOverview> bodySystemsList = options.IncludeBodySystems ? await _bodySystemsRepo.GetByClientIdAsync(client.ClientID) : Enumerable.Empty<BodySystemsOverview>();
            
            // Prescription Preparation
            var prescriptionDetails = new List<(Prescription Rx, IEnumerable<PrescriptionSupplement> Supplements)>();
            if (options.IncludePrescriptions)
            {
                var prescriptions = await _prescriptionRepo.GetByClientIdAsync(client.ClientID);
                foreach(var rx in prescriptions)
                {
                    if (rx.PrescriptionID.HasValue)
                    {
                        var sups = await _prescriptionRepo.GetSupplementsByPrescriptionIdAsync(rx.PrescriptionID.Value);
                        prescriptionDetails.Add((rx, sups));
                    }
                }
            }

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header().Element(ComposeHeader);
                        
                        page.Content().Element(content => ComposeContent(content, client, options, medHxList, anthroList, dietList, treatmentList, eyeList, bodySystemsList, prescriptionDetails));

                        page.Footer().Element(ComposeFooter);
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public async Task GeneratePrescriptionReportAsync(Client client, string filePath)
        {
            var prescriptions = await _prescriptionRepo.GetByClientIdAsync(client.ClientID);
            
            // We need to fetch supplements for each prescription to display them details
            // This might be N+1 query problem but for a single client report it's negligible
            var prescriptionDetails = new List<(Prescription Rx, IEnumerable<PrescriptionSupplement> Supplements)>();
            
            foreach(var rx in prescriptions)
            {
                if (rx.PrescriptionID.HasValue)
                {
                    var sups = await _prescriptionRepo.GetSupplementsByPrescriptionIdAsync(rx.PrescriptionID.Value);
                    prescriptionDetails.Add((rx, sups));
                }
            }

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header().Element(head => 
                        {
                            head.Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("Prescription Report").FontSize(20).SemiBold().FontColor(Colors.Green.Medium);
                                    column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(10);
                                });
                            });
                        });

                        page.Content().PaddingVertical(10).Column(column =>
                        {
                            column.Item().Element(c => ComposePatientDetails(c, client));
                            column.Item().Element(c => ComposePrescriptionDetails(c, prescriptionDetails));
                        });

                        page.Footer().Element(ComposeFooter);
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public async Task GenerateContactListReportAsync(IEnumerable<Client> clients, List<string> selectedColumns, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4); // or PageSizes.A4.Landscape() if many columns
                        if (selectedColumns.Count > 4) page.Size(PageSizes.A4.Landscape());
                        
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                        page.Header().Text("Client Contact List").FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                // Dynamic column definition
                                foreach(var col in selectedColumns)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            table.Header(header =>
                            {
                                foreach (var col in selectedColumns)
                                {
                                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Padding(5).Text(col).Bold();
                                }
                            });

                            foreach (var client in clients)
                            {
                                foreach (var col in selectedColumns)
                                {
                                    string value = col switch
                                    {
                                        "Name" => client.Name,
                                        "Mobile" => client.Mobile ?? "-",
                                        "Email" => client.Email ?? "-",
                                        "Address" => client.Address ?? "-",
                                        "DOB" => client.DOB?.ToString("yyyy-MM-dd") ?? "-",
                                        "Occupation" => client.Occupation ?? "-",
                                        "Gender" => client.GenderDisplay,
                                        "Ref" => client.Ref ?? "-",
                                        _ => "-"
                                    };
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(value);
                                }
                            }
                        });

                        page.Footer().Element(ComposeFooter);
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public async Task GenerateSupplementsReportAsync(IEnumerable<Supplement> supplements, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape()); // Landscape for more data
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("Supplements Inventory").FontSize(20).SemiBold().FontColor(Colors.Green.Medium);
                                column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Name
                                columns.RelativeColumn(2); // Type
                                columns.RelativeColumn(2); // Distributor
                                columns.RelativeColumn(3); // Description
                                columns.RelativeColumn(3); // Usage
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).Padding(5).Text("Name").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Type").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Distributor").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Description").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Usage").Bold();
                            });

                            foreach (var item in supplements)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Name ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Type ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.DistributorName ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Description ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Usage ?? "-");
                            }
                        });

                        page.Footer().Element(ComposeFooter);
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public async Task GenerateDistributorReportAsync(IEnumerable<Distributor> distributors, string filePath)
        {
             await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("Distributor List").FontSize(20).SemiBold().FontColor(Colors.Orange.Medium);
                                column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Name
                                columns.RelativeColumn(2); // Email
                                columns.RelativeColumn(2); // Mobile
                                columns.RelativeColumn(2); // Work Phone
                                columns.RelativeColumn(2); // Website
                                columns.RelativeColumn(3); // Address
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).Padding(5).Text("Name").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Email").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Mobile").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Work Phone").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Website").Bold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Address").Bold();
                            });

                            foreach (var item in distributors)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Name ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Email ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Mobile ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Work_Phone ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Website ?? "-");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Address ?? "-");
                            }
                        });

                        page.Footer().Element(ComposeFooter);
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Client Health Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    column.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd}").FontSize(10);
                });
            });
        }

        private void ComposeContent(IContainer container, Client client, ReportOptions options,
            IEnumerable<MedHx> medHxList, IEnumerable<Anthropometrics> anthroList, 
            IEnumerable<Diet> dietList, IEnumerable<Treatment> treatmentList, 
            IEnumerable<EyeAnalysis> eyeList,
            IEnumerable<BodySystemsOverview> bodySystemsList,
            List<(Prescription Rx, IEnumerable<PrescriptionSupplement> Supplements)> prescriptionDetails)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // Patient Details
                column.Item().Element(c => ComposePatientDetails(c, client));
                
                if (options.IncludeMedicalHistory)
                    column.Item().Element(c => ComposeMedicalHistory(c, medHxList));
                
                if (options.IncludeAnthropometrics)
                    column.Item().Element(c => ComposeAnthropometrics(c, anthroList));
                
                if (options.IncludeDiet)
                    column.Item().Element(c => ComposeDiet(c, dietList));

                if (options.IncludeTreatment)
                    column.Item().Element(c => ComposeTreatment(c, treatmentList));

                if (options.IncludeEyeAnalysis)
                    column.Item().Element(c => ComposeEyeAnalysis(c, eyeList));

                if (options.IncludeBodySystems)
                    column.Item().Element(c => ComposeBodySystems(c, bodySystemsList));

                if (options.IncludePrescriptions)
                    column.Item().Element(c => ComposePrescriptionDetails(c, prescriptionDetails));
            });
        }

        private void ComposePatientDetails(IContainer container, Client client)
        {
            container.ShowEntire().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column => 
            {
                column.Item().Text("Patient Details").FontSize(14).SemiBold().Underline();
                
                column.Item().Row(row => 
                {
                    row.RelativeItem().Text($"Name: {client.Name}");
                    row.RelativeItem().Text($"DOB: {client.DOB:yyyy-MM-dd} (Age: {client.Age})");
                });

                column.Item().Row(row => 
                {
                    row.RelativeItem().Text($"Gender: {client.GenderDisplay}");
                    row.RelativeItem().Text($"Mobile: {client.Mobile}");
                });
                
                 column.Item().Row(row => 
                {
                    row.RelativeItem().Text($"Email: {client.Email}");
                    row.RelativeItem().Text($"Occupation: {client.Occupation}");
                });
            });
        }

        private void ComposeMedicalHistory(IContainer container, IEnumerable<MedHx> list)
        {
            container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("Medical History").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                
                if (!list.Any())
                {
                    column.Item().Text("No records found.");
                    return;
                }

                foreach (var item in list)
                {
                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).Column(c =>
                    {
                        c.Item().Text($"Date: {item.Assessment_Date:yyyy-MM-dd}").Bold();
                        c.Item().Text($"Notes: {item.HistoryNotes}");
                        if (!string.IsNullOrEmpty(item.Accidents_Previous_Illness)) c.Item().Text($"Accidents/Previous Illness: {item.Accidents_Previous_Illness}");
                        if (!string.IsNullOrEmpty(item.Blood_Test_Results)) c.Item().Text($"Blood Test Results: {item.Blood_Test_Results}");
                        if (!string.IsNullOrEmpty(item.Menstrual_Notes)) c.Item().Text($"Menstrual Notes: {item.Menstrual_Notes}");
                        if (!string.IsNullOrEmpty(item.Vaccinations)) c.Item().Text($"Vaccinations: {item.Vaccinations}");
                        if (!string.IsNullOrEmpty(item.Family_Med_Hx)) c.Item().Text($"Family Medical History: {item.Family_Med_Hx}");
                        if (!string.IsNullOrEmpty(item.Medication)) c.Item().Text($"Medication: {item.Medication}");
                        if (!string.IsNullOrEmpty(item.Supplements)) c.Item().Text($"Supplements: {item.Supplements}");
                    });
                }
            });
        }

        private void ComposeAnthropometrics(IContainer container, IEnumerable<Anthropometrics> list)
        {
             container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("Anthropometrics").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                
                if (!list.Any())
                {
                    column.Item().Text("No records found.");
                    return;
                }

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70); // Date
                        columns.RelativeColumn(); // Weight
                        columns.RelativeColumn(); // Height
                        columns.RelativeColumn(); // BMI
                        columns.RelativeColumn(); // BP
                        columns.RelativeColumn(); // Pulse
                        columns.RelativeColumn(); // SpO2
                        columns.RelativeColumn(); // Temp
                        columns.RelativeColumn(1.5f); // Status
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Date").Bold();
                        header.Cell().Text("Wt (kg)").Bold();
                        header.Cell().Text("Ht (cm)").Bold();
                        header.Cell().Text("BMI").Bold();
                        header.Cell().Text("BP").Bold();
                        header.Cell().Text("Pulse").Bold();
                        header.Cell().Text("SpO2").Bold();
                        header.Cell().Text("Temp").Bold();
                        header.Cell().Text("Zinc/NOX").Bold();
                    });

                    foreach (var item in list)
                    {
                        table.Cell().Text($"{item.Assessment_Date:yyyy-MM-dd}");
                        table.Cell().Text($"{item.Weight}");
                        table.Cell().Text($"{item.Height}");
                        var bmi = item.BMI.HasValue ? item.BMI.Value.ToString("F1") : "-";
                        table.Cell().Text(bmi);
                        table.Cell().Text(item.BP);
                        table.Cell().Text(item.Pulse.ToString());
                        table.Cell().Text(item.SpO2_Percent?.ToString() ?? "-");
                        table.Cell().Text(item.Temp?.ToString() ?? "-");
                        table.Cell().Column(col => 
                        {
                            if(!string.IsNullOrEmpty(item.Zinc_Status)) col.Item().Text($"Zn: {item.Zinc_Status}").FontSize(9);
                            if(!string.IsNullOrEmpty(item.NOX_Status)) col.Item().Text($"NOX: {item.NOX_Status}").FontSize(9);
                        });
                    }
                });
            });
        }

        private void ComposeDiet(IContainer container, IEnumerable<Diet> list)
        {
             container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("Diet Plan").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                
                if (!list.Any())
                {
                    column.Item().Text("No records found.");
                    return;
                }

                 foreach (var item in list)
                {
                    column.Item().PaddingVertical(5).Column(c =>
                    {
                        c.Item().Text($"Plan Date: {item.Diet_Date:yyyy-MM-dd}").Bold();
                        c.Item().Text("Breakfast:").SemiBold();
                        c.Item().PaddingLeft(10).Text(item.Breakfast);
                        c.Item().Text("Lunch:").SemiBold();
                        c.Item().PaddingLeft(10).Text(item.Lunch);
                        c.Item().Text("Dinner:").SemiBold();
                        c.Item().PaddingLeft(10).Text(item.Dinner);
                        if (!string.IsNullOrEmpty(item.Snacks))
                        {
                            c.Item().Text("Snacks:").SemiBold();
                            c.Item().PaddingLeft(10).Text(item.Snacks);
                        }
                    });
                }
            });
        }

        private void ComposeTreatment(IContainer container, IEnumerable<Treatment> list)
        {
             container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("Treatment Plan").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                if (!list.Any())
                {
                    column.Item().Text("No records found.");
                    return;
                }
                foreach (var item in list)
                {
                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).Column(c =>
                    {
                        c.Item().Text($"Date: {item.Treatment_Date:yyyy-MM-dd}").Bold();
                        if (!string.IsNullOrEmpty(item.Expectations_of_Treatment)) c.Item().Text($"Expectations: {item.Expectations_of_Treatment}");
                        c.Item().Text($"Symptom: {item.Presenting_Symptoms}");
                        c.Item().Text($"Impression: {item.Impression}");
                        c.Item().Text($"Rx: {item.Rx}");
                    });
                }
            });
        }
        
        private void ComposePrescriptionDetails(IContainer container, IEnumerable<(Prescription Rx, IEnumerable<PrescriptionSupplement> Supplements)> list)
        {
             container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("Prescriptions").FontSize(16).SemiBold().FontColor(Colors.Green.Darken2);
                
                if (!list.Any())
                {
                    column.Item().Text("No records found.");
                    return;
                }

                foreach (var item in list)
                {
                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(10).Column(c =>
                    {
                        c.Item().Text($"Date: {item.Rx.Prescription_Date:yyyy-MM-dd}").Bold().FontSize(12);
                        if(item.Rx.Next_Appointment_Date.HasValue)
                             c.Item().Text($"Next Appointment: {item.Rx.Next_Appointment_Date:yyyy-MM-dd}");
                        
                        c.Item().PaddingTop(5).Text("Recommendations:").SemiBold();
                        c.Item().Text(item.Rx.Recommendations);

                        if(item.Supplements.Any())
                        {
                            c.Item().PaddingTop(5).Text("Supplements:").SemiBold();
                            c.Item().Table(table => 
                            {
                                table.ColumnsDefinition(cols => 
                                {
                                    cols.RelativeColumn(3); // Name
                                    cols.RelativeColumn(); // Breakfast
                                    cols.RelativeColumn(); // Lunch
                                    cols.RelativeColumn(); // Dinner
                                    cols.RelativeColumn(); // Bedtime
                                });
                                
                                table.Header(h => 
                                {
                                    h.Cell().Text("Name").Italic();
                                    h.Cell().Text("Brkfst").Italic();
                                    h.Cell().Text("Lunch").Italic();
                                    h.Cell().Text("Dinner").Italic();
                                    h.Cell().Text("Bedtime").Italic();
                                });

                                foreach(var sup in item.Supplements)
                                {
                                    table.Cell().Text(sup.SupplementName);
                                    table.Cell().Text(sup.Breakfast);
                                    table.Cell().Text(sup.Lunch);
                                    table.Cell().Text(sup.Dinner);
                                    table.Cell().Text(sup.Bedtime);
                                }
                            });
                        }
                    });
                }
            });
        }

        private void ComposeEyeAnalysis(IContainer container, IEnumerable<EyeAnalysis> list)
        {
             container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("Eye Analysis").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                if (!list.Any())
                {
                    column.Item().Text("No records found.");
                    return;
                }
                foreach (var item in list)
                {
                     column.Item().PaddingVertical(5).Column(c =>
                    {
                        c.Item().Text($"Analysis Date: {item.Analysis_Date:yyyy-MM-dd}").Bold();
                        c.Item().Text($"Constitution: {item.Iris_Colour} / {item.Texture}");
                        c.Item().Text($"Type: {item.Type} | Pupil: {item.Pupil}");
                        c.Item().Text($"S.I.T: {item.S_I_T} | ANW: {item.ANW} | Nerve Rings: {item.Nerve_Rings}");
                        c.Item().Text($"Tissues: {item.Stomach} (Stomach), {item.Bowel} (Bowel), {item.Organs} (Organs)");
                        c.Item().Text($"Other: Scurf: {item.Scurf} | Radii: {item.Radii} | Psora: {item.Psora}");
                        if (!string.IsNullOrEmpty(item.Nox)) c.Item().Text($"Nox: {item.Nox}");
                        if (!string.IsNullOrEmpty(item.Urine)) c.Item().Text($"Urine: {item.Urine}");
                        if (!string.IsNullOrEmpty(item.Meridian_Scan)) c.Item().Text($"Meridian Scan: {item.Meridian_Scan}");
                    });
                }
            });
        }

        private void ComposeBodySystems(IContainer container, IEnumerable<BodySystemsOverview> list)
        {
            container.PaddingTop(20).Column(column =>
            {
                column.Item().Text("Body Systems Overview").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                if (!list.Any())
                {
                    column.Item().Text("No records found.");
                    return;
                }
                foreach (var item in list)
                {
                    column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(10).Column(c =>
                    {
                        c.Item().Text($"Date: {item.Assessment_Date:yyyy-MM-dd}").Bold();
                        
                        // Using a Grid for better layout of many fields
                        c.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            table.Cell().Column(col => 
                            {
                                col.Item().Text($"Immune: {item.Immune}");
                                col.Item().Text($"Allergy: {item.Allergy}");
                                col.Item().Text($"Sleep: {item.Sleep}");
                                col.Item().Text($"Snore: {item.Snore}");
                                col.Item().Text($"Smoke/Alc: {item.Smoke_Alc}");
                                col.Item().Text($"Exercise: {item.Exercise}");
                                col.Item().Text($"Tongue: {item.Tongue}");
                                col.Item().Text($"Cravings: {item.Cravings}");
                                col.Item().Text($"Beverages: {item.Beverages}");
                                col.Item().Text($"Digestion: {item.Digestion}");
                                col.Item().Text($"Bowels: {item.Bowels}");
                            });

                            table.Cell().Column(col => 
                            {
                                col.Item().Text($"Urination: {item.Urination}");
                                col.Item().Text($"Head: {item.Head}");
                                col.Item().Text($"ENT: {item.ENT}");
                                col.Item().Text($"Skin/Hair: {item.Skin_Hair}");
                                col.Item().Text($"Nails: {item.Nails}");
                                col.Item().Text($"Mind/Emotional: {item.Mind_Emotional}");
                                col.Item().Text($"Thyroid: {item.Thyroid}");
                                col.Item().Text($"Backache: {item.Backache}");
                                col.Item().Text($"Joint Pain: {item.Joint_Pain}");
                            });
                        });
                    });
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
            });
        }
    }
}
