namespace testDeclaratie112;

public class SalaryReport(ISalaryCalculator salaryCalculator, ITaxCalculator taxCalculator)
{
    public double GenerateReport(Angajat employee)
    {
        return salaryCalculator.CalculateGrossSalary(employee) 
               - taxCalculator.CalculeazaCas(employee) 
               - taxCalculator.CalculeazaCass(employee) 
               - taxCalculator.CalculeazaImpozitVenit(employee);
    }
}