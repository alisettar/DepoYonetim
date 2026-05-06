<template>
  <div>
    <div class="header-row">
      <button class="btn" @click="router.push(`/lots/${lotId}`)">&#8592; Lot Detay</button>
      <h1>Trace Zinciri - {{ lotNumber ?? '' }}</h1>
    </div>

    <div v-if="!lotNumber" class="empty-state">Lot bulunamadi.</div>
    <template v-else>
      <!-- Summary Cards -->
      <div class="summary-cards">
        <div class="summary-card">
          <div class="summary-label">Toplam Alim</div>
          <div class="summary-value positive">{{ totalInbound }} {{ unit }}</div>
        </div>
        <div class="summary-card">
          <div class="summary-label">Toplam Cikis</div>
          <div class="summary-value negative">{{ totalOutbound }}</div>
        </div>
        <div class="summary-card">
          <div class="summary-label">Hareket Sayisi</div>
          <div class="summary-value">{{ movements.length }}</div>
        </div>
        <div class="summary-card">
          <div class="summary-label">Guncel Bakiye</div>
          <div class="summary-value" :class="runningBalance > 0 ? 'positive' : 'negative'">{{ runningBalance }}</div>
        </div>
      </div>

      <!-- Timeline -->
      <div class="card">
        <h3>Timeline</h3>

        <div v-if="movements.length === 0" class="empty-state">Hareket bulunamadi.</div>
        <div v-else class="timeline">
          <div
            v-for="(entry, idx) in movements"
            :key="entry.movement.id"
            :class="['timeline-item', entry.isInbound ? 'inbound' : 'outbound']"
          >
            <div class="timeline-marker">
              <div :class="['dot', entry.isInbound ? 'dot-in' : 'dot-out']"></div>
              <div v-if="idx < movements.length - 1" class="line"></div>
            </div>
            <div class="timeline-content">
              <div class="timeline-header">
                <span :class="['badge', entry.isInbound ? 'badge-in' : 'badge-out']">
                  {{ entry.isInbound ? '+' : '-' }}{{ entry.movement.quantity }}
                </span>
                <strong>{{ typeLabel(entry.movement.type) }}</strong>
                <span class="timeline-date">{{ formatDate(entry.movement.occurredAt) }}</span>
              </div>
              <div class="timeline-details">
                <span>
                  Bakiye: <strong>{{ entry.runningBalance }}</strong>
                  <span v-if="entry.cumulativeConsumed > 0">
                    | Toplam Tuketim: {{ entry.cumulativeConsumed }}
                  </span>
                </span>
                <span v-if="entry.movement.warehouseId">{{ entry.movement.warehouseId.substring(0,8) }}...</span>
                <span v-if="entry.movement.note" style="color:#6b7280">"{{ entry.movement.note }}"</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getLotTrace } from '../../api/client'
import type { TraceChainEntry } from '../../types'

const router = useRouter()
const route = useRoute()
const lotId = route.params.id as string

const movements = ref<TraceChainEntry[]>([])
const lotNumber = ref('')
const unit = ref('')

const typeLabels: Record<string, string> = {
  GoodsReceipt: 'Mal Kabul',
  Shipment: 'Sevkiyat',
  TransferIn: 'Transfer Giris',
  TransferOut: 'Transfer Cikis',
  InventoryAdjustmentIn: 'Sayim +',
  InventoryAdjustmentOut: 'Sayim -',
  MachineLoad: 'Makine Yuki',
  MachineUnload: 'Makine Bosaltma',
  ProductionConsumption: 'Uretim Tüketimi',
  ProductionOutput: 'Uretim Cikisi',
  Scrap: 'Fire',
  Reversal: 'Iptal',
}
const typeLabel = (t: string) => typeLabels[t] ?? t

const formatDate = (iso: string) => {
  const d = new Date(iso)
  return d.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })
    + ' ' + d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })
}

const totalInbound = computed(() =>
  movements.value.filter(e => e.isInbound).reduce((s, e) => s + e.movement.quantity, 0))

const totalOutbound = computed(() =>
  movements.value.filter(e => e.isOutbound).reduce((s, e) => s + Math.abs(e.movement.quantity), 0))

const runningBalance = computed(() => {
  if (!movements.value.length) return 0
  return movements.value[movements.value.length - 1].runningBalance
})

const fetchTrace = async () => {
  try {
    movements.value = await getLotTrace(lotId)
  } catch { movements.value = [] }
}

onMounted(fetchTrace)
</script>

<style scoped>
.summary-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.summary-card {
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  padding: 1rem;
  text-align: center;
}

.summary-label {
  font-size: 0.8rem;
  color: #6b7280;
  margin-bottom: 0.5rem;
}

.summary-value {
  font-size: 1.5rem;
  font-weight: 700;
}

.summary-value.positive { color: #16a34a; }
.summary-value.negative { color: #dc2626; }

/* Timeline */
.timeline {
  position: relative;
  padding-left: 1.5rem;
}

.timeline::before {
  content: '';
  position: absolute;
  left: 7px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #e5e7eb;
}

.timeline-item {
  position: relative;
  padding: 1rem 0;
  border-bottom: 1px solid #f3f4f6;
}

.timeline-marker {
  position: absolute;
  left: -1.5rem;
  top: 1rem;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.dot {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 2px solid;
}

.dot-in { background: #dcfce7; border-color: #16a34a; }
.dot-out { background: #fee2e2; border-color: #dc2626; }

.line {
  width: 2px;
  height: 40px;
  background: #e5e7eb;
}

.timeline-content {
  flex: 1;
}

.timeline-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
}

.badge {
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  font-size: 0.85rem;
  font-weight: 600;
}

.badge-in { background: #dcfce7; color: #15803d; }
.badge-out { background: #fee2e2; color: #dc2626; }

.timeline-date {
  font-size: 0.8rem;
  color: #6b7280;
}

.timeline-details {
  font-size: 0.85rem;
  color: #374151;
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}
</style>