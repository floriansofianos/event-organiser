import { createContext, useContext, useState, useEffect, type ReactNode } from 'react'
import type { AuthResponse } from '../api/auth'

interface AuthUser {
  token: string
  email: string
  firstName: string
  lastName: string
}

interface AuthContextValue {
  user: AuthUser | null
  login: (response: AuthResponse) => void
  logout: () => void
  isAuthenticated: boolean
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    try {
      const stored = localStorage.getItem('auth')
      return stored ? (JSON.parse(stored) as AuthUser) : null
    } catch {
      return null
    }
  })

  useEffect(() => {
    if (user) {
      localStorage.setItem('auth', JSON.stringify(user))
      localStorage.setItem('token', user.token)
    } else {
      localStorage.removeItem('auth')
      localStorage.removeItem('token')
    }
  }, [user])

  const login = (response: AuthResponse) => {
    setUser({ token: response.token, email: response.email, firstName: response.firstName, lastName: response.lastName })
  }

  const logout = () => setUser(null)

  useEffect(() => {
    const handleUnauthorized = () => setUser(null)
    window.addEventListener('auth:unauthorized', handleUnauthorized)
    return () => window.removeEventListener('auth:unauthorized', handleUnauthorized)
  }, [])

  return (
    <AuthContext.Provider value={{ user, login, logout, isAuthenticated: user !== null }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
