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
  employees: Employee[] = [];
  filteredEmployees: Employee[] = [];
  searchText = '';
  loading = false;
  saving = false;
  deletingId: number | null = null;
  error = '';
  showForm = false;
  editingEmployee: Employee | null = null;
  form: Omit<Employee, 'id'> = this.emptyForm();

  get departmentCount(): number {
    return new Set(this.employees.map(e => e.department.trim().toLowerCase())).size;
  }

  get averageSalary(): number {
    return this.employees.length
      ? this.employees.reduce((sum, e) => sum + Number(e.salary), 0) / this.employees.length
      : 0;
  }

  ngOnInit(): void { this.loadEmployees(); }

  loadEmployees(): void {
    this.loading = true;
    this.error = '';
    this.employeeService.getEmployees().subscribe({
      next: employees => {
        this.employees = employees;
        this.applyFilter();
        this.loading = false;
      },
      error: () => {
        this.error = 'Unable to load employees. Please try again.';
        this.loading = false;
      }
    });
  }

  applyFilter(): void {
    const term = this.searchText.trim().toLowerCase();
    this.filteredEmployees = !term
      ? this.employees
      : this.employees.filter(e => `${e.firstName} ${e.lastName} ${e.email} ${e.department}`.toLowerCase().includes(term));
  }

  openCreate(): void {
    this.editingEmployee = null;
    this.form = this.emptyForm();
    this.error = '';
    this.showForm = true;
  }

  openEdit(employee: Employee): void {
    this.editingEmployee = employee;
    this.form = {
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      department: employee.department,
      salary: employee.salary,
      joiningDate: employee.joiningDate.substring(0, 10)
    };
    this.error = '';
    this.showForm = true;
  }

  save(): void {
    if (this.saving) return;
    this.error = '';
    this.saving = true;

    if (this.editingEmployee) {
      this.employeeService.updateEmployee({ ...this.form, id: this.editingEmployee.id }).subscribe({
        next: () => this.finishSave(),
        error: err => this.handleSaveError(err)
      });
    } else {
      this.employeeService.createEmployee(this.form).subscribe({
        next: () => this.finishSave(),
        error: err => this.handleSaveError(err)
      });
    }
  }

  delete(id: number): void {
    if (this.deletingId !== null || !confirm('Delete this employee? This action cannot be undone.')) return;
    this.error = '';
    this.deletingId = id;
    this.employeeService.deleteEmployee(id).subscribe({
      next: () => {
        this.deletingId = null;
        this.loadEmployees();
      },
      error: () => {
        this.deletingId = null;
        this.error = 'Unable to delete employee. Please try again.';
      }
    });
  }

  private finishSave(): void {
    this.saving = false;
    this.showForm = false;
    this.loadEmployees();
  }

  private handleSaveError(error: { status?: number }): void {
    this.saving = false;
    this.error = error?.status === 409
      ? 'An employee with this email already exists.'
      : 'Unable to save employee. Please check the details and try again.';
  }

  private emptyForm(): Omit<Employee, 'id'> {
    return {
      firstName: '',
      lastName: '',
      email: '',
      department: '',
      salary: 0,
      joiningDate: new Date().toISOString().substring(0, 10)
    };
  }
}
