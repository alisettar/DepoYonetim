import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/Dashboard.vue'
import WarehouseListView from '../views/warehouses/WarehouseListView.vue'
import UnitListView from '../views/catalog/UnitListView.vue'
import CategoryListView from '../views/catalog/CategoryListView.vue'
import ProductListView from '../views/catalog/ProductListView.vue'
import MovementsView from '../views/inventory/MovementsView.vue'
import GoodsReceiptView from '../views/inventory/GoodsReceiptView.vue'
import RecipeListView from '../views/recipes/RecipeListView.vue'
import RecipeDetailView from '../views/recipes/RecipeDetailView.vue'
import LotListView from '../views/lots/LotListView.vue'
import LotDetailView from '../views/lots/LotDetailView.vue'
import LotTraceView from '../views/lots/LotTraceView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'Dashboard', component: DashboardView },
    { path: '/warehouses', name: 'Warehouses', component: WarehouseListView },
    { path: '/units', name: 'Units', component: UnitListView },
    { path: '/categories', name: 'Categories', component: CategoryListView },
    { path: '/products', name: 'Products', component: ProductListView },
    { path: '/recipes', name: 'Recipes', component: RecipeListView },
    { path: '/recipes/:id', name: 'RecipeDetail', component: RecipeDetailView },
    { path: '/inventory/movements', name: 'Movements', component: MovementsView },
    { path: '/inventory/goods-receipt', name: 'GoodsReceipt', component: GoodsReceiptView },
    { path: '/lots', name: 'Lots', component: LotListView },
    { path: '/lots/:id', name: 'LotDetail', component: LotDetailView },
    { path: '/lots/:id/trace', name: 'LotTrace', component: LotTraceView }
  ]
})

export default router
