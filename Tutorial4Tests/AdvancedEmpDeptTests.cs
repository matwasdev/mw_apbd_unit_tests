using Tutorial3.Models;

namespace Tutorial3Tests;

public class AdvancedEmpDeptTests
{
    // 11. MAX salary
    // SQL: SELECT MAX(Sal) FROM Emp;
    [Fact]
    public void ShouldReturnMaxSalary()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        decimal? maxSalary = emps.Max(e => e.Sal);

        //Assert
        Assert.Equal(5000, maxSalary);
    }

    // 12. MIN salary in department 30
    // SQL: SELECT MIN(Sal) FROM Emp WHERE DeptNo = 30;
    [Fact]
    public void ShouldReturnMinSalaryInDept30()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        decimal? minSalary = emps.Where(e => e.DeptNo==30)
            .Min(e => e.Sal);

        //Assert
        Assert.Equal(1250, minSalary);
    }

    // 13. Take first 2 employees ordered by hire date
    // SQL: SELECT * FROM Emp ORDER BY HireDate ASC FETCH FIRST 2 ROWS ONLY;
    [Fact]
    public void ShouldReturnFirstTwoHiredEmployees()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var firstTwo = emps.OrderBy(emp => emp.HireDate).Take(2).ToList();
        
        //Assert
        Assert.Equal(2, firstTwo.Count);
        Assert.True(firstTwo[0].HireDate <= firstTwo[1].HireDate);
    }

    // 14. DISTINCT job titles
    // SQL: SELECT DISTINCT Job FROM Emp;
    [Fact]
    public void ShouldReturnDistinctJobTitles()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var jobs = emps.Select(emp => emp.Job).Distinct().ToList();
        
        //Assert
        Assert.Contains("PRESIDENT", jobs);
        Assert.Contains("SALESMAN", jobs);
    }

    // 15. Employees with managers (NOT NULL Mgr)
    // SQL: SELECT * FROM Emp WHERE Mgr IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithManagers()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var withMgr = emps.Where(emp => emp.Mgr != null).ToList(); 
        
        //Assert
        Assert.All(withMgr, e => Assert.NotNull(e.Mgr));
    }

    // 16. All employees earn more than 500
    // SQL: SELECT * FROM Emp WHERE Sal > 500; (simulate all check)
    [Fact]
    public void AllEmployeesShouldEarnMoreThan500()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var result = emps.All(emp => emp.Sal > 500);
        
        //Assert
        Assert.True(result);
    }

    // 17. Any employee with commission over 400
    // SQL: SELECT * FROM Emp WHERE Comm > 400;
    [Fact]
    public void ShouldFindAnyWithCommissionOver400()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var result = emps.Any(emp => emp.Comm > 400);
        
        //Assert
        Assert.True(result);
    }

    // 18. Self-join to get employee-manager pairs
    // SQL: SELECT E1.EName AS Emp, E2.EName AS Manager FROM Emp E1 JOIN Emp E2 ON E1.Mgr = E2.EmpNo;
    [Fact]
    public void ShouldReturnEmployeeManagerPairs()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var result = emps
            .Join(emps, e1 => e1.Mgr, e2 => e2.EmpNo, (e1, e2) => new { Employee = e1.EName, Manager = e2.EName })
            .Select(e => new
            {
                Employee = e.Employee,
                Manager = e.Manager,
            });
        
        //Assert
        Assert.Contains(result, r => r.Employee == "SMITH" && r.Manager == "FORD");
    }

    // 19. Let clause usage (sal + comm)
    // SQL: SELECT EName, (Sal + COALESCE(Comm, 0)) AS TotalIncome FROM Emp;
    [Fact]
    public void ShouldReturnTotalIncomeIncludingCommission()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var result = emps.Select(emp => new {emp.EName, Total = emp.Sal + (emp.Comm ?? 0)}); 
        
        //Assert
        Assert.Contains(result, r => r.EName == "ALLEN" && r.Total == 1900);
    }

    // 20. Join all three: Emp → Dept → Salgrade
    // SQL: SELECT E.EName, D.DName, S.Grade FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldJoinEmpDeptSalgrade()
    {
        //Arrange
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        var grades = Database.GetSalgrades();

        //Act
        var result = (from e in emps
                join d in depts on e.DeptNo equals d.DeptNo
                from s in grades
                where e.Sal >= s.Losal && e.Sal <= s.Hisal
                select new
                {
                    e.EName,
                    d.DName,
                    s.Grade
                }
            );
        
        //Assert
        Assert.Contains(result, r => r.EName == "ALLEN" && r.DName == "SALES" && r.Grade == 3);
    }
}
