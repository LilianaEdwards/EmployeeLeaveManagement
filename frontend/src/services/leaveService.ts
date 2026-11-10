import axios from 'axios'
import type {
  LeaveBalance,
  LeaveRequest,
  LeaveType,
} from '../types/leave'

const leaveApi = axios.create({
  baseURL: '/leave-api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
})

leaveApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('accessToken')

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

export interface CreateLeaveRequest {
  type: LeaveType
  startDate: string
  endDate: string
  reason: string
}

export type ReviewDecision = 1 | 2

export interface ReviewLeaveRequest {
  decision: ReviewDecision
  comment?: string
}

export interface LeaveRequestFilters {
  employeeId?: string
  status?: 0 | 1 | 2
}

export const leaveService = {
  async getMyBalances(employeeId: string): Promise<LeaveBalance[]> {
    const response = await leaveApi.get<LeaveBalance[]>(
      `/LeaveRequests/${employeeId}/balance`,
    )

    return response.data
  },

  async getMyRequests(): Promise<LeaveRequest[]> {
    const response = await leaveApi.get<LeaveRequest[]>(
      '/LeaveRequests',
    )

    return response.data
  },

  async createRequest(
    request: CreateLeaveRequest,
  ): Promise<LeaveRequest> {
    const response = await leaveApi.post<LeaveRequest>(
      '/LeaveRequests',
      request,
    )

    return response.data
  },

  async getManagementRequests(
    filters: LeaveRequestFilters = {},
  ): Promise<LeaveRequest[]> {
    const response = await leaveApi.get<LeaveRequest[]>(
      '/LeaveRequests',
      { params: filters },
    )

    return response.data
  },

  async reviewRequest(
    requestId: string,
    request: ReviewLeaveRequest,
  ): Promise<LeaveRequest> {
    const response = await leaveApi.patch<LeaveRequest>(
      `/LeaveRequests/${requestId}/review`,
      request,
    )

    return response.data
  },
}
