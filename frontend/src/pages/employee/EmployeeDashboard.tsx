import ApplyLeaveForm from './ApplyLeaveForm'
import { useEffect, useState } from 'react'
import {
  CalendarDays,
  CheckCircle2,
  Clock3,
  FileText,
  LogOut,
  Plus,
  UserRound,
  RefreshCw,
} from 'lucide-react'

import { leaveService } from '../../services/leaveService'
import type {
  LeaveBalance,
  LeaveRequest,
  LeaveType,
  LeaveStatus,
} from '../../types/leave'

interface EmployeeDashboardProps {
  user: {
    employeeId: string
    fullName: string
    email: string
    role: 'Employee' | 'HOD' | 'HR'
    expiresAtUtc: string
  }
  onLogout: () => void
}

const leaveTypeNames: Record<LeaveType, string> = {
  0: 'Annual Leave',
  1: 'Sick Leave',
  2: 'Paid Leave',
  3: 'Unpaid Leave',
}

const leaveStatusNames: Record<LeaveStatus, string> = {
  0: 'Pending',
  1: 'Approved',
  2: 'Rejected',
}

const leaveColors: Record<LeaveType, string> = {
  0: 'bg-blue-500',
  1: 'bg-emerald-500',
  2: 'bg-violet-500',
  3: 'bg-amber-500',
}

const statusColors: Record<LeaveStatus, string> = {
  0: 'bg-amber-50 text-amber-700',
  1: 'bg-emerald-50 text-emerald-700',
  2: 'bg-red-50 text-red-700',
}

const leaveIcons = [
  CalendarDays,
  Clock3,
  CheckCircle2,
  FileText,
]

function formatDate(date: string): string {
  return new Date(`${date}T00:00:00`).toLocaleDateString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })
}

function formatDateTime(date: string): string {
  return new Date(date).toLocaleString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function EmployeeDashboard({
  user,
  onLogout,
}: EmployeeDashboardProps) {
  const [balances, setBalances] = useState<LeaveBalance[]>([])
  const [requests, setRequests] = useState<LeaveRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showApplyForm, setShowApplyForm] = useState(false)

  async function loadDashboard() {
    setLoading(true)
    setError('')

    try {
      const [balanceData, requestData] = await Promise.all([
        leaveService.getMyBalances(user.employeeId),
        leaveService.getMyRequests(),
      ])

      setBalances(balanceData)
      setRequests(requestData)
    } catch (err: unknown) {
      console.error('Dashboard loading error:', err)

      setError(
        'Unable to load your leave information. Please check the backend connection and try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadDashboard()
  }, [user.employeeId])

  const summaryCards = [
    {
      type: 0 as LeaveType,
      subtitle: 'Days available this year',
    },
    {
      type: 1 as LeaveType,
      subtitle: 'Days available this year',
    },
    {
      type: 2 as LeaveType,
      subtitle: 'Days available this year',
    },
    {
      type: 3 as LeaveType,
      subtitle: 'Days available this month',
    },
  ]

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">
          <div>
            <h1 className="text-xl font-bold text-slate-900">
              Leave Management
            </h1>
            <p className="text-xs text-slate-500">Employee Portal</p>
          </div>

          <button
            onClick={onLogout}
            className="flex items-center gap-2 rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-50"
          >
            <LogOut size={16} />
            Sign Out
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-6 py-8">
        <section className="mb-8">
          <p className="text-sm font-medium text-blue-600">
            EMPLOYEE WORKSPACE
          </p>

          <h2 className="mt-2 text-3xl font-bold text-slate-900">
            Welcome, {user.fullName}
          </h2>

          <p className="mt-2 text-slate-500">
            Manage your leave requests and track your leave balances.
          </p>
        </section>

        {error && (
          <div className="mb-6 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">
            <p>{error}</p>

            <button
              onClick={() => void loadDashboard()}
              className="flex items-center gap-2 font-semibold"
            >
              <RefreshCw size={15} />
              Retry
            </button>
          </div>
        )}

        <section className="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {summaryCards.map((card, index) => {
            const balance = balances.find(
              (item) => item.type === card.type,
            )

            const Icon = leaveIcons[index]

            return (
              <div
                key={card.type}
                className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"
              >
                <div className="flex items-center justify-between">
                  <p className="text-sm font-medium text-slate-500">
                    {leaveTypeNames[card.type]}
                  </p>

                  <div className="rounded-xl bg-slate-100 p-2 text-slate-600">
                    <Icon size={20} />
                  </div>
                </div>

                <p className="mt-5 text-3xl font-bold text-slate-900">
                  {loading ? 'â€”' : balance?.remainingDays ?? 'â€”'}
                </p>

                <p className="mt-1 text-xs text-slate-500">
                  {card.subtitle}
                </p>
              </div>
            )
          })}
        </section>

        <section className="grid gap-6 lg:grid-cols-3">
          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm lg:col-span-2">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-lg font-bold text-slate-900">
                  Leave Balance
                </h3>

                <p className="mt-1 text-sm text-slate-500">
                  Your actual remaining leave allowance
                </p>
              </div>

              <CalendarDays className="text-slate-400" />
            </div>

            <div className="mt-6 space-y-6">
              {loading ? (
                <p className="py-6 text-center text-sm text-slate-400">
                  Loading leave balances...
                </p>
              ) : balances.length === 0 ? (
                <p className="py-6 text-center text-sm text-slate-400">
                  No balance information available.
                </p>
              ) : (
                balances.map((leave) => {
                  const percentage =
                    leave.allowanceDays > 0
                      ? (leave.remainingDays / leave.allowanceDays) * 100
                      : 0

                  return (
                    <div key={leave.type}>
                      <div className="mb-2 flex flex-wrap justify-between gap-2 text-sm">
                        <span className="font-medium text-slate-700">
                          {leaveTypeNames[leave.type]}
                        </span>

                        <span className="text-slate-500">
                          {leave.remainingDays} / {leave.allowanceDays} days
                          available
                        </span>
                      </div>

                      <div className="h-2 overflow-hidden rounded-full bg-slate-100">
                        <div
                          className={`h-full rounded-full ${leaveColors[leave.type]}`}
                          style={{ width: `${percentage}%` }}
                        />
                      </div>

                      <p className="mt-1 text-xs text-slate-400">
                        {leave.usedDays} days used Â· Period: {leave.period}
                      </p>
                    </div>
                  )
                })
              )}
            </div>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
            <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-blue-50 text-blue-600">
              <UserRound size={22} />
            </div>

            <h3 className="mt-4 text-lg font-bold text-slate-900">
              My Profile
            </h3>

            <div className="mt-5 space-y-4 text-sm">
              <div>
                <p className="text-slate-400">Full Name</p>
                <p className="mt-1 font-medium text-slate-800">
                  {user.fullName}
                </p>
              </div>

              <div>
                <p className="text-slate-400">Email Address</p>
                <p className="mt-1 break-all font-medium text-slate-800">
                  {user.email}
                </p>
              </div>

              <div>
                <p className="text-slate-400">Account Role</p>
                <p className="mt-1 font-medium text-slate-800">
                  {user.role}
                </p>
              </div>
            </div>
          </div>
        </section>

        <section className="mt-6 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-4">
            <div>
              <h3 className="text-lg font-bold text-slate-900">
                My Leave Requests
              </h3>

              <p className="mt-1 text-sm text-slate-500">
                Track the status of your leave applications.
              </p>
            </div>

            <button
  onClick={() => setShowApplyForm(true)}
  className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-blue-700"
