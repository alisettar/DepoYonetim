// Warehouse
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

// Unit
export interface Unit {
  id: string
  code: string
  name: string
}

export interface CreateUnitRequest {
  code: string
  name: string
}

export interface UpdateUnitRequest {
  name: string
}

// Category
export interface Category {
  id: string
  code: string
  name: string
}

export interface CreateCategoryRequest {
  code: string
  name: string
}

export interface UpdateCategoryRequest {
  name: string
}

// Product
export type ProductStatus = 'Active' | 'Passive'

export interface ProductUnit {
  id: string
  unitId: string
  conversionToPrimary: number
}

export interface Product {
  id: string
  code: string
  name: string
  categoryId: string
  primaryUnitId: string
  lotRequired: boolean
  shelfLifeDays: number | null
  minStock: number | null
  maxStock: number | null
  status: ProductStatus
  createdAt: string
  updatedAt: string
  units: ProductUnit[]
}

export interface CreateProductRequest {
  code: string
  name: string
  categoryId: string
  primaryUnitId: string
  lotRequired?: boolean
  shelfLifeDays?: number
  minStock?: number
  maxStock?: number
}

export interface UpdateProductRequest {
  name: string
  categoryId: string
  lotRequired: boolean
  shelfLifeDays?: number
  minStock?: number
  maxStock?: number
}

export interface AddProductUnitRequest {
  unitId: string
  conversionToPrimary: number
}

// Dashboard
export interface DashboardStats {
  totalWarehouses: number
  totalLocations: number
  activeWarehouses: number
}

// Inventory
export type MovementType =
  | 'GoodsReceipt' | 'Shipment' | 'TransferIn' | 'TransferOut'
  | 'MachineLoad' | 'MachineUnload' | 'ProductionConsumption' | 'ProductionOutput'
  | 'InventoryAdjustmentIn' | 'InventoryAdjustmentOut' | 'Scrap'
  | 'CustomerReturn' | 'SupplierReturn' | 'Reversal'

export type LotSource = 'Receipt' | 'Production'
export type QualityStatus = 'OK' | 'Quarantine' | 'Rejected'

export interface Lot {
  id: string
  productId: string
  lotNumber: string
  productionDate: string
  expiryDate: string | null
  source: LotSource
  qualityStatus: QualityStatus
  createdAt: string
}

export interface StockMovement {
  id: string
  occurredAt: string
  type: MovementType
  productId: string
  lotId: string | null
  warehouseId: string | null
  machineWarehouseId: string | null
  locationId: string | null
  quantity: number
  unitId: string
  unitCost: number | null
  note: string | null
  createdByUserId: string
  createdAt: string
}

export interface StockBalance {
  id: string
  productId: string
  lotId: string | null
  warehouseId: string | null
  machineWarehouseId: string | null
  quantity: number
  updatedAt: string
}

export interface GoodsReceiptItemRequest {
  productId: string
  lotNumber?: string
  productionDate?: string
  expiryDate?: string
  warehouseId: string
  locationId?: string
  quantity: number
  unitId: string
  unitCost?: number
}

export interface GoodsReceiptRequest {
  items: GoodsReceiptItemRequest[]
  note?: string
}
