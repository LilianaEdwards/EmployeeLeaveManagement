
import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  KeyRound,
  Plus,
  RefreshCw,
  Search,
  Users,
  X,
} from 'lucide-react'
import {
  employeeService,
  type Employee,
} from '../../services/employeeService'

function getErrorMessage(error: unknown): string {
  if (
    typeof error === 'object' &&
    error !== null &&
    'response' in error
  ) {
    const response = (
      error as {
        response?: {
          data?: {
            message?: string
            detail?: string
            title?: string
          }
          status?: number
        }
      }
    ).response

    if (response?.status === 401) {
      return 'Your session has expired. Please sign in again.'
    }

    if (response?.status === 403) {
      return 'You do not have permission to perform this action.'
    }

    return (
      response?.data?.message ??
      response?.data?.detail ??
      response?.data?.title ??
      'The server could not complete your request.'
    )
  }

  return 'Unable to connect to the server. Please check that the backend is running.'
}

function formatDate(value: string) {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleDateString(undefined, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export default function EmployeeManagement() {
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [search, setSearch] = useState('')

  const [showCreateForm, setShowCreateForm] = useState(false)
  const [creating, setCreating] = useState(false)
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [department, setDepartment] = useState('')

  const [selectedEmployee, setSelectedEmployee] =
    useState<Employee | null>(null)
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [savingPassword, setSavingPassword] = useState(false)

  const loadEmployees = useCallback(async () => {
    setLoading(true)
    setError('')

    try {
      const data = await employeeService.getEmployees()
      setEmployees(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadEmployees()
  }, [loadEmployees])

  const filteredEmployees = useMemo(() => {
    const term = search.trim().toLowerCase()

    if (!term) return employees

    return employees.filter(
      (employee) =>
        employee.fullName.toLowerCase().includes(term) ||
        employee.email.toLowerCase().includes(term) ||
        employee.department.toLowerCase().includes(term) ||
        employee.role.toLowerCase().includes(term),
    )
  }, [employees, search])

  async function handleCreateEmployee(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setError('')
    setSuccess('')

    if (!fullName.trim() || !email.trim() || !department.trim()) {
      setError('Please complete all employee fields.')
      return
    }

    setCreating(true)

    try {
      await employeeService.createEmployee({
        fullName: fullName.trim(),
        email: email.trim(),
        department: department.trim(),
      })

      setSuccess(
        'Employee created successfully. Set their password before they sign in.',
      )

      setFullName('')
      setEmail('')
      setDepartment('')
      setShowCreateForm(false)

      await loadEmployees()
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setCreating(false)
    }
  }

  async function handleSetPassword(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setError('')
    setSuccess('')

    if (!selectedEmployee) return

    if (password.length < 12 || password.length > 128) {
      setError('Password must contain between 12 and 128 characters.')
      return
    }

    if (password !== confirmPassword) {
      setError('The passwords do not match.')
      return
    }

    setSavingPassword(true)

    try {
      await employeeService.setPassword(selectedEmployee.id, {
        password,
      })

      setSuccess(
        `Password successfully set for ${selectedEmployee.fullName}.`,
      )

      setSelectedEmployee(null)
      setPassword('')
      setConfirmPassword('')
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setSavingPassword(false)
    }
  }

  function closeCreateForm() {
    setShowCreateForm(false)
    setFullName('')
    setEmail('')
    setDepartment('')
  }

  function closePasswordForm() {
    setSelectedEmployee(null)
    setPassword('')
    setConfirmPassword('')
  }

  return (
    <div className="employee-management">
      <div className="employee-management-heading">
        <div>
          <h3>Employee Directory</h3>
          <p>Register and manage employee accounts.</p>
        </div>

        <div className="employee-management-actions">
          <button
            type="button"
            className="management-refresh"
            onClick={() => void loadEmployees()}
            disabled={loading}
          >
            <RefreshCw size={16} />
            Refresh
          </button>

          <button
            type="button"
            className="employee-add-button"
            onClick={() => {
              setError('')
              setSuccess('')
              setShowCreateForm(true)
            }}
          >
            <Plus size={17} />
            Add Employee
          </button>
        </div>
      </div>

      <div className="employee-management-summary">
        <div className="employee-summary-icon">
          <Users size={23} />
        </div>
        <div>
          <span>Total registered employees</span>
          <strong>{employees.length}</strong>
        </div>
      </div>

      {error && (
        <div className="management-alert error">
          {error}
        </div>
      )}

      {success && (
        <div className="employee-success-message">
          {success}
        </div>
      )}

      <section className="management-panel">
        <div className="management-panel-heading">
          <div>
            <h3>All Employees</h3>
            <p>Search employees by name, email, department, or role.</p>
          </div>
        </div>

        <div className="management-filters">
          <div className="management-search">
            <Search size={17} />
            <input
              type="text"
              placeholder="Search employees..."
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
          </div>
        </div>

        {loading ? (
          <div className="management-loading">
            <RefreshCw size={23} className="management-spin" />
            <p>Loading employees...</p>
          </div>
        ) : employees.length === 0 ? (
          <div className="management-empty">
            <Users size={30} />
            <h3>No employees registered</h3>
            <p>Use Add Employee to register the first employee.</p>
          </div>
        ) : filteredEmployees.length === 0 ? (
          <div className="management-empty">
            <Search size={30} />
            <h3>No matching employees</h3>
            <p>Try another name, email, or department.</p>
          </div>
        ) : (
          <div className="management-table-wrapper">
            <table className="management-table">
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>Role</th>
                  <th>Created</th>
                  <th>Action</th>
                </tr>
              </thead>

              <tbody>
                {filteredEmployees.map((employee) => (
                  <tr key={employee.id}>
                    <td>
                      <div className="employee-directory-name">
                        <div className="employee-directory-avatar">
                          {employee.fullName.charAt(0).toUpperCase()}
                        </div>
                        <strong>{employee.fullName}</strong>
                      </div>
                    </td>

                    <td>{employee.email}</td>
                    <td>{employee.department}</td>

                    <td>
                      <span className="employee-role-badge">
                        {employee.role}
                      </span>
                    </td>

                    <td>{formatDate(employee.createdAtUtc)}</td>

                    <td>
                      <button
                        type="button"
                        className="employee-password-button"
                        onClick={() => {
                          setError('')
                          setSuccess('')
                          setPassword('')
                          setConfirmPassword('')
                          setSelectedEmployee(employee)
                        }}
                      >
                        <KeyRound size={15} />
                        Set password
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {showCreateForm && (
        <div
          className="management-modal-backdrop"
          onClick={closeCreateForm}
        >
          <section
            className="management-modal employee-management-modal"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="management-modal-heading">
              <div>
                <span className="management-eyebrow">
                  EMPLOYEE MANAGEMENT
                </span>
                <h3>Add New Employee</h3>
              </div>

              <button
                type="button"
                className="management-modal-close"
                onClick={closeCreateForm}
                aria-label="Close form"
              >
                <X size={20} />
              </button>
            </div>

            <form
              className="employee-management-form"
              onSubmit={handleCreateEmployee}
            >
              <label htmlFor="employee-full-name">
                Full Name
              </label>
              <input
                id="employee-full-name"
                type="text"
                placeholder="Enter employee's full name"
                minLength={2}
                maxLength={100}
                required
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
              />

              <label htmlFor="employee-email">
                Email Address
              </label>
              <input
                id="employee-email"
                type="email"
                placeholder="employee@example.com"
                maxLength={254}
                required
                value={email}
                onChange={(event) => setEmail(event.target.value)}
              />

              <label htmlFor="employee-department">
                Department
              </label>
              <input
                id="employee-department"
                type="text"
                placeholder="e.g. Engineering"
                minLength={1}
                maxLength={100}
                required
                value={department}
                onChange={(event) => setDepartment(event.target.value)}
              />

              <div className="employee-form-note">
                New accounts are created with the Employee role.
                You can set their login password after registration.
              </div>

              <div className="employee-form-actions">
                <button
                  type="button"
                  className="employee-cancel-button"
                  onClick={closeCreateForm}
                  disabled={creating}
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="employee-add-button"
                  disabled={creating}
                >
                  {creating ? 'Creating...' : 'Create Employee'}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}

      {selectedEmployee && (
        <div
          className="management-modal-backdrop"
          onClick={closePasswordForm}
        >
          <section
            className="management-modal employee-management-modal"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="management-modal-heading">
              <div>
                <span className="management-eyebrow">
                  ACCOUNT SECURITY
                </span>
                <h3>Set Employee Password</h3>
              </div>

              <button
                type="button"
                className="management-modal-close"
                onClick={closePasswordForm}
                aria-label="Close form"
              >
                <X size={20} />
              </button>
            </div>

            <div className="employee-selected-user">
              <strong>{selectedEmployee.fullName}</strong>
              <span>{selectedEmployee.email}</span>
            </div>

            <form
              className="employee-management-form"
              onSubmit={handleSetPassword}
            >
              <label htmlFor="employee-new-password">
                New Password
              </label>
              <input
                id="employee-new-password"
                type="password"
                placeholder="Enter a strong password"
                minLength={12}
                maxLength={128}
                required
                autoComplete="new-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
              />

              <label htmlFor="employee-confirm-password">
                Confirm Password
              </label>
              <input
                id="employee-confirm-password"
                type="password"
                placeholder="Enter the password again"
                minLength={12}
                maxLength={128}
                required
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(event) =>
                  setConfirmPassword(event.target.value)
                }
              />

              <div className="employee-form-note">
                Password must contain between 12 and 128 characters.
                The employee can use it to sign in after it is set.
              </div>

              <div className="employee-form-actions">
                <button
                  type="button"
                  className="employee-cancel-button"
                  onClick={closePasswordForm}
                  disabled={savingPassword}
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="employee-add-button"
                  disabled={savingPassword}
                >
                  {savingPassword ? 'Saving...' : 'Save Password'}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
    </div>
  )
}
