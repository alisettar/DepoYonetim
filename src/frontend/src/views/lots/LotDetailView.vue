<template>
  <div>
    <div class="header-row">
      <button class="btn" @click="router.push('/lots')">&#8592; Lot Listesi</button>
      <h1>{{ lot?.lotNumber ?? 'Yükleniyor...' }}</h1>
      <span :class="qualityClass(lot?.qualityStatus ?? 'OK')">
        {{ qualityLabel(lot?.qualityStatus ?? '—') }}
      </span>
    </div>

    <div v-if="!lot" class="empty-state">Lot bulunamadi.</div>
    <template v-else>
      <!-- Lot Info Card -->
      <div class="card" style="margin-bottom:1rem;">
        <h3>Lot Bilgileri</h3>
        <div class="info-grid">
          <div class="info-item">
            <strong>Lot No:</strong> {{ lot.lotNumber }}
          </div>
          <div class="info-item">
            <strong>Urun:</strong> <code>{{ lot.productId.substring(0,8) }}...</code>
          </div>
          <div class="info-item">
            <strong>Kaynak:</strong> {{ sourceLabel(lot.source) }}
          </div>
          <div class="info-item">
            <strong>Uretim Tarihi:</strong> {{ formatDate(lot.productionDate) }}
          </div>
          <div class="info-item">
            <strong>En Son Tarih:</strong> {{ lot.expiryDate ? formatDate(lot.expiryDate) : '—' }}
          </div>
          <div class="info-item">
            <strong>Guncel Bakiye:</strong> {{ currentBalance?.quantity ?? 0 }} {{ currentBalance ? '' : '—' }}
          </div>
        </div>
      </div>

      <!-- Quality Status Update -->
      <div class="card" style="margin-bottom:1rem;">
        <h3>Kalite Durumu Guncelle</h3>
        <div style="display:flex; gap:0.5rem; align-items:center;">
          <select v-model="newStatus" style="padding:0.5rem; border:1px solid #d1d5db; border-radius:4px;">
            <option value="OK">Onayli</option>
            <option value="Quarantine">Karantinada</option>
            <option value="Rejected">Reddedilmis</option>
          </select>
          <button class="btn btn-primary btn-sm" @click="handleUpdateStatus">Guncelle</button>
          <span v-if="statusMsg" :class="statusMsgType" style="font-size:0.875rem;">{{ statusMsg }}</span>
        </div>
      </div>

      <!-- Movement History -->
      <div class="card">
        <div class="section-header">
          <h3>Hareket Geçmisi</h3>
          <button class="btn btn-sm btn-primary" @click="router.push(`/lots/${lotId}/trace`)">Tam Trace Zinciri</button>
        </div>

        <div class="table-container">
          <table v-if="movements.length > 0">
            <thead>
              <tr>
                <th>Tarih</th><th>Tip</th><th>Depo</th><th>Lot</th><th>Miktar</th><th>Not</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="m in movements" :key="m.id">
                <td>{{ formatDate(m.occurredAt) }}</td>
                <td><span class="type-badge" :class="typeBadgeClass(m.type)">{{ typeLabel(m.type) }}</span></td>
                <td>{{ m.warehouseId ? m.warehouseId.substring(0,8) + '...' : '—' }}</td>
                <td>{{ m.lotId ? m.lotId.substring(0,8) + '...' : '—' }}</td>
                <td :style="{ color: m.quantity > 0 ? '#16a34a' : '#dc2626', fontWeight: '600' }">
                  {{ m.quantity > 0 ? '+' : '' }}{{ m.quantity }}
                </td>
                <td>{{ m.note ?? '—' }}</td>
              </tr>
            </tbody>
          </table>
          <div v-else class="empty-state">Bu lot ile ilgili hareket yok.</div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getLotMovements, updateLotQualityStatus } from '../../api/client'
import type { LotDetailResponse, Lot } from '../../types'

const router = useRouter()
const route = useRoute()
const lotId = route.params.id as string

const lotData = ref<LotDetailResponse | null>(null)
const lot = ref<Lot | null>(null)
const movements = ref<any[]>([])
const currentBalance = ref<{ quantity: number } | null>(null)
const newStatus = ref('OK')
const statusMsg = ref('')
const statusMsgType = ref('')

const sourceLabels: Record<string, string> = { Receipt: 'Mal Kabul', Production: 'Uretim' }
const sourceLabel = (s: string) => sourceLabels[s] ?? s

const qualityLabels: Record<string, string> = { OK: 'Onayli', Quarantine: 'Karantinada', Rejected: 'Reddedilmis' }
const qualityLabel = (s: string) => qualityLabels[s] ?? s
const qualityClass = (s: string) => s === 'OK' ? 'quality-ok' : s === 'Quarantine' ? 'quality-quarantine' : 'quality-rejected'

const typeLabels: Record<string, string> = {
  GoodsReceipt: 'Mal Kabul',
  Shipment: 'Sevkiyat',
  TransferIn: 'Transfer Giris',
  TransferOut: 'Transfer Cikis',
  InventoryAdjustmentIn: 'Sayim +',
  InventoryAdjustmentOut: 'Sayim -',
  Scrap: 'Fire',
  Reversal: 'Iptal',
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

const fetchDetail = async () => {
  try {
    lotData.value = await getLotMovements(lotId)
    lot.value = lotData.value.lot
    movements.value = lotData.value.movements
    currentBalance.value = lotData.value.currentBalance
  } catch { lotData.value = null }
}

const handleUpdateStatus = async () => {
  statusMsg.value = ''
  try {
    await updateLotQualityStatus(lotId, { qualityStatus: newStatus.value })
    statusMsg.value = 'Kalite durumu guncellendi.'
    statusMsgType.value = 'status-success'
    await fetchDetail()
  } catch (e: any) {
    statusMsg.value = e?.response?.data?.message || 'Guncelleme basarisiz'
    statusMsgType.value = 'status-error'
  }
}

onMounted(fetchDetail)
</script>

<style scoped>
.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 0.5rem;
  margin-top: 0.5rem;
}
.info-item { padding: 0.5rem; background: #f9fafb; border-radius: 4px; font-size: 0.875rem; }

.status-success { color: #16a34a; }
.status-error { color: #dc2626; }

.quality-ok { color: #16a34a; font-weight: 600; }
.quality-quarantine { color: #d97706; font-weight: 600; }
.quality-rejected { color: #dc2626; font-weight: 600; }

.type-badge { padding: 0.15rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 500; }
.badge-receipt    { background: #dcfce7; color: #15803d; }
.badge-shipment   { background: #fee2e2; color: #dc2626; }
.badge-transfer   { background: #dbeafe; color: #1d4ed8; }
.badge-adjustment { background: #fef9c3; color: #854d0e; }
.badge-other      { background: #f3f4f6; color: #374151; }
</style>
