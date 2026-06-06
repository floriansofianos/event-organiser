import { useState } from 'react'
import { Link as RouterLink } from 'react-router-dom'
import {
  Box, Button, Container, Link, Paper, TextField, Typography, Alert,
} from '@mui/material'
import { register } from '../api/auth'
import { getErrorMessage } from '../utils/error'

export default function RegisterPage() {
  const [form, setForm] = useState({
    firstName: '', lastName: '', email: '', password: '', confirmPassword: '',
  })
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [loading, setLoading] = useState(false)

  const set = (field: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((f) => ({ ...f, [field]: e.target.value }))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await register(form)
      setSuccess(true)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  if (success) {
    return (
      <Container maxWidth="xs">
        <Box sx={{ mt: 8 }}>
          <Paper elevation={3} sx={{ p: 4, textAlign: 'center' }}>
            <Typography variant="h5" gutterBottom fontWeight={700}>Check your email</Typography>
            <Typography color="text.secondary">
              We sent a confirmation link to <strong>{form.email}</strong>.
              Click the link in the email to activate your account.
            </Typography>
            <Link component={RouterLink} to="/login" sx={{ display: 'block', mt: 3 }}>
              Back to Sign In
            </Link>
          </Paper>
        </Box>
      </Container>
    )
  }

  return (
    <Container maxWidth="xs">
      <Box sx={{ mt: 8 }}>
        <Paper elevation={3} sx={{ p: 4 }}>
          <Typography variant="h5" align="center" gutterBottom fontWeight={700}>
            Create Account
          </Typography>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          <Box component="form" onSubmit={handleSubmit} noValidate>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <TextField label="First Name" fullWidth margin="normal" required
                value={form.firstName} onChange={set('firstName')} />
              <TextField label="Last Name" fullWidth margin="normal" required
                value={form.lastName} onChange={set('lastName')} />
            </Box>
            <TextField label="Email" type="email" fullWidth margin="normal" required
              value={form.email} onChange={set('email')} />
            <TextField label="Password" type="password" fullWidth margin="normal" required
              value={form.password} onChange={set('password')} />
            <TextField label="Confirm Password" type="password" fullWidth margin="normal" required
              value={form.confirmPassword} onChange={set('confirmPassword')} />
            <Button type="submit" fullWidth variant="contained" size="large" sx={{ mt: 2 }} disabled={loading}>
              {loading ? 'Creating account…' : 'Create Account'}
            </Button>
          </Box>
          <Typography variant="body2" align="center" sx={{ mt: 2 }}>
            Already have an account?{' '}
            <Link component={RouterLink} to="/login">Sign In</Link>
          </Typography>
        </Paper>
      </Box>
    </Container>
  )
}
