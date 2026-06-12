import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  AppBar, Alert, Box, Button, Container, IconButton,
  Paper, TextField, Toolbar, Typography,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { getEvents, updateEvent } from '../api/events'
import { getErrorMessage } from '../utils/error'

function toDatetimeLocal(iso: string) {
  const d = new Date(iso)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

export default function EditEventPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [name, setName] = useState('')
  const [date, setDate] = useState('')
  const [description, setDescription] = useState('')
  const [location, setLocation] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [notFound, setNotFound] = useState(false)

  useEffect(() => {
    getEvents()
      .then(({ data }) => {
        const event = data.find((e) => e.id === id)
        if (!event) { setNotFound(true); return }
        setName(event.name)
        setDate(toDatetimeLocal(event.date))
        setDescription(event.description ?? '')
        setLocation(event.location ?? '')
      })
      .catch((err) => setError(getErrorMessage(err)))
  }, [id])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await updateEvent(id!, {
        name,
        date: new Date(date).toISOString(),
        description: description || undefined,
        location: location || undefined,
      })
      navigate('/events')
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }

  if (notFound) {
    return (
      <Container maxWidth="sm">
        <Box sx={{ mt: 6 }}>
          <Alert severity="error">Event not found.</Alert>
          <Button sx={{ mt: 2 }} onClick={() => navigate('/events')}>Back to Events</Button>
        </Box>
      </Container>
    )
  }

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <IconButton color="inherit" onClick={() => navigate('/events')} sx={{ mr: 1 }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h6">Edit Event</Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="sm">
        <Box sx={{ mt: 6 }}>
          <Paper elevation={2} sx={{ p: 4 }}>
            <Typography variant="h5" gutterBottom>Edit Event</Typography>
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
            <Box component="form" onSubmit={handleSubmit} noValidate sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                label="Event Name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                fullWidth
              />
              <TextField
                label="Date & Time"
                type="datetime-local"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                required
                fullWidth
                InputLabelProps={{ shrink: true }}
              />
              <TextField
                label="Location"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                fullWidth
              />
              <TextField
                label="Description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                fullWidth
                multiline
                rows={3}
              />
              <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                <Button onClick={() => navigate('/events')} disabled={loading}>Cancel</Button>
                <Button type="submit" variant="contained" disabled={loading}>
                  {loading ? 'Saving…' : 'Save Changes'}
                </Button>
              </Box>
            </Box>
          </Paper>
        </Box>
      </Container>
    </>
  )
}
