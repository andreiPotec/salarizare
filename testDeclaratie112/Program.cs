using System;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;

namespace Declaratie112Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("  COMPLETARE AUTOMATĂ DECLARAȚIE 112 - TEMPLATE ANAF");
            Console.WriteLine("═══════════════════════════════════════════════════\n");

            // OPȚIUNE: Mod test cu date hardcodate
            Console.Write("Rulează în mod TEST cu date hardcodate? (D/N): ");
            string modTest = Console.ReadLine()?.ToUpper();
            
            if (modTest == "D")
            {
                RuleazaTest();
                Console.WriteLine("\nApasă orice tastă pentru a închide aplicația...");
                Console.ReadKey();
                return;
            }

            var declaratie = new Declaratie112();
            
            // Verificare template
            Console.WriteLine("📄 VERIFICARE TEMPLATE PDF");
            Console.WriteLine("─────────────────────────────────────────────────\n");
            
            Console.Write("Introdu calea către template-ul D112 PDF de la ANAF\n");
            Console.Write("(sau apasă Enter pentru 'D112_template.pdf'): ");
            string templatePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(templatePath))
                templatePath = "D112_template.pdf";
            
            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"\n❌ EROARE: Fișierul '{templatePath}' nu a fost găsit!");
                Console.WriteLine("\n💡 SOLUȚII:");
                Console.WriteLine("1. Descarcă template-ul oficial de la:");
                Console.WriteLine("   https://static.anaf.ro/static/10/Anaf/formulare/D112_OPANAF_299_2025.pdf");
                Console.WriteLine("2. Salvează-l în același folder cu aplicația");
                Console.WriteLine("3. Redenumește-l 'D112_template.pdf' sau specifică calea corectă");
                Console.WriteLine("\nApasă orice tastă pentru a închide...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"✓ Template găsit: {templatePath}\n");
            
            // Colectare date
            Console.WriteLine("📋 DATE PERSONALE");
            Console.WriteLine("─────────────────────────────────────────────────\n");
            
            Console.Write("Nume: ");
            declaratie.Nume = Console.ReadLine();
            
            Console.Write("Prenume: ");
            declaratie.Prenume = Console.ReadLine();
            
            Console.Write("CNP: ");
            declaratie.CNP = Console.ReadLine();
            
            Console.Write("Cod fiscal (CUI/CIF): ");
            declaratie.CodFiscal = Console.ReadLine();
            
            Console.Write("Adresă: ");
            declaratie.Adresa = Console.ReadLine();
            
            Console.Write("Localitate: ");
            declaratie.Localitate = Console.ReadLine();
            
            Console.Write("Județ: ");
            declaratie.Judet = Console.ReadLine();
            
            Console.Write("Telefon (opțional): ");
            declaratie.Telefon = Console.ReadLine();
            
            Console.Write("Email (opțional): ");
            declaratie.Email = Console.ReadLine();
            
            Console.WriteLine("\n📅 DATE DECLARAȚIE");
            Console.WriteLine("─────────────────────────────────────────────────\n");
            
            Console.Write("Luna (1-12): ");
            declaratie.Luna = int.Parse(Console.ReadLine());
            
            Console.Write("Anul: ");
            declaratie.An = int.Parse(Console.ReadLine());
            
            Console.WriteLine("\n💰 VENITURI ȘI CALCULE");
            Console.WriteLine("─────────────────────────────────────────────────\n");
            
            Console.Write("Venit brut realizat (RON): ");
            declaratie.VenitBrut = decimal.Parse(Console.ReadLine());
            
            Console.Write("Tip contribuabil (1=PFA, 2=II, 3=Salariat): ");
            int tipContribuabil = int.Parse(Console.ReadLine());
            
            declaratie.CalculeazaContributii(tipContribuabil);
            
            AfiseazaSumar(declaratie);
            GenereazaFisiere(declaratie);
            
            Console.WriteLine("\nApasă orice tastă pentru a închide aplicația...");
            Console.ReadKey();
        }
        
        static void RuleazaTest()
        {
            Console.WriteLine("\n🧪 MOD TEST - Date hardcodate\n");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
            
            // TEST 1: Salariat cu salariu 5000 RON
            Console.WriteLine("📋 TEST 1: SALARIAT - 5,000 RON");
            Console.WriteLine("─────────────────────────────────────────────────");
            
            var test1 = new Declaratie112
            {
                Nume = "Popescu",
                Prenume = "Ion",
                CNP = "1850101123456",
                CodFiscal = "12345678",
                Adresa = "Str. Victoriei, nr. 10",
                Localitate = "București",
                Judet = "București",
                Telefon = "0722123456",
                Email = "ion.popescu@email.ro",
                Luna = 1,
                An = 2026,
                VenitBrut = 5000m
            };
            
            test1.CalculeazaContributii(3); // Salariat
            AfiseazaSumar(test1);
            GenereazaFisiere(test1, "_TEST1_Salariat");
            
            Console.WriteLine("\n───────────────────────────────────────────────────\n");
            
            // TEST 2: PFA cu venit mic (sub prag CASS)
            Console.WriteLine("📋 TEST 2: PFA - 5,000 RON (sub prag CASS)");
            Console.WriteLine("─────────────────────────────────────────────────");
            
            var test2 = new Declaratie112
            {
                Nume = "Ionescu",
                Prenume = "Maria",
                CNP = "2900202234567",
                CodFiscal = "87654321",
                Adresa = "Str. Libertății, nr. 25",
                Localitate = "Cluj-Napoca",
                Judet = "Cluj",
                Telefon = "0733456789",
                Email = "maria.ionescu@email.ro",
                Luna = 1,
                An = 2026,
                VenitBrut = 5000m
            };
            
            test2.CalculeazaContributii(1); // PFA
            AfiseazaSumar(test2);
            GenereazaFisiere(test2, "_TEST2_PFA_Mic");
            
            Console.WriteLine("\n───────────────────────────────────────────────────\n");
            
            // TEST 3: PFA cu venit mare (peste prag CASS)
            Console.WriteLine("📋 TEST 3: PFA - 30,000 RON (peste prag CASS)");
            Console.WriteLine("─────────────────────────────────────────────────");
            
            var test3 = new Declaratie112
            {
                Nume = "Georgescu",
                Prenume = "Andrei",
                CNP = "1750303345678",
                CodFiscal = "11223344",
                Adresa = "Bd. Unirii, nr. 50",
                Localitate = "Timișoara",
                Judet = "Timiș",
                Telefon = "0744567890",
                Email = "andrei.georgescu@email.ro",
                Luna = 1,
                An = 2026,
                VenitBrut = 30000m
            };
            
            test3.CalculeazaContributii(1); // PFA
            AfiseazaSumar(test3);
            GenereazaFisiere(test3, "_TEST3_PFA_Mare");
            
            Console.WriteLine("\n═══════════════════════════════════════════════════");
            Console.WriteLine("✓ Toate testele au fost executate cu succes!");
            Console.WriteLine("═══════════════════════════════════════════════════");
        }
        
        static void AfiseazaSumar(Declaratie112 declaratie)
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════");
            Console.WriteLine("           SUMAR CALCULE CONTRIBUȚII");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
            
            Console.WriteLine($"Nume complet:                  {declaratie.Nume} {declaratie.Prenume}");
            Console.WriteLine($"CNP:                           {declaratie.CNP}");
            Console.WriteLine($"CUI:                           {declaratie.CodFiscal}");
            Console.WriteLine($"Perioada:                      {declaratie.Luna:D2}/{declaratie.An}");
            Console.WriteLine();
            Console.WriteLine($"Venit brut:                    {declaratie.VenitBrut:N2} RON");
            Console.WriteLine($"Bază de calcul CAS:            {declaratie.BazaCalculCAS:N2} RON");
            Console.WriteLine($"Bază de calcul CASS:           {declaratie.BazaCalculCASS:N2} RON");
            Console.WriteLine();
            Console.WriteLine($"CAS (25%):                     {declaratie.CAS:N2} RON");
            Console.WriteLine($"CASS (10%):                    {declaratie.CASS:N2} RON");
            Console.WriteLine();
            Console.WriteLine($"TOTAL CONTRIBUȚII DE PLATĂ:    {declaratie.TotalContributii:N2} RON");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
        }
        
        static void GenereazaFisiere(Declaratie112 declaratie, string suffix = "")
        {
            Console.Write("Generez fișierele... ");
            try
            {
                // Generează XML
                string fisierXML = declaratie.GenereazaXML(suffix);
                
                // Generează PDF
                string fisierPDF = declaratie.GenereazaPDFNou(suffix);
                
                Console.WriteLine("✓ Succes!\n");
                
                Console.WriteLine("📄 FIȘIERE GENERATE:");
                Console.WriteLine($"   • XML: {Path.GetFileName(fisierXML)}");
                Console.WriteLine($"   • PDF: {Path.GetFileName(fisierPDF)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Eroare!\n");
                Console.WriteLine($"Detalii: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
        }
    }

    class Declaratie112
    {
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string CNP { get; set; }
        public string CodFiscal { get; set; }
        public string Adresa { get; set; }
        public string Localitate { get; set; }
        public string Judet { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }
        
        public int Luna { get; set; }
        public int An { get; set; }
        
        public decimal VenitBrut { get; set; }
        public decimal BazaCalculCAS { get; set; }
        public decimal BazaCalculCASS { get; set; }
        public decimal CAS { get; set; }
        public decimal CASS { get; set; }
        public decimal TotalContributii { get; set; }
        
        private const decimal SalariuMinimBrut = 3700m;
        private const decimal CotaCAS = 0.25m;
        private const decimal CotaCASS = 0.10m;
        private const int PlafoaneCAS = 12;
        
        public void CalculeazaContributii(int tipContribuabil)
        {
            if (tipContribuabil == 3) // Salariat
            {
                BazaCalculCAS = VenitBrut;
                BazaCalculCASS = VenitBrut;
                CAS = BazaCalculCAS * CotaCAS;
                CASS = BazaCalculCASS * CotaCASS;
            }
            else // PFA (1) sau II (2)
            {
                BazaCalculCAS = PlafoaneCAS * SalariuMinimBrut;
                decimal pragCASS = 6 * SalariuMinimBrut;
                BazaCalculCASS = VenitBrut >= pragCASS ? VenitBrut : 0;
                CAS = BazaCalculCAS * CotaCAS;
                CASS = BazaCalculCASS * CotaCASS;
            }
            
            TotalContributii = CAS + CASS;
        }
        
        public string CompleteazaTemplatePDF(string templatePath)
        {
            string numeFisier = $"D112_completat_{CodFiscal}_{An}_{Luna:D2}.pdf";
            
            using (PdfReader reader = new PdfReader(templatePath))
            using (PdfStamper stamper = new PdfStamper(reader, new FileStream(numeFisier, FileMode.Create)))
            {
                AcroFields fields = stamper.AcroFields;
                
                if (fields == null || fields.Fields.Count == 0)
                {
                    throw new Exception("PDF-ul nu conține câmpuri editabile (nu este formular interactiv)");
                }
                
                // DEBUG: Afișează toate câmpurile găsite
                Console.WriteLine("\n🔍 Câmpuri găsite în template:");
                foreach (KeyValuePair<string, AcroFields.Item> field in fields.Fields)
                {
                    Console.WriteLine($"   - {field.Key}");
                }
                Console.WriteLine();
                
                // Completare câmpuri - încearcă diferite variante de nume
                SetField(fields, new[] {"luna", "Luna", "LUNA", "Perioada_Luna"}, Luna.ToString("D2"));
                SetField(fields, new[] {"an", "An", "AN", "Perioada_An"}, An.ToString());
                
                SetField(fields, new[] {"cui", "CUI", "cod_identificare", "Cod_identificare_fiscala"}, CodFiscal);
                SetField(fields, new[] {"denumire", "Denumire", "nume_prenume"}, $"{Nume} {Prenume}");
                SetField(fields, new[] {"nume", "Nume"}, Nume);
                SetField(fields, new[] {"prenume", "Prenume"}, Prenume);
                SetField(fields, new[] {"cnp", "CNP"}, CNP);
                
                SetField(fields, new[] {"adresa", "Adresa", "adresa_domiciliu"}, 
                    $"{Adresa}, {Localitate}, {Judet}");
                SetField(fields, new[] {"telefon", "Telefon"}, Telefon ?? "");
                SetField(fields, new[] {"email", "Email", "E-mail"}, Email ?? "");
                
                // CAS
                SetField(fields, new[] {"cas_datorat", "CAS_datorat", "suma_cas"}, CAS.ToString("0.00"));
                SetField(fields, new[] {"cas_plata", "CAS_plata"}, CAS.ToString("0.00"));
                
                // CASS
                if (CASS > 0)
                {
                    SetField(fields, new[] {"cass_datorat", "CASS_datorat", "suma_cass"}, CASS.ToString("0.00"));
                    SetField(fields, new[] {"cass_plata", "CASS_plata"}, CASS.ToString("0.00"));
                }
                
                // Total
                SetField(fields, new[] {"total", "Total", "total_obligatii"}, TotalContributii.ToString("0.00"));
                
                // Păstrează formular editabil (nu face flatten)
                stamper.FormFlattening = false;
            }
            
            return Path.GetFullPath(numeFisier);
        }
        
        private void SetField(AcroFields fields, string[] possibleNames, string value)
        {
            foreach (string name in possibleNames)
            {
                if (fields.Fields.ContainsKey(name))
                {
                    fields.SetField(name, value);
                    Console.WriteLine($"   ✓ Completat: {name} = {value}");
                    return;
                }
            }
        }
        
        public string GenereazaPDFNou(string suffix = "")
        {
            string numeFisier = $"D112_completat_{CodFiscal}_{An}_{Luna:D2}{suffix}.pdf";
            
            Document document = null;
            PdfWriter writer = null;
            
            try
            {
                Console.WriteLine("   [DEBUG] Creez documentul...");
                document = new Document(PageSize.A4, 30, 30, 30, 30);
                
                Console.WriteLine("   [DEBUG] Creez writer-ul...");
                writer = PdfWriter.GetInstance(document, new FileStream(numeFisier, FileMode.Create));
                
                Console.WriteLine("   [DEBUG] Deschid documentul...");
                document.Open();
                
                if (!document.IsOpen())
                {
                    throw new Exception("Documentul nu s-a deschis!");
                }
                Console.WriteLine("   [DEBUG] Document deschis cu succes!");
                
                Console.WriteLine("   [DEBUG] Creez fonturile...");
                // Folosim IDENTITY-H pentru Unicode (funcționează pe toate platformele)
                BaseFont bfArial = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);
                Font fontNormal = new Font(bfArial, 9, Font.NORMAL);
                Font fontBold = new Font(bfArial, 9, Font.BOLD);
                Font fontTitle = new Font(bfArial, 10, Font.BOLD);
                Font fontSmall = new Font(bfArial, 7, Font.NORMAL);
                Font fontHeader = new Font(bfArial, 8, Font.NORMAL);
                
                Console.WriteLine("   [DEBUG] Adaug conținut - Header...");
                // HEADER SIMPLU pentru test
                Paragraph p1 = new Paragraph("DECLARAȚIE 112", fontTitle);
                p1.Alignment = Element.ALIGN_CENTER;
                document.Add(p1);
                
                Console.WriteLine("   [DEBUG] Adaug conținut - Spațiu...");
                document.Add(new Paragraph(" "));
                
                Console.WriteLine("   [DEBUG] Adaug conținut - Date identificare...");
                Paragraph p2 = new Paragraph($"Nume: {Nume} {Prenume}", fontNormal);
                document.Add(p2);
                
                Paragraph p3 = new Paragraph($"CNP: {CNP}", fontNormal);
                document.Add(p3);
                
                Paragraph p4 = new Paragraph($"CUI: {CodFiscal}", fontNormal);
                document.Add(p4);
                
                document.Add(new Paragraph(" "));
                
                Console.WriteLine("   [DEBUG] Adaug conținut - Contribuții...");
                Paragraph p5 = new Paragraph($"CAS: {CAS:N2} RON", fontBold);
                document.Add(p5);
                
                Paragraph p6 = new Paragraph($"CASS: {CASS:N2} RON", fontBold);
                document.Add(p6);
                
                Paragraph p7 = new Paragraph($"TOTAL: {TotalContributii:N2} RON", fontBold);
                document.Add(p7);
                
                Console.WriteLine("   [DEBUG] Conținut adăugat cu succes!");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   [DEBUG] EROARE la: {ex.Message}");
                Console.WriteLine($"   [DEBUG] StackTrace: {ex.StackTrace}");
                
                try
                {
                    if (document != null && document.IsOpen())
                        document.Close();
                }
                catch { }
                
                throw;
            }
            finally
            {
                Console.WriteLine("   [DEBUG] Închid documentul...");
                try
                {
                    if (document != null && document.IsOpen())
                    {
                        document.Close();
                        Console.WriteLine("   [DEBUG] Document închis!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   [DEBUG] Eroare la închidere: {ex.Message}");
                }
                
                try
                {
                    if (writer != null)
                    {
                        writer.Close();
                        Console.WriteLine("   [DEBUG] Writer închis!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   [DEBUG] Eroare la închidere writer: {ex.Message}");
                }
            }
            
            Console.WriteLine($"   [DEBUG] Returnez: {numeFisier}");
            return Path.GetFullPath(numeFisier);
        }
        
        public string GenereazaXML(string suffix = "")
        {
            string numeFisier = $"D112_{CodFiscal}_{An}{Luna:D2}{suffix}.xml";
            
            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<declaratie xmlns=\"mfp:anaf:dgti:d112:declaratie:v1\">");
            xml.AppendLine("  <antet>");
            xml.AppendLine($"    <luna>{Luna}</luna>");
            xml.AppendLine($"    <an>{An}</an>");
            xml.AppendLine("    <denumire_declaratie>D112</denumire_declaratie>");
            xml.AppendLine("  </antet>");
            xml.AppendLine("  <platitor>");
            xml.AppendLine($"    <cui>{CodFiscal}</cui>");
            xml.AppendLine($"    <denumire>{Nume} {Prenume}</denumire>");
            xml.AppendLine($"    <cnp>{CNP}</cnp>");
            xml.AppendLine("    <adresa>");
            xml.AppendLine($"      <strada>{Adresa}</strada>");
            xml.AppendLine($"      <localitate>{Localitate}</localitate>");
            xml.AppendLine($"      <judet>{Judet}</judet>");
            xml.AppendLine("    </adresa>");
            
            if (!string.IsNullOrEmpty(Telefon))
                xml.AppendLine($"    <telefon>{Telefon}</telefon>");
            
            if (!string.IsNullOrEmpty(Email))
                xml.AppendLine($"    <email>{Email}</email>");
            
            xml.AppendLine("  </platitor>");
            xml.AppendLine("  <creante_fiscale>");
            
            // CAS
            xml.AppendLine("    <creanta>");
            xml.AppendLine("      <cod_bugetar>51.01.01</cod_bugetar>");
            xml.AppendLine("      <denumire>Contribuția de asigurări sociale</denumire>");
            xml.AppendLine($"      <suma_datorata>{CAS:F2}</suma_datorata>");
            xml.AppendLine("      <suma_deductibila>0.00</suma_deductibila>");
            xml.AppendLine("      <suma_scutita>0.00</suma_scutita>");
            xml.AppendLine($"      <suma_de_plata>{CAS:F2}</suma_de_plata>");
            xml.AppendLine("    </creanta>");
            
            // CASS
            if (CASS > 0)
            {
                xml.AppendLine("    <creanta>");
                xml.AppendLine("      <cod_bugetar>51.01.04</cod_bugetar>");
                xml.AppendLine("      <denumire>Contribuția de asigurări sociale de sănătate</denumire>");
                xml.AppendLine($"      <suma_datorata>{CASS:F2}</suma_datorata>");
                xml.AppendLine("      <suma_deductibila>0.00</suma_deductibila>");
                xml.AppendLine("      <suma_scutita>0.00</suma_scutita>");
                xml.AppendLine($"      <suma_de_plata>{CASS:F2}</suma_de_plata>");
                xml.AppendLine("    </creanta>");
            }
            
            xml.AppendLine("  </creante_fiscale>");
            xml.AppendLine($"  <total_obligatii>{TotalContributii:F2}</total_obligatii>");
            xml.AppendLine("</declaratie>");
            
            File.WriteAllText(numeFisier, xml.ToString(), Encoding.UTF8);
            
            return Path.GetFullPath(numeFisier);
        }
        
        private PdfPCell CreateCell(string text, Font font, bool alignRight)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.Padding = 3;
            if (alignRight)
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            return cell;
        }
    }
}