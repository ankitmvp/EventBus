using System.Collections.Generic;

namespace EmployeeService
{
    public interface IEmployeeService
    {
        IEnumerable<Employee> GetEmployees();
    }
}