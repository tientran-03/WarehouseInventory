import { useEffect } from 'react'
import { joinWarehouseGroup, subscribeNewSubOrders } from '../services/signalrService'
import type { NewSubOrderEvent } from '../types'

export function useWarehouseOrdersHub(
  warehouseIds: string[],
  onNewSubOrder: (event: NewSubOrderEvent) => void,
) {
  useEffect(() => {
    if (warehouseIds.length === 0) return

    let active = true

    const setup = async () => {
      try {
        await subscribeNewSubOrders((event) => {
          if (active) onNewSubOrder(event)
        })
        for (const id of warehouseIds) {
          await joinWarehouseGroup(id)
        }
      } catch {
        // Hub optional — app works without realtime
      }
    }

    setup()

    return () => {
      active = false
    }
  }, [warehouseIds.join(','), onNewSubOrder])
}
