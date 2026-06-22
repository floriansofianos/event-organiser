import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  AppBar, Box, Button, Chip, Container, IconButton, Paper,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Toolbar, Tooltip, Typography, Dialog, DialogActions, DialogContent,
  DialogContentText, DialogTitle, Alert,
} from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import CancelIcon from '@mui/icons-material/Cancel'
import AddIcon from '@mui/icons-material/Add'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import RoomIcon from '@mui/icons-material/Room'
import { getEvents, cancelEvent, type EventDto } from '../api/events'
import { getErrorMessage } from '../utils/error'
import { useAuth } from '../context/AuthContext'

export default function EventsPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const [events, setEvents] = useState<EventDto[]>([])
  const [error, setError] = useState('')
  const [cancelTarget, setCancelTarget] = useState<EventDto | null>(null)
  const [cancelling, setCancelling] = useState(false)

  useEffect(() => {
    getEvents()
      .then(({ data }) => setEvents(data))
      .catch((err) => setError(getErrorMessage(err)))
  }, [])

  const handleConfirmCancel = async () => {
    if (!cancelTarget) return
    setCancelling(true)
    try {
      await cancelEvent(cancelTarget.id)
      setEvents((prev) =>
        prev.map((e) => (e.id === cancelTarget.id ? { ...e, status: 'Cancelled' } : e))
      )
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setCancelling(false)
      setCancelTarget(null)
    }
  }

  const formatDate = (iso: string) =>
    new Date(iso).toLocaleString(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short',
    })

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <IconButton color="inherit" onClick={() => navigate('/')} sx={{ mr: 1 }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>My Events</Typography>
          <Button color="inherit" onClick={logout}>Logout</Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg">
        <Box sx={{ mt: 4, mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography variant="h5">Events</Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate('/events/new')}
          >
            New Event
          </Button>
        </Box>

        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

        {events.length === 0 ? (
          <Paper elevation={2} sx={{ p: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">
              No events yet. Create your first one!
            </Typography>
          </Paper>
        ) : (
          <TableContainer component={Paper} elevation={2}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell><strong>Name</strong></TableCell>
                  <TableCell><strong>Date</strong></TableCell>
                  <TableCell><strong>Location</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell align="right"><strong>Actions</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {events.map((event) => (
                  <TableRow key={event.id} hover>
                    <TableCell>{event.name}</TableCell>
                    <TableCell>{formatDate(event.date)}</TableCell>
                    <TableCell>
                      {event.location ? (
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                          <Box component="span" sx={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', maxWidth: 180 }}>
                            {event.location}
                          </Box>
                          <Tooltip title="View on map">
                            <IconButton
                              size="small"
                              component="a"
                              href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(event.location)}`}
                              target="_blank"
                              rel="noopener noreferrer"
                            >
                              <RoomIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Box>
                      ) : '—'}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={event.status}
                        color={event.status === 'Active' ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      {event.status === 'Active' && (
                        <>
                          <IconButton
                            size="small"
                            title="Edit"
                            onClick={() => navigate(`/events/${event.id}/edit`)}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                          <IconButton
                            size="small"
                            title="Cancel event"
                            onClick={() => setCancelTarget(event)}
                            sx={{ ml: 0.5 }}
                          >
                            <CancelIcon fontSize="small" />
                          </IconButton>
                        </>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Container>

      <Dialog open={!!cancelTarget} onClose={() => setCancelTarget(null)}>
        <DialogTitle>Cancel event?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to cancel <strong>{cancelTarget?.name}</strong>? This cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCancelTarget(null)} disabled={cancelling}>Keep</Button>
          <Button color="error" onClick={handleConfirmCancel} disabled={cancelling}>
            {cancelling ? 'Cancelling…' : 'Cancel Event'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}
