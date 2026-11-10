
import api from './api'

export interface Employee {
  id: string
  fullName: string
  email: string
  department: string
  role: 'Employee' | 'HOD' | 'HR'
  createdAtUtc: string
}

export interface CreateEmployeeRequest {
  fullName: string
  email: string
  department: string
}

export interface SetEmployeePasswordRequest {
  password: string
}

export const employeeService = {
  async getEmployees(): Promise<Employee[]> {
    const response = await api.get<Employee[]>('/Employees')
    return response.data
  },

  async createEmployee(
    request: CreateEmployeeRequest,
  ): Promise<Employee> {
    const response = await api.post<Employee>(
      '/Employees',
      request,
    )

    return response.data
  },

  async setPassword(
    employeeId: string,
    request: SetEmployeePasswordRequest,
  ): Promise<void> {
    await api.post(
      `/Employees/${employeeId}/set-password`,
      request,
    )
  },
}
