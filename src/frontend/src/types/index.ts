export interface WarehouseLocation {
  id: string
  zone: string
  aisle: string
  section: string
  bin: string
  fullName: string
  isActive: boolean
}

export interface Warehouse {
  id: string
  code: string
  name: string
  address: string | null
  isActive: boolean
  createdAt: string
  locations: WarehouseLocation[]
}

export interface CreateWarehouseRequest {
  code: string
  name: string
  address?: string
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