>
              <Plus size={17} />
              Apply for Leave
            </button>
          </div>

          <div className="mt-6 overflow-x-auto">
            {loading ? (
              <p className="py-8 text-center text-sm text-slate-400">
                Loading leave requests...
              </p>
            ) : requests.length === 0 ? (
              <div className="flex flex-col items-center py-8 text-center">
                <div className="rounded-full bg-slate-100 p-4 text-slate-400">
                  <FileText size={28} />
                </div>

                <p className="mt-4 font-medium text-slate-700">
                  No leave requests yet
                </p>

                <p className="mt-1 text-sm text-slate-400">
                  Your submitted applications will appear here.
                </p>
              </div>
            ) : (
              <table className="w-full min-w-[950px] text-left text-sm">
                <thead>
                     <tr className="border-b border-slate-200 text-slate-500">
              <th className="px-4 py-3 font-medium">Leave Type</th>
              <th className="px-4 py-3 font-medium">Start Date</th>
              <th className="px-4 py-3 font-medium">End Date</th>
              <th className="px-4 py-3 font-medium">Reason</th>
              <th className="px-4 py-3 font-medium">Submitted On</th>
              <th className="px-4 py-3 font-medium">Status</th>
           </tr>
                </thead>

                <tbody>
                  {requests.map((request) => (
                    <tr
                      key={request.id}
                      className="border-b border-slate-100 last:border-0"
                    >
                      <td className="px-4 py-4 font-medium text-slate-800">
              {leaveTypeNames[request.type]}
            </td>

            <td className="px-4 py-4 text-slate-600">
              {formatDate(request.startDate)}
            </td>

            <td className="px-4 py-4 text-slate-600">
              {formatDate(request.endDate)}
            </td>

            <td className="max-w-xs px-4 py-4 text-slate-600">
              <span className="line-clamp-2">
                {request.reason}
              </span>
            </td>

            <td className="whitespace-nowrap px-4 py-4 text-slate-600">
              {formatDateTime(request.createdAtUtc)}
            </td>

            <td className="px-4 py-4">
              <span
                className={`rounded-full px-3 py-1 text-xs font-semibold ${statusColors[request.status]}`}
              >
                {leaveStatusNames[request.status]}
              </span>
            </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </section>
    {showApplyForm && (
  <ApplyLeaveForm
    onClose={() => setShowApplyForm(false)}
    onSuccess={() => {
      setShowApplyForm(false)
      void loadDashboard()
    }}
  />
)}
      </main>
    </div>
  )
}

export default EmployeeDashboard
