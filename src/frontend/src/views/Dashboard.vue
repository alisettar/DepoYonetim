<template>
  <div>
    <h1>Dashboard</h1>

    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalWarehouses }}</div>
        <div class="stat-label">Toplam Depo</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.activeWarehouses }}</div>
        <div class="stat-label">Aktif Depo</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ stats.totalLocations }}</div>
        <div class="stat-label">Toplam Konum</div>
      </div>
    </div>

    <div class="section">
      <h2 style="font-size: 1.125rem; margin-bottom: 0.75rem;">Depolar</h2>
      <div class="table-container">
        <table v-if="warehouses.length > 0">
          <thead>
            <tr>
              <th>Kod</th>
              <th>Ad</th>
              <th>Tür</th>
              <th>Adres</th>
              <th>Konum</th>
              <th>Durum</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="wh in warehouses" :key="wh.id">
              <td><strong>{{ wh.code }}</strong></td>
              <td>{{ wh.name }}</td>
              <td>{{ typeLabel(wh.type) }}</td>
              <td>{{ wh.address || '-' }}</td>
              <td>{{ wh.locations.length }}</td>
              <td>
                <span :class="wh.status === 'Active' ? 'status-active' : 'status-inactive'">
                  {{ wh.status === 'Active' ? 'Aktif' : 'Pasif' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-else class="empty-state">
          Henüz depo yok. <router-link to="/warehouses">Depo oluşturun</router-link>.
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { getWarehouses } from '../api/client'
import type { Warehouse, WarehouseType } from '../types'

const warehouses = ref<Warehouse[]>([])

const typeLabel = (type: WarehouseType) =>
  ({ Physical: 'Fiziksel', Virtual: 'Sanal', Machine: 'Makine' })[type]

const fetchWarehouses = async () => {
  try {
    warehouses.value = await getWarehouses()
  } catch {
    warehouses.value = []
  }
}

const stats = computed(() => ({
  totalWarehouses: warehouses.value.length,
  activeWarehouses: warehouses.value.filter(w => w.status === 'Active').length,
  totalLocations: warehouses.value.reduce((sum, w) => sum + w.locations.length, 0)
}))

onMounted(fetchWarehouses)
</script>

<style scoped>
.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.stat-card {
  background: white;
  border-radius: 8px;
  padding: 1rem 1.25rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: #2563eb;
}

.stat-label {
  font-size: 0.875rem;
  color: #666;
  margin-top: 0.25rem;
}

.section { margin-top: 1.5rem; }

.status-active   { color: #16a34a; font-weight: 500; }
.status-inactive { color: #dc2626; font-weight: 500; }
</style>
