<template>
  <div>
    <div class="header-row">
      <h1>Lot Listesi</h1>
    </div>

    <div style="margin-bottom: 1rem; display: flex; gap: 0.5rem; align-items: center; flex-wrap: wrap;">
      <input v-model="lotNumberFilter" placeholder="Lot Numarasi Ara..." style="padding: 0.5rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.875rem; min-width: 200px;" />
      <select v-model="qualityStatusFilter" style="padding: 0.5rem; border: 1px solid #d1d5db; border-radius: 4px; font-size: 0.875rem;">
        <option value="">Tum Kalite Durumlar</option>
        <option value="OK">Onayli (OK)</option>
        <option value="Quarantine">Karantinada</option>
        <option value="Rejected">Reddedilmis</option>
      </select>
      <button class="btn" style="background:#6b7280;color:white" @click="fetchData">Filtrele</button>
      <button class="btn" style="background:#9ca3af;color:white" @click="resetFilters">Sifirla</button>
    </div>

    <div class="table-container">
      <table v-if="lots.length > 0">
        <thead>
          <tr>
            <th>Lot No</th><th>Urun</th><th>Kaynak</th><th>Kalite Durumu</th><th>Uretim Tarihi</th><th>Olusturulma</th><th>Islem</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="lot in lots" :key="lot.id" @click="router.push(`/lots/${lot.id}`)" style="cursor:pointer">
            <td><strong>{{ lot.lotNumber }}</strong></td>
            <td><code style="font-size:0.8rem">{{ lot.productId.substring(0,8) }}...</code></td>
            <td>{{ sourceLabel(lot.source) }}</td>
            <td>
              <span :class="qualityClass(lot.qualityStatus)">
                {{ qualityLabel(lot.qualityStatus) }}
              </span>
            </td>
            <td>{{ formatDate(lot.productionDate) }}</td>
            <td>{{ formatDate(lot.createdAt) }}</td>
            <td>
              <button class="btn btn-sm" @click.stop="router.push(`/lots/${lot.id}`)">Detay</button>
              <button class="btn btn-sm" style="background:#4f46e5;color:white" @click.stop="router.push(`/lots/${lot.id}/trace`)">Trace</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">Henuz lot yok.</div>
    </div>

    <div v-if="totalPages > 1" style="display:flex; gap:0.5rem; justify-content:center; margin-top:1rem;">
      <button class="btn btn-sm" :disabled="page <= 1" @click="page--">Onceki</button>
      <span style="padding:0.5rem; font-size:0.875rem;">Sayfa {{ page }} / {{ totalPages }}</span>
      <button class="btn btn-sm" :disabled="page >= totalPages" @click="page++">Sonraki</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getLots } from '../../api/client'
import type { Lot } from '../../types'

const router = useRouter()
const lots = ref<Lot[]>([])
const lotNumberFilter = ref('')
const qualityStatusFilter = ref('')
const page = ref(1)
const totalPages = ref(1)

const sourceLabels: Record<string, string> = { Receipt: 'Mal Kabul', Production: 'Uretim' }
const sourceLabel = (s: string) => sourceLabels[s] ?? s

const qualityLabels: Record<string, string> = { OK: 'Onayli', Quarantine: 'Karantinada', Rejected: 'Reddedilmis' }
const qualityLabel = (s: string) => qualityLabels[s] ?? s
const qualityClass = (s: string) => s === 'OK' ? 'quality-ok' : s === 'Quarantine' ? 'quality-quarantine' : 'quality-rejected'

const formatDate = (iso: string) => new Date(iso).toLocaleDateString('tr-TR')

const fetchData = async () => {
  try {
    const params: any = { page: page.value, pageSize: 20 }
    if (lotNumberFilter.value) params.lotNumberFilter = lotNumberFilter.value
    if (qualityStatusFilter.value) params.qualityStatus = qualityStatusFilter.value
    const res = await getLots(params)
    lots.value = res.items
    totalPages.value = res.totalPages
  } catch { lots.value = [] }
}

const resetFilters = () => {
  lotNumberFilter.value = ''
  qualityStatusFilter.value = ''
  page.value = 1
  fetchData()
}

onMounted(fetchData)
</script>

<style scoped>
.quality-ok { color: #16a34a; font-weight: 600; }
.quality-quarantine { color: #d97706; font-weight: 600; }
.quality-rejected { color: #dc2626; font-weight: 600; }
</style>
