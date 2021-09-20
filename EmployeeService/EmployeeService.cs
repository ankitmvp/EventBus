using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EventBus.Contract;
using Newtonsoft.Json;

namespace EmployeeService
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IEventBus _bus;

        public EmployeeService(IEmployeeRepository repo, IEventBus bus)
        {
            _repo = repo;
            _bus = bus;
        }

        public IEnumerable<Employee> GetEmployees()
        {
            var employees = _repo.GetEmployees();
            _bus.SendCommand(new FileCreateCommand(JsonConvert.SerializeObject(employees)));
            return employees;
        }
    }
}
