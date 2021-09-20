using System;
using System.Collections.Generic;

namespace EmployeeService
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private EmployeeDbContext _dbcontext;

        public EmployeeRepository(EmployeeDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _dbcontext.Employees;
        }
    }
}
