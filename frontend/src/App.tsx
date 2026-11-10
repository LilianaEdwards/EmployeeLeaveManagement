
import ManagementDashboard from './pages/management/ManagementDashboard'
import EmployeeDashboard from './pages/employee/EmployeeDashboard'
import { useState } from 'react'
import {
  ArrowRight,
  CalendarDays,
  CheckCircle2,
  Eye,
  EyeOff,
  LockKeyhole,
  Mail,
  ShieldCheck,
  Users,
} from 'lucide-react'
import { useNavigate, Navigate, Route, Routes } from 'react-router-dom'
import axios from 'axios'
import { authService } from './services/authService'
import type { LoginResponse } from './services/authService'
import './App.css'

type Portal = 'employee' | 'management'

function LoginPage() {
  const navigate = useNavigate()

  const [portal, setPortal] = useState<Portal>('employee')
  const [showPassword, setShowPassword] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setMessage('')
    setLoading(true)

    try {
      const user = await authService.login({
        email: email.trim(),
        password,
      })

      const isEmployee = user.role === 'Employee'
      const isManagement = user.role === 'HOD' || user.role === 'HR'

      if (
        (portal === 'employee' && !isEmployee) ||
        (portal === 'management' && !isManagement)
      ) {
        setMessage(
          portal === 'employee'
            ? 'This is a management account. Please select the Management portal.'
            : 'This is an employee account. Please select the Employee portal.',
        )
        return
      }

      sessionStorage.setItem('accessToken', user.accessToken)

      sessionStorage.setItem(
        'user',
        JSON.stringify({
          employeeId: user.employeeId,
          fullName: user.fullName,
          email: user.email,
          role: user.role,
          expiresAtUtc: user.expiresAtUtc,
        }),
      )

      if (user.role === 'Employee') {
        navigate('/employee/dashboard', { replace: true })
      } else {
        navigate('/management/dashboard', { replace: true })
      }
    } catch (error: unknown) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 401) {
          setMessage('Invalid email or password. Please try again.')
        } else if (error.response?.status === 400) {
          setMessage('Please check your login information.')
        } else if (error.response) {
          setMessage('Unable to sign in. Please try again later.')
        } else {
          setMessage(
            'Cannot connect to the server. Check that the backend is running.',
          )
        }
      } else {
        setMessage('An unexpected error occurred. Please try again.')
      }
    } finally {
      setLoading(false)
    }
  }

  function handlePortalChange(nextPortal: Portal) {
    setPortal(nextPortal)
    setMessage('')
  }

  return (
    <main className="login-page">
      <section className="login-left">
        <header className="brand">
          <div className="brand-icon">
            <CalendarDays size={25} />
          </div>
          <div>
            <h1>LeaveFlow</h1>
            <p>Employee Leave Management</p>
          </div>
        </header>

        <div className="welcome-content">
          <div className="welcome-tag">
            <ShieldCheck size={16} />
            <span>Smart leave management</span>
          </div>

          <h2>
            Manage your time.
            <br />
            <span>Focus on what matters.</span>
          </h2>

          <p className="welcome-description">
            A simpler way to request, manage, and approve employee leave.
            Everything your team needs, all in one place.
          </p>

          <div className="feature-list">
            <div className="feature-item">
              <CheckCircle2 size={19} />
              <span>Easy leave requests and tracking</span>
            </div>
            <div className="feature-item">
              <CheckCircle2 size={19} />
              <span>Transparent leave balances</span>
            </div>
            <div className="feature-item">
              <CheckCircle2 size={19} />
              <span>Streamlined approval workflows</span>
            </div>
          </div>
        </div>

        <footer className="left-footer">
          © 2026 LeaveFlow. Employee Leave Management System.
        </footer>
      </section>

      <section className="login-right">
        <div className="login-card">
          <div className="mobile-brand">
            <div className="brand-icon">
              <CalendarDays size={23} />
            </div>
            <span>LeaveFlow</span>
          </div>

          <div className="login-heading">
            <span className="eyebrow">WELCOME BACK</span>
            <h2>Sign in to your account</h2>
            <p>Choose your portal and enter your account details.</p>
          </div>

          <div className="portal-selector">
            <button
              type="button"
              className={`portal-option ${portal === 'employee' ? 'active' : ''}`}
              onClick={() => handlePortalChange('employee')}
              aria-pressed={portal === 'employee'}
            >
              <Users size={18} />
              <span>Employee</span>
            </button>

            <button
              type="button"
              className={`portal-option ${portal === 'management' ? 'active' : ''}`}
              onClick={() => handlePortalChange('management')}
              aria-pressed={portal === 'management'}
            >
              <ShieldCheck size={18} />
              <span>Management</span>
            </button>
          </div>

          <div className="portal-description">
            {portal === 'employee'
              ? 'Sign in to submit leave requests and view your leave balance.'
              : 'Sign in to manage employee leave requests and approvals.'}
          </div>

          <form onSubmit={handleSubmit} className="login-form">
            <div className="form-field">
              <label htmlFor="email">Email address</label>
              <div className="input-wrapper">
                <Mail size={18} />
                <input
                  id="email"
                  type="email"
                  placeholder="you@company.com"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  autoComplete="username"
                  required
                />
              </div>
            </div>

            <div className="form-field">
              <label htmlFor="password">Password</label>

              <div className="input-wrapper">
                <LockKeyhole size={18} />
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Enter your password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  autoComplete="current-password"
                  required
                />
                <button
                  type="button"
                  className="password-toggle"
                  onClick={() => setShowPassword(!showPassword)}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="sign-in-button"
              disabled={loading}
            >
              {loading ? 'Signing in...' : 'Sign in'}
              {!loading && <ArrowRight size={18} />}
            </button>

            {message && (
              <p className="form-message" role="alert">
                {message}
              </p>
            )}
          </form>

          <div className="security-note">
            <LockKeyhole size={15} />
            <span>Your account information is securely protected.</span>
          </div>
        </div>

        <p className="right-footer">
          Need help accessing your account? Contact your administrator.
        </p>
      </section>
    </main>
  )
}

