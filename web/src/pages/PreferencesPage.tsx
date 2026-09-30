import { useEffect, useState } from 'react'
import { PageHeader } from '@/components/PageHeader'
import { LoadingState } from '@/components/LoadingState'
import { FormField, FormSelect } from '@/components/FormField'
import { useMyPreferences, useUpsertMyPreferences } from '@/hooks/useActivities'
import { useToastStore } from '@/stores/toastStore'

const PACE_OPTIONS = [
  { value: 'Relaxed', label: 'Relaxed' },
  { value: 'Moderate', label: 'Moderate' },
  { value: 'Packed', label: 'Packed' },
]

const BUDGET_OPTIONS = [
  { value: 'Budget', label: 'Budget' },
  { value: 'Mid', label: 'Mid-range' },
  { value: 'Luxury', label: 'Luxury' },
]

export function PreferencesPage() {
  const { data, isLoading } = useMyPreferences()
  const upsert = useUpsertMyPreferences()
  const push = useToastStore((s) => s.push)

  const [interests, setInterests] = useState('')
  const [pace, setPace] = useState('Moderate')
  const [dietary, setDietary] = useState('')
  const [budgetTier, setBudgetTier] = useState('Mid')
  const [mobilityNotes, setMobilityNotes] = useState('')

  useEffect(() => {
    if (data) {
      setInterests(data.interests.join(', '))
      setPace(data.pace)
      setDietary(data.dietary.join(', '))
      setBudgetTier(data.budgetTier)
      setMobilityNotes(data.mobilityNotes ?? '')
    }
  }, [data])

  async function handleSave() {
    try {
      await upsert.mutateAsync({
        interests: interests
          .split(',')
          .map((s) => s.trim())
          .filter(Boolean),
        pace: pace as 'Relaxed' | 'Moderate' | 'Packed',
        dietary: dietary
          .split(',')
          .map((s) => s.trim())
          .filter(Boolean),
        budgetTier: budgetTier as 'Budget' | 'Mid' | 'Luxury',
        mobilityNotes: mobilityNotes.trim() || null,
      })
      push('success', 'Preferences saved.')
    } catch {
      push('error', 'Failed to save preferences.')
    }
  }

  if (isLoading) return <LoadingState />

  return (
    <>
      <PageHeader
        title="Preferences"
        subtitle="Used by the recommender to score activities for you."
      />
      <div className="card" style={{ maxWidth: '560px' }}>
        <FormField
          label="Interests"
          id="interests"
          value={interests}
          onChange={setInterests}
          hint="Comma-separated, e.g. hiking, culture, food"
        />
        <FormSelect
          label="Pace"
          id="pace"
          value={pace}
          onChange={setPace}
          options={PACE_OPTIONS}
        />
        <FormField
          label="Dietary"
          id="dietary"
          value={dietary}
          onChange={setDietary}
          hint="Comma-separated, e.g. vegetarian, gluten-free"
        />
        <FormSelect
          label="Budget tier"
          id="budgetTier"
          value={budgetTier}
          onChange={setBudgetTier}
          options={BUDGET_OPTIONS}
        />
        <FormField
          label="Mobility notes"
          id="mobilityNotes"
          value={mobilityNotes}
          onChange={setMobilityNotes}
          hint="Any accessibility considerations"
        />
        <button className="btn btn-primary" onClick={handleSave} disabled={upsert.isPending}>
          {upsert.isPending ? 'Saving…' : 'Save preferences'}
        </button>
      </div>
    </>
  )
}