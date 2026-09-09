import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import StatCard from './StatCard'

describe('StatCard', () => {
  it('renders title and value', () => {
    render(<StatCard title="Test" value="100" />)
    expect(screen.getByText('Test')).toBeInTheDocument()
    expect(screen.getByText('100')).toBeInTheDocument()
  })

  it('renders icon when provided', () => {
    const icon = <span data-testid="icon">Icon</span>
    render(<StatCard title="Test" value="100" icon={icon} />)
    expect(screen.getByTestId('icon')).toBeInTheDocument()
  })
})
