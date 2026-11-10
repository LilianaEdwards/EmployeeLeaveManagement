
export type LeaveType = 0 | 1 | 2 | 3

export type LeaveStatus = 0 | 1 | 2

export interface LeaveBalance {
  type: LeaveType
  allowanceDays: number
  usedDays: number
  remainingDays: number
  period: string
}

export interface LeaveRequest {
  id: string
  employeeId: string
  type: LeaveType
  startDate: string
  endDate: string
  reason: string
  status: LeaveStatus
  createdAtUtc: string
  reviewedAtUtc: string | null
  reviewedByEmployeeId: string | null
  reviewComment: string | null
}

export const leaveTypeLabels: Record<LeaveType, string> = {
  0: 'Annual Leave',
  1: 'Sick Leave',
  2: 'Paid Leave',
  3: 'Unpaid Leave',
}

export const leaveStatusLabels: Record<LeaveStatus, string> = {
  0: 'Pending',
  1: 'Approved',
  2: 'Rejected',
}
