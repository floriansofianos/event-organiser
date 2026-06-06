import { useEffect, useState } from 'react'
import {
  AppBar, Box, Button, Container, Paper, Toolbar, Typography,
} from '@mui/material'
import { useAuth } from '../context/AuthContext'
import { getMe, type UserDto } from '../api/auth'

export default function DashboardPage() {
  const { user, logout } = useAuth()
  const [profile, setProfile] = useState<UserDto | null>(null)

  useEffect(() => {
    getMe().then(({ data }) => setProfile(data)).catch(() => {})
  }, [])

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>Event Organizer</Typography>
          <Button color="inherit" onClick={logout}>Logout</Button>
        </Toolbar>
      </AppBar>
      <Container maxWidth="sm">
        <Box sx={{ mt: 6 }}>
          <Paper elevation={2} sx={{ p: 4 }}>
            <Typography variant="h5" gutterBottom>
              Welcome, {user?.firstName}!
            </Typography>
            {profile && (
              <Box sx={{ mt: 2, '& > p': { mb: 0.5 } }}>
                <Typography><strong>Name:</strong> {profile.firstName} {profile.lastName}</Typography>
                <Typography><strong>Email:</strong> {profile.email}</Typography>
                <Typography><strong>Email confirmed:</strong> {profile.isEmailConfirmed ? 'Yes' : 'No'}</Typography>
              </Box>
            )}
          </Paper>
        </Box>
      </Container>
    </>
  )
}
