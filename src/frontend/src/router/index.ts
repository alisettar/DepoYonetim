import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/Dashboard.vue'
import WarehouseListView from '../views/warehouses/WarehouseListView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'Dashboard',
      component: DashboardView
    },
    {
      path: '/warehouses',
      name: 'Warehouses',
      component: WarehouseListView
    }
  ]
})

export default router
