import axios from 'axios'
import type {
  Warehouse, CreateWarehouseRequest, UpdateWarehouseRequest, WarehouseType,
  Unit, CreateUnitRequest, UpdateUnitRequest,
  Category, CreateCategoryRequest, UpdateCategoryRequest,
  Product, CreateProductRequest, UpdateProductRequest, AddProductUnitRequest, ProductStatus,
  StockMovement, StockBalance, GoodsReceiptRequest,
  Recipe, RecipeSummary, CreateRecipeRequest, UpdateRecipeRequest,
  AddRecipeVersionRequest, AddRecipeItemRequest, UpdateRecipeItemRequest,
  AddAlternativeRequest, BomLine, RecipeItem, RecipeVersion, AlternativeMaterial,
  LotDetailResponse, TraceChainEntry, UpdateQualityStatusRequest, PaginatedResponse, Lot,
} from '../types'

const api = axios.create({
  baseURL: '/api/v1',
  timeout: 10000
})

// Warehouses
export async function getWarehouses(type?: WarehouseType): Promise<Warehouse[]> {
  const params = type ? { type } : {}
  const res = await api.get<Warehouse[]>('/warehouses', { params })
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

// Units
export async function getUnits(): Promise<Unit[]> {
  const res = await api.get<Unit[]>('/units')
  return res.data
}

export async function createUnit(data: CreateUnitRequest): Promise<Unit> {
  const res = await api.post<Unit>('/units', data)
  return res.data
}

export async function updateUnit(id: string, data: UpdateUnitRequest): Promise<Unit> {
  const res = await api.put<Unit>(`/units/${id}`, data)
  return res.data
}

export async function deleteUnit(id: string): Promise<void> {
  await api.delete(`/units/${id}`)
}

// Categories
export async function getCategories(): Promise<Category[]> {
  const res = await api.get<Category[]>('/categories')
  return res.data
}

export async function createCategory(data: CreateCategoryRequest): Promise<Category> {
  const res = await api.post<Category>('/categories', data)
  return res.data
}

export async function updateCategory(id: string, data: UpdateCategoryRequest): Promise<Category> {
  const res = await api.put<Category>(`/categories/${id}`, data)
  return res.data
}

export async function deleteCategory(id: string): Promise<void> {
  await api.delete(`/categories/${id}`)
}

// Products
export async function getProducts(status?: ProductStatus): Promise<Product[]> {
  const params = status ? { status } : {}
  const res = await api.get<Product[]>('/products', { params })
  return res.data
}

export async function getProduct(id: string): Promise<Product> {
  const res = await api.get<Product>(`/products/${id}`)
  return res.data
}

export async function createProduct(data: CreateProductRequest): Promise<Product> {
  const res = await api.post<Product>('/products', data)
  return res.data
}

export async function updateProduct(id: string, data: UpdateProductRequest): Promise<Product> {
  const res = await api.put<Product>(`/products/${id}`, data)
  return res.data
}

export async function deleteProduct(id: string): Promise<void> {
  await api.delete(`/products/${id}`)
}

export async function addProductUnit(id: string, data: AddProductUnitRequest): Promise<Product> {
  const res = await api.post<Product>(`/products/${id}/units`, data)
  return res.data
}

export async function removeProductUnit(productId: string, unitId: string): Promise<Product> {
  const res = await api.delete<Product>(`/products/${productId}/units/${unitId}`)
  return res.data
}

// Inventory
export async function getMovements(params?: { type?: string; productId?: string; warehouseId?: string }): Promise<StockMovement[]> {
  const res = await api.get<StockMovement[]>('/inventory/movements', { params })
  return res.data
}

export async function getBalances(params?: { productId?: string; warehouseId?: string }): Promise<StockBalance[]> {
  const res = await api.get<StockBalance[]>('/inventory/balances', { params })
  return res.data
}

export async function createGoodsReceipt(data: GoodsReceiptRequest): Promise<StockMovement[]> {
  const res = await api.post<StockMovement[]>('/inventory/goods-receipts', data)
  return res.data
}

// Recipes
export async function getRecipes(productId?: string): Promise<RecipeSummary[]> {
  const params = productId ? { productId } : {}
  const res = await api.get<RecipeSummary[]>('/recipes', { params })
  return res.data
}

export async function getRecipe(id: string): Promise<Recipe> {
  const res = await api.get<Recipe>(`/recipes/${id}`)
  return res.data
}

export async function createRecipe(data: CreateRecipeRequest): Promise<Recipe> {
  const res = await api.post<Recipe>('/recipes', data)
  return res.data
}

export async function updateRecipe(id: string, data: UpdateRecipeRequest): Promise<Recipe> {
  const res = await api.patch<Recipe>(`/recipes/${id}`, data)
  return res.data
}

export async function archiveRecipe(id: string): Promise<void> {
  await api.delete(`/recipes/${id}`)
}

export async function getRecipeVersions(id: string): Promise<RecipeVersion[]> {
  const res = await api.get<RecipeVersion[]>(`/recipes/${id}/versions`)
  return res.data
}

export async function addRecipeVersion(id: string, data: AddRecipeVersionRequest): Promise<RecipeVersion> {
  const res = await api.post<RecipeVersion>(`/recipes/${id}/versions`, data)
  return res.data
}

export async function activateRecipeVersion(id: string, versionId: string): Promise<Recipe> {
  const res = await api.post<Recipe>(`/recipes/${id}/versions/${versionId}/activate`)
  return res.data
}

export async function explodeRecipeBom(id: string, versionId: string): Promise<BomLine[]> {
  const res = await api.post<BomLine[]>(`/recipes/${id}/versions/${versionId}/explode`)
  return res.data
}

export async function addRecipeItem(recipeId: string, versionId: string, data: AddRecipeItemRequest): Promise<RecipeItem> {
  const res = await api.post<RecipeItem>(`/recipes/${recipeId}/versions/${versionId}/items`, data)
  return res.data
}

export async function updateRecipeItem(recipeId: string, versionId: string, itemId: string, data: UpdateRecipeItemRequest): Promise<RecipeItem> {
  const res = await api.patch<RecipeItem>(`/recipes/${recipeId}/versions/${versionId}/items/${itemId}`, data)
  return res.data
}

export async function deleteRecipeItem(recipeId: string, versionId: string, itemId: string): Promise<void> {
  await api.delete(`/recipes/${recipeId}/versions/${versionId}/items/${itemId}`)
}

export async function addAlternative(recipeId: string, versionId: string, itemId: string, data: AddAlternativeRequest): Promise<AlternativeMaterial> {
  const res = await api.post<AlternativeMaterial>(`/recipes/${recipeId}/versions/${versionId}/items/${itemId}/alternatives`, data)
  return res.data
}

export async function deleteAlternative(recipeId: string, versionId: string, itemId: string, alternativeId: string): Promise<void> {
  await api.delete(`/recipes/${recipeId}/versions/${versionId}/items/${itemId}/alternatives/${alternativeId}`)
}

// Lot Traceability
export async function getLots(params?: {
  productId?: string
  lotNumberFilter?: string
  qualityStatus?: string
  page?: number
  pageSize?: number
}): Promise<PaginatedResponse<Lot>> {
  const res = await api.get<PaginatedResponse<Lot>>('/inventory/lots', { params })
  return res.data
}

export async function getLotMovements(lotId: string): Promise<LotDetailResponse> {
  const res = await api.get<LotDetailResponse>(`/inventory/lots/${lotId}/movements`)
  return res.data
}

export async function getLotTrace(lotId: string): Promise<TraceChainEntry[]> {
  const res = await api.get<TraceChainEntry[]>(`/inventory/lots/${lotId}/trace`)
  return res.data
}

export async function updateLotQualityStatus(lotId: string, data: UpdateQualityStatusRequest): Promise<Lot> {
  const res = await api.patch<Lot>(`/inventory/lots/${lotId}/quality-status`, data)
  return res.data
}

export { api }
