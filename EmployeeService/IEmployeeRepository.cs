using System.Collections.Generic;

namespace EmployeeService
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetEmployees();
    }
}