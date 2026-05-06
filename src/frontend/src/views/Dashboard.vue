<template>
  <div class="dashboard">
    <h1>Dashboard</h1>

    <!-- Lot Search -->
    <section class="search-section">
      <div class="search-bar">
        <input
          v-model="lotQuery"
          placeholder="Lot no ara..."
          class="search-input"
          @keyup.enter="handleLotSearch"
        />
        <button class="btn-primary" @click="handleLotSearch">Ara</button>
      </div>

      <div v-if="lotSearchResults.length > 0" class="lot-results">
        <table>
          <thead>
            <tr>
              <th>Lot No</th>
              <th>Ürün</th>
              <th>Kalite</th>
              <th>SKT</th>
              <th>Stok</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="lot in lotSearchResults" :key="lot.id">
              <td><strong>{{ lot.lotNumber }}</strong></td>
              <td>{{ lot.productName }}</td>
              <td><span :class="qualityStatusClass(lot.qualityStatus)">{{ qualityStatusLabel(lot.qualityStatus) }}</span></td>
              <td>{{ lot.expiryDate ? formatDate(lot.expiryDate) : '-' }}</td>
              <td>{{ lot.availableQty ?? '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- Stats Grid -->
    <div class="stats-grid">
      <div class="stat-card" :class="{ 'critical': criticalStock.length > 0 }">
        <div class="stat-value">{{ criticalStock.length }}</div>
        <div class="stat-label">Kritik Stok</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ warehouseFills.length }}</div>
        <div class="stat-label">Depo Konum</div>
      </div>
      <div class="stat-card">
        <div class="stat-value">{{ recentMovements.length }}</div>
        <div class="stat-label">Son Hareket</div>
      </div>
    </div>

    <!-- Critical Stock -->
    <section v-if="criticalStock.length > 0" class="section">
      <h2 class="section-title">
        Kritik Stok Altındaki Ürünler
        <span class="badge-red">{{ criticalStock.length }}</span>
      </h2>
      <div class="table-container">
        <table>
          <thead>
            <tr>
              <th>Kod</th>
              <th>Ürün</th>
              <th>Birim</th>
              <th>Mevcut</th>
              <th>Min Stok</th>
              <th>Doluluk</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in criticalStock" :key="item.productId">
              <td><strong>{{ item.productCode }}</strong></td>
              <td>{{ item.productName }}</td>
              <td>{{ item.unitName }}</td>
              <td class="qty-cell">{{ item.currentQty }}</td>
              <td>{{ item.minStock }}</td>
              <td>
                <div class="fill-bar">
                  <div
                    class="fill-fill"
                    :class="getFillClass(item.fillRatio)"
                    :style="{ width: `${Math.min(item.fillRatio * 100, 100)}%` }"
                  ></div>
                  <span class="fill-label">{{ (item.fillRatio * 100).toFixed(0) }}%</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- Warehouse Fill -->
    <section class="section">
      <h2 class="section-title">Depo Doluluk</h2>
      <div v-if="warehouseFills.length > 0" class="table-container">
        <table>
          <thead>
            <tr>
              <th>Depo</th>
              <th>Konum</th>
              <th>Kapasite</th>
              <th>Doluluk</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in warehouseFills" :key="item.locationId">
              <td>{{ item.warehouseName }}</td>
              <td>{{ item.locationCode }} - {{ item.locationName }}</td>
              <td>{{ item.capacity ? `${item.filledQuantity} / ${item.capacity}` : `${item.filledQuantity}` }}</td>
              <td>
                <div class="fill-bar">
                  <div
                    v-if="item.fillPercent != null"
                    class="fill-fill"
                    :class="getFillClass(item.fillPercent)"
                    :style="{ width: `${item.fillPercent * 100}%` }"
                  ></div>
                  <span v-if="item.fillPercent != null" class="fill-label">{{ (item.fillPercent * 100).toFixed(0) }}%</span>
                  <span v-else class="fill-label">-</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else class="empty-state">
        Henüz depo konumu yok.
      </div>
    </section>

    <!-- Recent Movements -->
    <section class="section">
      <h2 class="section-title">Son Hareketler</h2>
      <div v-if="recentMovements.length > 0" class="table-container">
        <table>
          <thead>
            <tr>
              <th>Zaman</th>
              <th>Tip</th>
              <th>Ürün</th>
              <th>Lot</th>
              <th>Depo</th>
              <th>Miktar</th>
              <th>Not</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in recentMovements" :key="m.id">
              <td class="time-cell">{{ formatDateTime(m.occurredAt) }}</td>
              <td><span :class="movementTypeClass(m.type)">{{ movementTypeLabel(m.type) }}</span></td>
              <td>{{ m.productName }}</td>
              <td>{{ m.lotNumber || '-' }}</td>
              <td>{{ m.warehouseName || '-' }}</td>
              <td class="qty-cell">{{ m.quantity }}</td>
              <td class="note-cell">{{ m.note || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else class="empty-state">
        Henüz hareket yok.
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  getCriticalStock,
  getWarehouseFill,
  getRecentMovements,
  searchLots,
} from '../api/client'
import type {
  CriticalStockItem,
  WarehouseFillItem,
  RecentMovementDto,
  LotSearchItem,
} from '../types'

const criticalStock = ref<CriticalStockItem[]>([])
const warehouseFills = ref<WarehouseFillItem[]>([])
const recentMovements = ref<RecentMovementDto[]>([])
const lotQuery = ref('')
const lotSearchResults = ref<LotSearchItem[]>([])

const fetchAll = async () => {
  try {
    const [stock, fills, movements] = await Promise.all([
      getCriticalStock(),
      getWarehouseFill(),
      getRecentMovements(20),
    ])
    criticalStock.value = stock
    warehouseFills.value = fills
    recentMovements.value = movements
  } catch {
    // ignore — show empty state
  }
}

const handleLotSearch = async () => {
  if (!lotQuery.value.trim()) {
    lotSearchResults.value = []
    return
  }
  try {
    lotSearchResults.value = await searchLots(lotQuery.value.trim())
  } catch {
    lotSearchResults.value = []
  }
}

// Formatting helpers
const formatDateTime = (d: string) => {
  const date = new Date(d)
  return date.toLocaleString('tr-TR', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const formatDate = (d: string) => {
  const date = new Date(d)
  return date.toLocaleDateString('tr-TR')
}

const qualityStatusLabel = (s: string) =>
  ({ OK: 'Tamam', Quarantine: 'Karantina', Rejected: 'Red' })[s] ?? s

const qualityStatusClass = (s: string) =>
  s === 'OK' ? 'status-ok' : s === 'Quarantine' ? 'status-warning' : 'status-error'

const movementTypeLabel = (t: string) => {
  const labels: Record<string, string> = {
    GoodsReceipt: 'Mal Kabul',
    Shipment: 'Sevkiyat',
    TransferIn: 'Transfer (Giriş)',
    TransferOut: 'Transfer (Çıkış)',
    MachineLoad: 'Makine Yükle',
    MachineUnload: 'Makine İndir',
    ProductionConsumption: 'Üretim Sarfiyat',
    ProductionOutput: 'Üretim Çıkış',
    InventoryAdjustmentIn: 'Sayım (Artı)',
    InventoryAdjustmentOut: 'Sayım (Eksi)',
    Scrap: 'Fire',
    CustomerReturn: 'Müşteri İadesi',
    SupplierReturn: 'Tedarikçi İadesi',
    Reversal: 'Geri Hareket',
  }
  return labels[t] ?? t
}

const movementTypeClass = (t: string) => {
  const isOutbound = ['Shipment', 'TransferOut', 'MachineUnload', 'ProductionConsumption', 'Scrap', 'InventoryAdjustmentOut'].includes(t)
  return isOutbound ? 'movement-out' : 'movement-in'
}

const getFillClass = (ratio: number) => {
  if (ratio >= 0.8) return 'fill-high'
  if (ratio >= 0.5) return 'fill-mid'
  return 'fill-low'
}

onMounted(fetchAll)
</script>

<style scoped>
.dashboard {
  max-width: 1200px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 1.25rem;
  border: 1px solid #e5e7eb;
  text-align: center;
}

.stat-card.critical {
  border: 2px solid #dc2626;
  background: #fef2f2;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: #2563eb;
}

.stat-card.critical .stat-value {
  color: #dc2626;
}

.stat-label {
  font-size: 0.875rem;
  color: #666;
  margin-top: 0.25rem;
}

.section {
  margin-top: 1.5rem;
}

.section-title {
  font-size: 1.125rem;
  margin-bottom: 0.75rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.badge-red {
  background: #dc2626;
  color: white;
  font-size: 0.75rem;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: 600;
}

/* Table styles */
.table-container {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th {
  background: #f9fafb;
  padding: 0.75rem 1rem;
  text-align: left;
  font-size: 0.8125rem;
  font-weight: 600;
  color: #666;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  border-bottom: 1px solid #e5e7eb;
}

td {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #f3f4f6;
  font-size: 0.875rem;
}

tr:hover {
  background: #f9fafb;
}

.qty-cell {
  font-weight: 600;
  text-align: right;
}

.time-cell {
  white-space: nowrap;
  color: #666;
  font-size: 0.8125rem;
}

.note-cell {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: #666;
}

/* Fill bar */
.fill-bar {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  width: 140px;
}

.fill-fill {
  height: 8px;
  border-radius: 4px;
  min-width: 2px;
  transition: width 0.3s;
}

.fill-fill.fill-low {
  background: #22c55e;
}

.fill-fill.fill-mid {
  background: #f59e0b;
}

.fill-fill.fill-high {
  background: #ef4444;
}

.fill-label {
  font-size: 0.75rem;
  font-weight: 600;
  white-space: nowrap;
  min-width: 36px;
  text-align: right;
}

/* Status colors */
.status-ok {
  color: #16a34a;
  font-weight: 500;
}

.status-warning {
  color: #d97706;
  font-weight: 500;
}

.status-error {
  color: #dc2626;
  font-weight: 500;
}

.movement-in {
  color: #16a34a;
  font-weight: 500;
}

.movement-out {
  color: #dc2626;
  font-weight: 500;
}

/* Search */
.search-section {
  margin-bottom: 1.5rem;
}

.search-bar {
  display: flex;
  gap: 0.75rem;
}

.search-input {
  flex: 1;
  padding: 0.625rem 1rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.875rem;
  outline: none;
  transition: border-color 0.2s;
}

.search-input:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.btn-primary {
  padding: 0.625rem 1.5rem;
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-primary:hover {
  background: #1d4ed8;
}

.lot-results {
  margin-top: 0.75rem;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  overflow: auto;
}

/* Empty state */
.empty-state {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  color: #999;
  font-size: 0.875rem;
}

.empty-state a {
  color: #2563eb;
}
</style>
