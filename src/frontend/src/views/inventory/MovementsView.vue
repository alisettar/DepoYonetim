<template>
  <div>
    <div class="header-row">
      <h1>Stok Hareketleri</h1>
      <button class="btn btn-primary" @click="router.push('/inventory/goods-receipt')">+ Mal Kabul</button>
    </div>

    <div style="margin-bottom: 1rem; display: flex; gap: 0.5rem; align-items: center;">
      <select v-model="filterType" style="padding: 0.5rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.875rem;">
        <option value="">Tüm Tipler</option>
        <option value="GoodsReceipt">Mal Kabul</option>
        <option value="Shipment">Sevkiyat</option>
        <option value="TransferIn">Transfer Giriş</option>
        <option value="TransferOut">Transfer Çıkış</option>
        <option value="InventoryAdjustmentIn">Sayım +</option>
        <option value="InventoryAdjustmentOut">Sayım -</option>
      </select>
      <button class="btn" style="background:#6b7280;color:white" @click="fetchData">Filtrele</button>
    </div>

    <div class="table-container">
      <table v-if="movements.length > 0">
        <thead>
          <tr>
            <th>Tarih</th><th>Tip</th><th>Ürün</th><th>Lot</th><th>Depo</th><th>Miktar</th><th>Not</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in movements" :key="m.id">
            <td>{{ formatDate(m.occurredAt) }}</td>
            <td><span class="type-badge" :class="typeBadgeClass(m.type)">{{ typeLabel(m.type) }}</span></td>
            <td><code style="font-size:0.8rem">{{ m.productId.substring(0,8) }}...</code></td>
            <td>{{ m.lotId ? m.lotId.substring(0,8) + '...' : '—' }}</td>
            <td>{{ m.warehouseId ? m.warehouseId.substring(0,8) + '...' : '—' }}</td>
            <td :style="{ color: m.quantity > 0 ? '#16a34a' : '#dc2626', fontWeight: '600' }">
              {{ m.quantity > 0 ? '+' : '' }}{{ m.quantity }}
            </td>
            <td>{{ m.note ?? '—' }}</td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">Henüz stok hareketi yok.</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMovements } from '../../api/client'
import type { StockMovement } from '../../types'

const router = useRouter()
const movements = ref<StockMovement[]>([])
const filterType = ref('')

const typeLabels: Record<string, string> = {
  GoodsReceipt: 'Mal Kabul',
  Shipment: 'Sevkiyat',
  TransferIn: 'Transfer Giriş',
  TransferOut: 'Transfer Çıkış',
  InventoryAdjustmentIn: 'Sayım +',
  InventoryAdjustmentOut: 'Sayım -',
  Scrap: 'Fire',
  Reversal: 'İptal',
}
const typeLabel = (t: string) => typeLabels[t] ?? t

const typeBadgeClass = (t: string) => {
  if (t === 'GoodsReceipt') return 'badge-receipt'
  if (t === 'Shipment') return 'badge-shipment'
  if (t.startsWith('Transfer')) return 'badge-transfer'
  if (t.startsWith('Inventory')) return 'badge-adjustment'
  return 'badge-other'
}

const formatDate = (iso: string) => {
  const d = new Date(iso)
  return d.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })
    + ' ' + d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })
}

const fetchData = async () => {
  try {
    movements.value = await getMovements(filterType.value ? { type: filterType.value } : undefined)
  } catch { movements.value = [] }
}

onMounted(fetchData)
</script>

<style scoped>
.type-badge { padding: 0.15rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 500; }
.badge-receipt    { background: #dcfce7; color: #15803d; }
.badge-shipment   { background: #fee2e2; color: #dc2626; }
.badge-transfer   { background: #dbeafe; color: #1d4ed8; }
.badge-adjustment { background: #fef9c3; color: #854d0e; }
.badge-other      { background: #f3f4f6; color: #374151; }
</style>