interface StoredUser {
  employeeId: string
  fullName: string
  email: string
  role: LoginResponse['role']
  expiresAtUtc: string
}

function getStoredUser(): StoredUser | null {
  const token = sessionStorage.getItem('accessToken')
  const storedUser = sessionStorage.getItem('user')

  if (!token || !storedUser) {
    return null
  }

  try {
    const user = JSON.parse(storedUser) as StoredUser

    if (new Date(user.expiresAtUtc).getTime() <= Date.now()) {
      sessionStorage.removeItem('accessToken')
      sessionStorage.removeItem('user')
      return null
    }

    return user
  } catch {
    sessionStorage.removeItem('accessToken')
    sessionStorage.removeItem('user')
    return null
  }
}

function ProtectedDashboard({
  expectedRole,
}: {
  expectedRole: 'Employee' | 'Management'
}) {
  const navigate = useNavigate()
  const user = getStoredUser()

  if (!user) {
    return <Navigate to="/" replace />
  }

  const isManagement = user.role === 'HOD' || user.role === 'HR'

  if (
    (expectedRole === 'Employee' && user.role !== 'Employee') ||
    (expectedRole === 'Management' && !isManagement)
  ) {
    return (
      <Navigate
        to={isManagement ? '/management/dashboard' : '/employee/dashboard'}
        replace
      />
    )
  }

  function handleLogout() {
    authService.logout()
    navigate('/', { replace: true })
  }

  if (expectedRole === 'Employee') {
    return (
      <EmployeeDashboard
        user={user}
        onLogout={handleLogout}
      />
    )
  }

  if (user.role !== 'HOD' && user.role !== 'HR') {
    return <Navigate to="/" replace />
  }

  return (
    <ManagementDashboard
      user={{
        ...user,
        role: user.role,
      }}
      onLogout={handleLogout}
    />
  )
}

function App() {
  return (
    <Routes>
      <Route path="/" element={<LoginPage />} />
      <Route
        path="/employee/dashboard"
        element={<ProtectedDashboard expectedRole="Employee" />}
      />
      <Route
        path="/management/dashboard"
        element={<ProtectedDashboard expectedRole="Management" />}
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
