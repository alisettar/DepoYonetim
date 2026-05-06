export type WarehouseType = 'Physical' | 'Virtual' | 'Machine'
export type WarehouseStatus = 'Active' | 'Inactive'
export type MachineWarehouseStatus = 'Idle' | 'Loaded' | 'Running'

export interface WarehouseLocation {
  id: string
  code: string
  name: string
  capacity: number | null
  isActive: boolean
}

export interface Warehouse {
  id: string
  code: string
  name: string
  type: WarehouseType
  status: WarehouseStatus
  address: string | null
  parentWarehouseId: string | null
  createdAt: string
  locations: WarehouseLocation[]
  machineCode?: string
  machineStatus?: MachineWarehouseStatus
}

export interface CreateWarehouseRequest {
  code: string
  name: string
  type: WarehouseType
  address?: string
  parentWarehouseId?: string
  machineCode?: string
}

export interface UpdateWarehouseRequest {
  name: string
  address?: string
}

export interface DashboardStats {
  totalWarehouses: number
  totalLocations: number
  activeWarehouses: number
}
