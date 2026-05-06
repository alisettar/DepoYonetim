import axios from 'axios'
import type { Warehouse, CreateWarehouseRequest, UpdateWarehouseRequest } from '../types'

const api = axios.create({
  baseURL: '/api/v1',
  timeout: 10000
})

export async function getWarehouses(): Promise<Warehouse[]> {
  const res = await api.get<Warehouse[]>('/warehouses')
  return res.data
}

export async function getWarehouse(id: string): Promise<Warehouse> {
  const res = await api.get<Warehouse>(`/warehouses/${id}`)
  return res.data
}

export async function createWarehouse(data: CreateWarehouseRequest): Promise<Warehouse> {
  const res = await api.post<Warehouse>('/warehouses', data)
  return res.data
}

export async function updateWarehouse(id: string, data: UpdateWarehouseRequest): Promise<Warehouse> {
  const res = await api.put<Warehouse>(`/warehouses/${id}`, data)
  return res.data
}

export async function deleteWarehouse(id: string): Promise<void> {
  await api.delete(`/warehouses/${id}`)
}

export { api }
