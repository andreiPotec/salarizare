using iTextSharp.text.pdf;

namespace Declaratie112Generator;

public class XFAInjector
{
    public string InjectPDF(string XML, ModelD112 model)
    {
        string pdf = $"D112_completat_{model.CUI}_{model.An}_{model.Luna:D2}.pdf";
        
        return InjectDataInPdf("D112_template.pdf",pdf, XML, model);
    }

    private string InjectDataInPdf(string template, string output, string xmlPath, ModelD112 model)
    {
        using (var reader = new PdfReader(template))
            using (var stamper = new PdfStamper(reader, new FileStream(output, FileMode.Create)))
            {
                var fields = stamper.AcroFields;
                int campuriCompletate = 0;
                
                Console.WriteLine("   🔍 Analizez structura PDF...");
                // Afișează toate câmpurile disponibile (pentru debugging)
                if (fields.Fields.Count > 0)
                {
                    Console.WriteLine($"   ℹ PDF conține {fields.Fields.Count} câmpuri editabile");
                    
                    // Încearcă să completeze câmpurile comune
                    var mapariCampuri = new Dictionary<string, string>
                    {
                        // Variante posibile pentru CUI
                        { "cui", model.CUI },
                        { "CUI", model.CUI },
                        { "cod_identificare", model.CUI },
                        { "Cod_identificare_fiscala", model.CUI },
                        { "codIdentificare", model.CUI },
                        
                        // Variante pentru denumire
                        { "denumire", $"{model.Nume} {model.Prenume}" },
                        { "Denumire", $"{model.Nume} {model.Prenume}" },
                        { "nume_prenume", $"{model.Nume} {model.Prenume}" },
                        
                        // Variante pentru nume separat
                        { "nume", model.Nume },
                        { "Nume", model.Nume },
                        { "prenume", model.Prenume },
                        { "Prenume", model.Prenume },
                        
                        // CNP
                        { "cnp", model.CNP },
                        { "CNP", model.CNP },
                        
                        // Adresă
                        { "adresa", $"{model.Adresa}, {model.Localitate}, {model.Judet}" },
                        { "Adresa", $"{model.Adresa}, {model.Localitate}, {model.Judet}" },
                        { "adresa_domiciliu", $"{model.Adresa}, {model.Localitate}, {model.Judet}" },
                        
                        // Perioada
                        { "luna", model.Luna.ToString("D2") },
                        { "Luna", model.Luna.ToString("D2") },
                        { "an", model.An.ToString() },
                        { "An", model.An.ToString() },
                        
                        // CAS
                        { "cas", model.CAS.ToString("0.00") },
                        { "CAS", model.CAS.ToString("0.00") },
                        { "cas_datorat", model.CAS.ToString("0.00") },
                        { "cas_plata", model.CAS.ToString("0.00") },
                        { "suma_cas", model.CAS.ToString("0.00") },
                        
                        // CASS
                        { "cass", model.CASS.ToString("0.00") },
                        { "CASS", model.CASS.ToString("0.00") },
                        { "cass_datorat", model.CASS.ToString("0.00") },
                        { "cass_plata", model.CASS.ToString("0.00") },
                        { "suma_cass", model.CASS.ToString("0.00") },
                        
                        // Total
                        { "total", model.TotalContributii.ToString("0.00") },
                        { "Total", model.TotalContributii.ToString("0.00") },
                        { "total_obligatii", model.TotalContributii.ToString("0.00") },
                        { "totalObligatii", model.TotalContributii.ToString("0.00") }
                    };
                    
                    // Încearcă să completeze fiecare câmp
                    foreach (var mapare in mapariCampuri)
                    {
                        if (TrySetField(fields, mapare.Key, mapare.Value))
                        {
                            campuriCompletate++;
                            Console.WriteLine($"   ✓ Completat: {mapare.Key} = {mapare.Value}");
                        }
                    }
                    
                    if (campuriCompletate == 0)
                    {
                        Console.WriteLine("   ⚠ Nu s-a putut completa niciun câmp automat");
                        Console.WriteLine("   ℹ Lista câmpuri disponibile în PDF:");
                        int count = 0;
                        foreach (var field in fields.Fields.Keys)
                        {
                            Console.WriteLine($"      - {field}");
                            count++;
                            if (count >= 10)
                            {
                                Console.WriteLine($"      ... și încă {fields.Fields.Count - 10} câmpuri");
                                break;
                            }
                        }
                    }
                    
                    // Păstrează formular editabil
                    stamper.FormFlattening = false;
                }
                else
                {
                    Console.WriteLine("   ℹ PDF nu conține câmpuri editabile (formular AcroForm)");
                    Console.WriteLine("   ℹ Probabil este formular XFA - necesită Adobe Reader");
                }
                
                // Încearcă să injecteze XML-ul în XFA (dacă există)
                if (reader.AcroFields.Xfa.XfaPresent)
                {
                    Console.WriteLine("   ✓ PDF conține formular XFA");
                    Console.WriteLine("   → Încerc injectare XML în XFA...");
                    
                    try
                    {
                        // Citește XML-ul generat
                        string xmlContent = File.ReadAllText(xmlPath);
                        
                        // Încearcă să injecteze în XFA
                        // NOTĂ: iTextSharp are suport limitat pentru XFA
                        // Pentru XFA complet, ar trebui folosit Adobe LiveCycle sau Acrobat
                        
                        Console.WriteLine("   ⚠ Injectarea XFA necesită Adobe Acrobat/LiveCycle");
                        Console.WriteLine("   💡 Alternative:");
                        Console.WriteLine("      1. Deschide PDF-ul în Adobe Reader");
                        Console.WriteLine("      2. Importă XML-ul folosind 'Import Data' din meniu");
                        Console.WriteLine($"      3. Folosește XML-ul generat: {Path.GetFileName(xmlPath)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠ Eroare XFA: {ex.Message}");
                    }
                }
            }
            
            Console.WriteLine($"   ✓ PDF salvat: {output}");
            
            return Path.GetFullPath(output);
    }

    bool TrySetField(AcroFields fields, string fieldName, string value)
    {
        try
        {
            if (fields.Fields.ContainsKey(fieldName))
            {
                fields.SetField(fieldName, value);
                return true;
            }

            return false;
        }
        catch
        {
            return false;

        }
    }
}