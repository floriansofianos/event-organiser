import apiClient from './client'

export interface CreateEventRequest {
  name: string
  date: string
  description?: string
  location?: string
}

export interface UpdateEventRequest {
  name: string
  date: string
  description?: string
  location?: string
}

export interface EventDto {
  id: string
  name: string
  date: string
  description?: string
  location?: string
  status: 'Active' | 'Cancelled'
  createdAt: string
}

export const createEvent = (data: CreateEventRequest) =>
  apiClient.post<EventDto>('/api/events', data)

export const getEvents = () =>
  apiClient.get<EventDto[]>('/api/events')

export const updateEvent = (id: string, data: UpdateEventRequest) =>
  apiClient.put<EventDto>(`/api/events/${id}`, data)

export const cancelEvent = (id: string) =>
  apiClient.post(`/api/events/${id}/cancel`)
