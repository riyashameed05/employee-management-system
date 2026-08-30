import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Employee } from './employee.model';
import { EmployeeService } from './employee.service';

@Component({
  selector: 'app-root', standalone: true, imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html', styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private readonly employeeService = inject(EmployeeService);
  employees: Employee[] = []; filteredEmployees: Employee[] = []; searchText = '';
  loading = false; error = ''; showForm = false; editingEmployee: Employee | null = null;
  form: Omit<Employee, 'id'> = this.emptyForm();
  get departmentCount(): number { return new Set(this.employees.map(e => e.department.toLowerCase())).size; }
  get averageSalary(): number { return this.employees.length ? this.employees.reduce((sum, e) => sum + Number(e.salary), 0) / this.employees.length : 0; }

  ngOnInit(): void { this.loadEmployees(); }
  loadEmployees(): void { this.loading = true; this.error = ''; this.employeeService.getEmployees().subscribe({ next: e => { this.employees = e; this.applyFilter(); this.loading = false; }, error: () => { this.error = 'Unable to load employees. Make sure the API and PostgreSQL are running.'; this.loading = false; } }); }
  applyFilter(): void { const term = this.searchText.trim().toLowerCase(); this.filteredEmployees = !term ? this.employees : this.employees.filter(e => `${e.firstName} ${e.lastName} ${e.email} ${e.department}`.toLowerCase().includes(term)); }
  openCreate(): void { this.editingEmployee = null; this.form = this.emptyForm(); this.showForm = true; }
  openEdit(employee: Employee): void { this.editingEmployee = employee; this.form = { firstName: employee.firstName, lastName: employee.lastName, email: employee.email, department: employee.department, salary: employee.salary, joiningDate: employee.joiningDate.substring(0, 10) }; this.showForm = true; }
  save(): void { const request = this.editingEmployee ? this.employeeService.updateEmployee({ ...this.form, id: this.editingEmployee.id }) : this.employeeService.createEmployee(this.form); request.subscribe({ next: () => { this.showForm = false; this.loadEmployees(); }, error: () => this.error = 'Unable to save employee. Check that the email is unique.' }); }
  delete(id: number): void { if (!confirm('Delete this employee?')) return; this.employeeService.deleteEmployee(id).subscribe({ next: () => this.loadEmployees(), error: () => this.error = 'Unable to delete employee.' }); }
  private emptyForm(): Omit<Employee, 'id'> { return { firstName: '', lastName: '', email: '', department: '', salary: 0, joiningDate: new Date().toISOString().substring(0, 10) }; }
}
