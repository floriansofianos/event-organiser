import axios from 'axios'

export function getErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const detail = err.response?.data?.detail as string | undefined
    if (detail) return detail

    const errors = err.response?.data?.errors as Record<string, string[]> | undefined
    if (errors) return Object.values(errors).flat().join(' ')

    return err.message
  }
  return 'An unexpected error occurred.'
}
