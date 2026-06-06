import { useEffect, useState } from 'react'
import { useSearchParams, Link as RouterLink } from 'react-router-dom'
import { Box, Button, CircularProgress, Container, Link, Paper, Typography } from '@mui/material'
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import { confirmEmail } from '../api/auth'
import { getErrorMessage } from '../utils/error'

type Status = 'loading' | 'success' | 'error'

export default function ConfirmEmailPage() {
  const [searchParams] = useSearchParams()
  const [status, setStatus] = useState<Status>('loading')
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const token = searchParams.get('token')
    if (!token) {
      setStatus('error')
      setErrorMessage('No confirmation token provided.')
      return
    }

    confirmEmail(token)
      .then(() => setStatus('success'))
      .catch((err) => {
        setStatus('error')
        setErrorMessage(getErrorMessage(err))
      })
  }, [searchParams])

  return (
    <Container maxWidth="xs">
      <Box sx={{ mt: 8 }}>
        <Paper elevation={3} sx={{ p: 4, textAlign: 'center' }}>
          {status === 'loading' && (
            <>
              <CircularProgress sx={{ mb: 2 }} />
              <Typography>Confirming your email…</Typography>
            </>
          )}
          {status === 'success' && (
            <>
              <CheckCircleOutlineIcon color="success" sx={{ fontSize: 56, mb: 1 }} />
              <Typography variant="h6" gutterBottom>Email Confirmed!</Typography>
              <Typography color="text.secondary" sx={{ mb: 3 }}>
                Your account is now active. You can sign in.
              </Typography>
              <Button component={RouterLink} to="/login" variant="contained" fullWidth>
                Sign In
              </Button>
            </>
          )}
          {status === 'error' && (
            <>
              <ErrorOutlineIcon color="error" sx={{ fontSize: 56, mb: 1 }} />
              <Typography variant="h6" gutterBottom>Confirmation Failed</Typography>
              <Typography color="text.secondary" sx={{ mb: 3 }}>{errorMessage}</Typography>
              <Link component={RouterLink} to="/register">Register again</Link>
            </>
          )}
        </Paper>
      </Box>
    </Container>
  )
}
