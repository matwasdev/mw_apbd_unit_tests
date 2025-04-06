using Tutorial3.Models;

public class EmpDeptSalgradeTests
{
    // 1. Simple WHERE filter
    // SQL: SELECT * FROM Emp WHERE Job = 'SALESMAN';
    [Fact]
    public void ShouldReturnAllSalesmen()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        List<Emp> result = emps.Where(emp => emp.Job.Equals("SALESMAN")).ToList();

        //Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("SALESMAN", e.Job));
    }

    // 2. WHERE + OrderBy
    // SQL: SELECT * FROM Emp WHERE DeptNo = 30 ORDER BY Sal DESC;
    [Fact]
    public void ShouldReturnDept30EmpsOrderedBySalaryDesc()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        List<Emp> result = emps
            .Where(emp => emp.DeptNo == 30)
            .OrderByDescending(emp => emp.Sal)
            .ToList();

        //Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].Sal >= result[1].Sal);
    }

    // 3. Subquery using LINQ (IN clause)
    // SQL: SELECT * FROM Emp WHERE DeptNo IN (SELECT DeptNo FROM Dept WHERE Loc = 'CHICAGO');
    [Fact]
    public void ShouldReturnEmployeesFromChicago()
    {
        //Arrange
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        //Act
        List<Emp> result = emps.Where(emp => depts
            .Where(dept => dept.Loc == "CHICAGO")
            .Select(dept => dept.DeptNo)
            .Contains(emp.DeptNo))
            .ToList();

        
        //Assert
        Assert.All(result, e => Assert.Equal(30, e.DeptNo));
    }

    // 4. SELECT projection
    // SQL: SELECT EName, Sal FROM Emp;
    [Fact]
    public void ShouldSelectNamesAndSalaries()
    {
        var emps = Database.GetEmps();
        
        //Act
        var result = emps.Select(emp => new { emp.EName, emp.Sal });
        
        //Assert
         Assert.All(result, r =>
         {
             Assert.False(string.IsNullOrWhiteSpace(r.EName));
             Assert.True(r.Sal > 0);
         });
    }

    // 5. JOIN Emp to Dept
    // SQL: SELECT E.EName, D.DName FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo;
    [Fact]
    public void ShouldJoinEmployeesWithDepartments()
    {
        //Arrange
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        //Act
        var result = emps.Join(depts, emp => emp.DeptNo, dept => dept.DeptNo,
            (emp, dept) => new { emp.EName, dept.DName });
        
        //Assert
        Assert.Contains(result, r => r.DName == "SALES" && r.EName == "ALLEN");
    }

    // 6. Group by DeptNo
    // SQL: SELECT DeptNo, COUNT(*) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCountEmployeesPerDepartment()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var result = emps.GroupBy(emp => emp.DeptNo)
            .Select(pair => new { DeptNo = pair.Key, Count = pair.Count() });  
            
        //Assert    
        Assert.Contains(result, g => g.DeptNo == 30 && g.Count == 2);
    }
    

    // 7. SelectMany (simulate flattening)
    // SQL: SELECT EName, Comm FROM Emp WHERE Comm IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithCommission()
    {
        //Arrange
        var emps = Database.GetEmps();

        // Act
        var result = emps
            .Where(emp => emp.Comm!=null)
            .SelectMany(e => new[] { new { e.EName, e.Comm } }).ToList();
        
         //Assert
         Assert.All(result, r => Assert.NotNull(r.Comm));
    }

    // 8. Join with Salgrade
    // SQL: SELECT E.EName, S.Grade FROM Emp E JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldMatchEmployeeToSalaryGrade()
    {
        //Arrange
        var emps = Database.GetEmps();
        var grades = Database.GetSalgrades();

        //Act
        var result = (from emp in emps
                         from sal in grades
                         where emp.Sal >= sal.Losal && emp.Sal <= sal.Hisal
                         select new { emp.EName,sal.Grade })
                        .ToList();

        //Assert
        Assert.Contains(result, r => r.EName == "ALLEN" && r.Grade == 3);
    }

    // 9. Aggregation (AVG)
    // SQL: SELECT DeptNo, AVG(Sal) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCalculateAverageSalaryPerDept()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var result = emps.GroupBy(emp => emp.DeptNo)
            .Select(emp => new { DeptNo = emp.Key, AvgSal = emp.Average(e => e.Sal) })
            .ToList();
        
        
        //Assert
        Assert.Contains(result, r => r.DeptNo == 30 && r.AvgSal > 1000);
    }

    // 10. Complex filter with subquery and join
    // SQL: SELECT E.EName FROM Emp E WHERE E.Sal > (SELECT AVG(Sal) FROM Emp WHERE DeptNo = E.DeptNo);
    [Fact]
    public void ShouldReturnEmployeesEarningMoreThanDeptAverage()
    {
        //Arrange
        var emps = Database.GetEmps();

        //Act
        var result = emps.Where(emp => emp.Sal > emps
                .Where(e => e.DeptNo == emp.DeptNo)
                .Average(e => e.Sal))
                .Select(emp => emp.EName)
                .ToList();
        
        
        //Assert
        Assert.Contains("ALLEN", result);
    }
    
}
