import { useEffect, useRef, useState } from 'react'
import { Autocomplete, CircularProgress, TextField } from '@mui/material'

const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY ?? ''

let mapsReady: Promise<void> | null = null

function loadMaps(): Promise<void> {
  if (mapsReady) return mapsReady
  mapsReady = new Promise((resolve, reject) => {
    if (window.google?.maps?.places) { resolve(); return }
    const script = document.createElement('script')
    script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places`
    script.async = true
    script.onload = () => resolve()
    script.onerror = () => reject(new Error('Failed to load Google Maps'))
    document.head.appendChild(script)
  })
  return mapsReady
}

interface Props {
  value: string
  onChange: (value: string) => void
  label?: string
  placeholder?: string
}

export default function LocationAutocomplete({ value, onChange, label = 'Location', placeholder }: Props) {
  const [options, setOptions] = useState<string[]>([])
  const [loading, setLoading] = useState(false)
  const serviceRef = useRef<google.maps.places.AutocompleteService | null>(null)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    loadMaps()
      .then(() => { serviceRef.current = new google.maps.places.AutocompleteService() })
      .catch(() => {})
    return () => { if (debounceRef.current) clearTimeout(debounceRef.current) }
  }, [])

  const handleInputChange = (_: React.SyntheticEvent, newValue: string, reason: string) => {
    onChange(newValue)

    if (reason !== 'input') return

    if (debounceRef.current) clearTimeout(debounceRef.current)

    if (newValue.length < 3 || !serviceRef.current) {
      setOptions([])
      return
    }

    debounceRef.current = setTimeout(() => {
      setLoading(true)
      serviceRef.current!.getPlacePredictions({ input: newValue }, (predictions, status) => {
        setLoading(false)
        if (status === google.maps.places.PlacesServiceStatus.OK && predictions) {
          setOptions(predictions.map((p) => p.description))
        } else {
          setOptions([])
        }
      })
    }, 300)
  }

  return (
    <Autocomplete
      freeSolo
      filterOptions={(x) => x}
      options={options}
      loading={loading}
      inputValue={value}
      onInputChange={handleInputChange}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          placeholder={placeholder}
          fullWidth
          InputProps={{
            ...params.InputProps,
            endAdornment: (
              <>
                {loading && <CircularProgress color="inherit" size={18} />}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
        />
      )}
    />
  )
}
