import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/Dashboard.vue'
import WarehouseListView from '../views/warehouses/WarehouseListView.vue'
import UnitListView from '../views/catalog/UnitListView.vue'
import CategoryListView from '../views/catalog/CategoryListView.vue'
import ProductListView from '../views/catalog/ProductListView.vue'
import MovementsView from '../views/inventory/MovementsView.vue'
import GoodsReceiptView from '../views/inventory/GoodsReceiptView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'Dashboard', component: DashboardView },
    { path: '/warehouses', name: 'Warehouses', component: WarehouseListView },
    { path: '/units', name: 'Units', component: UnitListView },
    { path: '/categories', name: 'Categories', component: CategoryListView },
    { path: '/products', name: 'Products', component: ProductListView },
    { path: '/inventory/movements', name: 'Movements', component: MovementsView },
    { path: '/inventory/goods-receipt', name: 'GoodsReceipt', component: GoodsReceiptView }
  ]
})

export default router
