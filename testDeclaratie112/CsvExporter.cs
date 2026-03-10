using System.Text;

namespace testDeclaratie112;

public class CsvExporter : IExporter
{
    public void Export(Angajat employee, string filePath)
    {
        StringBuilder csvContent = new StringBuilder();

        // Adăugăm antetul CSV
        csvContent.AppendLine("Nume,NormaLunară,OreLucrate,SalariuBrut,Bonus,AvntajeNatura,CAS,CASS,ImpozitVenit,SalarNet");
        
            // Calculăm salariul net și taxele
            var report = new SalaryReport(new SalaryCalculator(), new CalculatorTaxe());

            // Adăugăm datele în CSV
            csvContent.AppendLine($"{employee.Nume},{employee.NormaLunară},{employee.OreLucrate},{employee.SalariuBrutCim},{employee.Bonus},{employee.AvntajeNatura}" +
                                  $",{(new CalculatorTaxe()).CalculeazaCas(employee)},{(new CalculatorTaxe()).CalculeazaCass(employee)}" +
                                  $",{Math.Round((new CalculatorTaxe()).CalculeazaImpozitVenit(employee),0)},{Math.Round(report.GenerateReport(employee),0)}");
        

        // Salvăm fișierul CSV
        File.WriteAllText(filePath, csvContent.ToString());
    }
}