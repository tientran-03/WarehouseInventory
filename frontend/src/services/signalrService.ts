import * as signalR from '@microsoft/signalr'
import { STORAGE_KEYS } from '../constants/storageKeys'
import type { NewSubOrderEvent } from '../types'

let connection: signalR.HubConnection | null = null

export async function getWarehouseHubConnection(): Promise<signalR.HubConnection> {
  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    return connection
  }

  if (connection) {
    await connection.stop()
  }

  const token = localStorage.getItem(STORAGE_KEYS.TOKEN)
  const apiUrl = import.meta.env.VITE_API_URL || '/api'
  
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${apiUrl}/hubs/warehouse-orders`, {
      accessTokenFactory: () => token || ''
    })
    .withAutomaticReconnect()
    .build()

  await connection.start()
  return connection
}

export async function joinWarehouseGroup(warehouseId: string) {
  const hub = await getWarehouseHubConnection()
  await hub.invoke('JoinWarehouse', warehouseId)
}

export async function subscribeNewSubOrders(handler: (event: NewSubOrderEvent) => void) {
  const hub = await getWarehouseHubConnection()
  hub.off('NewSubOrder')
  hub.on('NewSubOrder', handler)
}

export async function disconnectWarehouseHub() {
  if (connection) {
    await connection.stop()
    connection = null
  }
}
