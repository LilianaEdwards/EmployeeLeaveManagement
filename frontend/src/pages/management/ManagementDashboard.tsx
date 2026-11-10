
import ApplyLeaveForm from '../employee/ApplyLeaveForm'
import './ManagementDashboard.css'
import { useCallback, useEffect, useState } from 'react'
import EmployeeManagement from './EmployeeManagement'
import {
  CalendarDays,
  CheckCircle2,
  Clock3,
  FileText,
  LogOut,
  Plus,
  RefreshCw,
  Search,
  ShieldCheck,
  Users,
  XCircle,
} from 'lucide-react'
import { leaveService } from '../../services/leaveService'
import type {
  LeaveBalance,
  LeaveRequest,
} from '../../types/leave'
import {
  leaveStatusLabels,
  leaveTypeLabels,
} from '../../types/leave'

interface ManagementDashboardProps {
  user: {
    employeeId: string
    fullName: string
    email: string
    role: 'HOD' | 'HR'
    expiresAtUtc: string
  }
  onLogout: () => void
}

type Tab = 'overview' | 'requests' | 'my-leave' | 'employees'
type StatusFilter = 'all' | '0' | '1' | '2'

function formatDate(value: string) {
  // DateOnly values arrive as YYYY-MM-DD.
  // Parse them as local dates to prevent timezone shifts.
  const dateOnly = /^\d{4}-\d{2}-\d{2}$/.test(value)

  const date = dateOnly
    ? (() => {
        const [year, month, day] = value.split('-').map(Number)
        return new Date(year, month - 1, day)
      })()
    : new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleDateString(undefined, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

function getErrorMessage(error: unknown) {
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

export default function ManagementDashboard({
  user,
  onLogout,
}: ManagementDashboardProps) {
  const [activeTab, setActiveTab] = useState<Tab>('overview')
  const [requests, setRequests] = useState<LeaveRequest[]>([])
  const [balances, setBalances] = useState<LeaveBalance[]>([])
  const [myRequests, setMyRequests] = useState<LeaveRequest[]>([])
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all')
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [actionError, setActionError] = useState('')
  const [reviewingId, setReviewingId] = useState<string | null>(null)
  const [reviewComment, setReviewComment] = useState('')
  const [selectedRequest, setSelectedRequest] =
    useState<LeaveRequest | null>(null)
  const [showApplyForm, setShowApplyForm] = useState(false)

  // Load company-wide requests and the signed-in manager's own balances.
  const loadData = useCallback(async () => {
    setLoading(true)
    setError('')

    try {
      const [allRequests, myBalances] = await Promise.all([
        leaveService.getManagementRequests(),
        leaveService.getMyBalances(user.employeeId),
      ])

      setRequests(allRequests)
      setBalances(myBalances)

      // Management users receive all requests from this endpoint.
      // Filter by the signed-in user's ID for their personal history.
      setMyRequests(
        allRequests.filter(
          (request) =>
            request.employeeId.toLowerCase() ===
            user.employeeId.toLowerCase(),
        ),
      )
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [user.employeeId])

  useEffect(() => {
    void loadData()
  }, [loadData])

  const pendingCount = requests.filter(
    (request) => request.status === 0,
  ).length

  const approvedCount = requests.filter(
    (request) => request.status === 1,
  ).length

  const rejectedCount = requests.filter(
    (request) => request.status === 2,
  ).length

  const filteredRequests = requests.filter((request) => {
    const matchesStatus =
      statusFilter === 'all' ||
      request.status === Number(statusFilter)

    const searchTerm = search.trim().toLowerCase()

    const matchesSearch =
      !searchTerm ||
      request.employeeId.toLowerCase().includes(searchTerm) ||
      request.reason.toLowerCase().includes(searchTerm) ||
      leaveTypeLabels[request.type].toLowerCase().includes(searchTerm)

    return matchesStatus && matchesSearch
  })

  async function handleReview(
    requestId: string,
    decision: 1 | 2,
  ) {
    setActionError('')

    const confirmed = window.confirm(
      decision === 1
        ? 'Are you sure you want to approve this leave request?'
        : 'Are you sure you want to reject this leave request?',
    )

    if (!confirmed) return

    setReviewingId(requestId)

    try {
      await leaveService.reviewRequest(requestId, {
        decision,
        comment: reviewComment.trim() || undefined,
      })

      setReviewComment('')
      setSelectedRequest(null)

      await loadData()
    } catch (err) {
      setActionError(getErrorMessage(err))
    } finally {
      setReviewingId(null)
    }
  }

  function renderRequestTable(items: LeaveRequest[]) {
    if (items.length === 0) {
      return (
        <div className="management-empty">
          <FileText size={30} />
          <h3>No leave requests found</h3>
          <p>There are no requests matching your current filter.</p>
        </div>
      )
    }

    return (
      <div className="management-table-wrapper">
        <table className="management-table">
          <thead>
            <tr>
              <th>Employee ID</th>
              <th>Leave Type</th>
              <th>Leave Period</th>
              <th>Submitted</th>
              <th>Status</th>
              <th>Action</th>
            </tr>
          </thead>

          <tbody>
            {items.map((request) => (
              <tr key={request.id}>
                <td>
                  <span className="employee-id">
                    {request.employeeId.slice(0, 8)}...
                  </span>
                </td>

                <td>{leaveTypeLabels[request.type]}</td>

                <td>
                  {formatDate(request.startDate)}
                  <span className="date-separator"> - </span>
                  {formatDate(request.endDate)}
                </td>

                <td>{formatDate(request.createdAtUtc)}</td>

                <td>
                  <span className={`status-badge status-${request.status}`}>
                    {leaveStatusLabels[request.status]}
                  </span>
                </td>

                <td>
                  <button
                    className="management-view-button"
                    onClick={() => {
                      setReviewComment('')
                      setActionError('')
                      setSelectedRequest(request)
                    }}
                  >
                    View details
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )
  }

  const isOwnSelectedRequest =
    selectedRequest?.employeeId.toLowerCase() ===
    user.employeeId.toLowerCase()

  return (
    <main className="management-page">
      <aside className="management-sidebar">
        <div className="management-brand">
          <div className="management-brand-icon">
            <CalendarDays size={23} />
          </div>
          <div>
            <h1>LeaveFlow</h1>
            <p>Management Portal</p>
          </div>
        </div>

        <div className="management-nav-label">WORKSPACE</div>

        <nav className="management-nav">
          <button
            className={activeTab === 'overview' ? 'active' : ''}
            onClick={() => setActiveTab('overview')}
          >
            <CalendarDays size={18} />
            Overview
          </button>

          <button
            className={activeTab === 'requests' ? 'active' : ''}
            onClick={() => setActiveTab('requests')}
          >
            <FileText size={18} />
            Leave Requests
            {pendingCount > 0 && (
              <span className="management-nav-count">
                {pendingCount}
              </span>
            )}
          </button>

          <button
            className={activeTab === 'my-leave' ? 'active' : ''}
            onClick={() => setActiveTab('my-leave')}
          >
            <Users size={18} />
            My Leave
          </button>
          {user.role === 'HOD' && (
              <button
                className={activeTab === 'employees' ? 'active' : ''}
                onClick={() => setActiveTab('employees')}
              >
                <Users size={18} />
                Employees
              </button>
)}
        </nav>

        <div className="management-sidebar-bottom">
          <div className="management-user">
            <div className="management-avatar">
              {user.fullName.charAt(0).toUpperCase()}
            </div>
            <div>
              <strong>{user.fullName}</strong>
              <span>{user.role}</span>
            </div>
          </div>

          <button
            className="management-logout"
            onClick={onLogout}
          >
            <LogOut size={17} />
            Sign out
          </button>
        </div>
      </aside>

      <section className="management-main">
        <header className="management-topbar">
          <div>
            <span className="management-eyebrow">
              {user.role} WORKSPACE
            </span>
            <h2>
              {activeTab === 'overview'
                  ? 'Dashboard Overview'
                  : activeTab === 'requests'
                    ? 'Leave Requests'
                    : activeTab === 'my-leave'
                      ? 'My Leave'
                      : 'Employee Management'}
            </h2>
          </div>

          <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
            <button
              className="management-refresh"
              onClick={() => setShowApplyForm(true)}
            >
              <Plus size={16} />
              Apply for Leave
            </button>

            <button
              className="management-refresh"
              onClick={() => void loadData()}
              disabled={loading}
            >
              <RefreshCw size={16} />
              Refresh
            </button>
          </div>
        </header>

        <div className="management-content">
          {error && (
            <div className="management-alert error">
              {error}
              <button onClick={() => void loadData()}>
                Retry
              </button>
            </div>
          )}

          {actionError && (
            <div className="management-alert error">
              {actionError}
            </div>
          )}

          {loading ? (
            <div className="management-loading">
              <RefreshCw size={23} className="management-spin" />
              <p>Loading management data...</p>
            </div>
          ) : (
            <>
              {activeTab === 'overview' && (
                <>
                  <div className="management-welcome">
                    <div>
                      <h3>Welcome back, {user.fullName}!</h3>
                      <p>
                        Here is the current leave activity across your organization.
                      </p>
                    </div>
                    <div className="management-welcome-icon">
                      <ShieldCheck size={31} />
                    </div>
                  </div>

                  <div className="management-stats">
                    <div className="management-stat-card">
                      <div className="stat-icon blue">
                        <FileText size={20} />
                      </div>
                      <span>Total Requests</span>
                      <strong>{requests.length}</strong>
                      <small>All leave requests</small>
                    </div>

                    <div className="management-stat-card">
                      <div className="stat-icon orange">
                        <Clock3 size={20} />
                      </div>
                      <span>Pending</span>
                      <strong>{pendingCount}</strong>
                      <small>Awaiting review</small>
                    </div>

                    <div className="management-stat-card">
                      <div className="stat-icon green">
                        <CheckCircle2 size={20} />
                      </div>
                      <span>Approved</span>
                      <strong>{approvedCount}</strong>
                      <small>Approved requests</small>
                    </div>

                    <div className="management-stat-card">
                      <div className="stat-icon red">
                        <XCircle size={20} />
                      </div>
                      <span>Rejected</span>
                      <strong>{rejectedCount}</strong>
                      <small>Rejected requests</small>
                    </div>
                  </div>

                  <section className="management-panel">
                    <div className="management-panel-heading">
                      <div>
                        <h3>Recent Leave Requests</h3>
                        <p>Latest submissions from employees</p>
                      </div>

                      <button
                        className="management-text-button"
                        onClick={() => setActiveTab('requests')}
                      >
                        View all
                      </button>
                    </div>

                    {renderRequestTable(requests.slice(0, 5))}
                  </section>
                </>
              )}

              {activeTab === 'requests' && (
                <section className="management-panel">
                  <div className="management-panel-heading">
                    <div>
                      <h3>All Leave Requests</h3>
                      <p>Review and manage employee leave applications</p>
                    </div>
                  </div>

                  <div className="management-filters">
                    <div className="management-search">
                      <Search size={17} />
                      <input
                        placeholder="Search employee ID or reason..."
                        value={search}
                        onChange={(event) => setSearch(event.target.value)}
                      />
                    </div>

                    <select
                      value={statusFilter}
                      onChange={(event) =>
                        setStatusFilter(event.target.value as StatusFilter)
                      }
                    >
                      <option value="all">All statuses</option>
                      <option value="0">Pending</option>
                      <option value="1">Approved</option>
                      <option value="2">Rejected</option>
                    </select>
                  </div>

                  {renderRequestTable(filteredRequests)}
                </section>
              )}

              {activeTab === 'my-leave' && (
                <>
                  <section className="management-panel">
                    <div className="management-panel-heading">
                      <div>
                        <h3>My Leave Balances</h3>
                        <p>Your available leave allowances</p>
                      </div>

                      <button
                        className="management-text-button"
                        onClick={() => setShowApplyForm(true)}
                      >
                        <Plus size={15} />
                        Apply for Leave
                      </button>
                    </div>

                    <div className="management-balance-grid">
                      {balances.map((balance) => (
                        <div
                          className="management-balance-card"
                          key={balance.type}
                        >
                          <span>{leaveTypeLabels[balance.type]}</span>
                          <strong>{balance.remainingDays}</strong>
                          <small>
                            Remaining of {balance.allowanceDays} days
                          </small>
                          <div className="management-balance-track">
                            <div
                              style={{
                                width: `${
                                  balance.allowanceDays > 0
                                    ? Math.min(
                                        100,
                                        (balance.usedDays /
                                          balance.allowanceDays) *
                                          100,
                                      )
                                    : 0
                                }%`,
                              }}
                            />
                          </div>
                          <small>
                            {balance.usedDays} days used - {balance.period}
                          </small>
                        </div>
                      ))}
                    </div>
                  </section>

                  <section className="management-panel">
                    <div className="management-panel-heading">
                      <div>
                        <h3>My Leave History</h3>
                        <p>Your own submitted leave requests</p>
                      </div>
                    </div>

                    {renderRequestTable(myRequests)}
                  </section>
                </>
              )}
              {activeTab === 'employees' && user.role === 'HOD' && (
                <EmployeeManagement />
              )}
            </>
          )}
        </div>
      </section>

      {selectedRequest && (
        <div
          className="management-modal-backdrop"
          onClick={() => setSelectedRequest(null)}
        >
          <section
            className="management-modal"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="management-modal-heading">
              <div>
                <span className="management-eyebrow">REQUEST DETAILS</span>
                <h3>Leave Application</h3>
              </div>

              <button
                className="management-modal-close"
                onClick={() => setSelectedRequest(null)}
                aria-label="Close details"
              >
                X
              </button>
            </div>

            <div className="management-detail-grid">
              <div>
                <span>Employee ID</span>
                <strong>{selectedRequest.employeeId}</strong>
              </div>
              <div>
                <span>Leave Type</span>
                <strong>{leaveTypeLabels[selectedRequest.type]}</strong>
              </div>
              <div>
                <span>Start Date</span>
                <strong>{formatDate(selectedRequest.startDate)}</strong>
              </div>
              <div>
                <span>End Date</span>
                <strong>{formatDate(selectedRequest.endDate)}</strong>
              </div>
              <div>
                <span>Submitted</span>
                <strong>{formatDate(selectedRequest.createdAtUtc)}</strong>
              </div>
              <div>
                <span>Status</span>
                <strong>
                  {leaveStatusLabels[selectedRequest.status]}
                </strong>
              </div>
            </div>

            <div className="management-reason">
              <span>Reason for leave</span>
              <p>{selectedRequest.reason}</p>
            </div>

            {selectedRequest.reviewComment && (
              <div className="management-reason">
                <span>Review comment</span>
                <p>{selectedRequest.reviewComment}</p>
              </div>
            )}

            {selectedRequest.status === 0 && !isOwnSelectedRequest && (
              <>
                <div className="management-comment">
                  <label htmlFor="review-comment">
                    Review comment (optional)
                  </label>
                  <textarea
                    id="review-comment"
                    rows={3}
                    maxLength={500}
                    placeholder="Enter a comment for this decision..."
                    value={reviewComment}
                    onChange={(event) =>
                      setReviewComment(event.target.value)
                    }
                  />
                </div>

                <div className="management-review-actions">
                  <button
                    className="management-reject"
                    disabled={reviewingId === selectedRequest.id}
                    onClick={() =>
                      void handleReview(selectedRequest.id, 2)
                    }
                  >
                    <XCircle size={17} />
                    Reject
                  </button>

                  <button
                    className="management-approve"
                    disabled={reviewingId === selectedRequest.id}
                    onClick={() =>
                      void handleReview(selectedRequest.id, 1)
                    }
                  >
                    <CheckCircle2 size={17} />
                    Approve
                  </button>
                </div>
              </>
            )}

            {selectedRequest.status === 0 && isOwnSelectedRequest && (
              <p className="management-alert error">
                You cannot review your own leave request.
              </p>
            )}
          </section>
        </div>
      )}

      {showApplyForm && (
        <ApplyLeaveForm
          onClose={() => setShowApplyForm(false)}
          onSuccess={() => {
            setShowApplyForm(false)
            void loadData()
          }}
        />
      )}
    </main>
  )
}
