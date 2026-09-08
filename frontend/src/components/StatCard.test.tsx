import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import StatCard from '../components/StatCard'

describe('StatCard', () => {
  it('renders title and value', () => {
    render(<StatCard title="Warehouses" value={5} />)
    expect(screen.getByText('Warehouses')).toBeInTheDocument()
    expect(screen.getByText('5')).toBeInTheDocument()
  })

  it('renders subtitle when provided', () => {
    render(<StatCard title="Low Stock" value={2} subtitle="Needs attention" variant="warning" />)
    expect(screen.getByText('Needs attention')).toBeInTheDocument()
  })
})
