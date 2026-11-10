
import { useState } from 'react'
import { CalendarDays, Send, X } from 'lucide-react'
import { leaveService } from '../../services/leaveService'
import type { LeaveType } from '../../types/leave'

interface ApplyLeaveFormProps {
  onClose: () => void
  onSuccess: () => void
}

function ApplyLeaveForm({
  onClose,
  onSuccess,
}: ApplyLeaveFormProps) {
  const [type, setType] = useState<LeaveType>(0)
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [reason, setReason] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setError('')

    if (endDate < startDate) {
      setError('End date cannot be earlier than start date.')
      return
    }

    if (reason.trim().length < 5) {
      setError('Please provide a reason of at least 5 characters.')
      return
    }

    if (reason.trim().length > 500) {
      setError('Reason cannot exceed 500 characters.')
      return
    }

    setSubmitting(true)

    try {
      await leaveService.createRequest({
        type,
        startDate,
        endDate,
        reason: reason.trim(),
      })

      onSuccess()
    } catch (err: unknown) {
      console.error('Leave submission error:', err)

      if (
        typeof err === 'object' &&
        err !== null &&
        'response' in err
      ) {
        const response = (
          err as {
            response?: {
              data?: { message?: string; title?: string }
            }
          }
        ).response

        setError(
          response?.data?.message ||
          response?.data?.title ||
          'Unable to submit your leave request. Please check your information.',
        )
      } else {
        setError('Unable to connect to the server. Please try again.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4">
      <div className="w-full max-w-xl rounded-2xl bg-white p-6 shadow-xl">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <div className="flex items-center gap-2 text-blue-600">
              <CalendarDays size={20} />
              <span className="text-sm font-semibold">
                LEAVE APPLICATION
              </span>
            </div>

            <h2 className="mt-2 text-2xl font-bold text-slate-900">
              Apply for Leave
            </h2>

            <p className="mt-1 text-sm text-slate-500">
              Complete the details below to submit your request.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={submitting}
            className="rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700"
            aria-label="Close form"
          >
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label
              htmlFor="leaveType"
              className="mb-2 block text-sm font-medium text-slate-700"
            >
              Leave Type
            </label>

            <select
              id="leaveType"
              value={type}
              onChange={(event) =>
                setType(Number(event.target.value) as LeaveType)
              }
              className="w-full rounded-lg border border-slate-300 px-4 py-3 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
            >
              <option value={0}>Annual Leave (14 days/year)</option>
              <option value={1}>Sick Leave (5 days/year)</option>
              <option value={2}>Paid Leave (5 days/year)</option>
              <option value={3}>Unpaid Leave (5 days/month)</option>
            </select>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label
                htmlFor="startDate"
                className="mb-2 block text-sm font-medium text-slate-700"
              >
                Start Date
              </label>

              <input
                id="startDate"
                type="date"
                value={startDate}
                onChange={(event) => setStartDate(event.target.value)}
                required
                className="w-full rounded-lg border border-slate-300 px-3 py-3 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
              />
            </div>

            <div>
              <label
                htmlFor="endDate"
                className="mb-2 block text-sm font-medium text-slate-700"
              >
                End Date
              </label>

              <input
                id="endDate"
                type="date"
                value={endDate}
                min={startDate || undefined}
                onChange={(event) => setEndDate(event.target.value)}
                required
                className="w-full rounded-lg border border-slate-300 px-3 py-3 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
              />
            </div>
          </div>

          <div>
            <div className="mb-2 flex items-center justify-between">
              <label
                htmlFor="reason"
                className="text-sm font-medium text-slate-700"
              >
                Reason for Leave
              </label>

              <span className="text-xs text-slate-400">
                {reason.length}/500
              </span>
            </div>

            <textarea
              id="reason"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              required
              minLength={5}
              maxLength={500}
              rows={4}
              placeholder="Please explain why you need leave..."
              className="w-full resize-none rounded-lg border border-slate-300 px-4 py-3 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
            />
          </div>

          {error && (
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
              {error}
            </div>
          )}

          <div className="flex justify-end gap-3 border-t border-slate-100 pt-5">
            <button
              type="button"
              onClick={onClose}
              disabled={submitting}
              className="rounded-lg border border-slate-300 px-5 py-2.5 text-sm font-semibold text-slate-600 hover:bg-slate-50 disabled:opacity-50"
            >
              Cancel
            </button>

            <button
              type="submit"
              disabled={submitting}
              className="flex items-center gap-2 rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <Send size={16} />
              {submitting ? 'Submitting...' : 'Submit Request'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

export default ApplyLeaveForm
