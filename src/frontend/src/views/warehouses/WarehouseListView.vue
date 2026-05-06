<template>
  <div>
    <div class="header-row">
      <h1>Depolar</h1>
      <button class="btn btn-primary" @click="showCreate = true">+ Yeni Depo</button>
    </div>

    <div class="table-container">
      <table v-if="warehouses.length > 0">
        <thead>
          <tr>
            <th>Kod</th>
            <th>Ad</th>
            <th>Tür</th>
            <th>Adres</th>
            <th>Konumlar</th>
            <th>Durum</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="wh in warehouses" :key="wh.id">
            <td><strong>{{ wh.code }}</strong></td>
            <td>
              {{ wh.name }}
              <span v-if="wh.machineCode" style="font-size:0.75rem;color:#666;display:block">
                {{ wh.machineCode }}
              </span>
            </td>
            <td>
              <span :class="['type-badge', `type-${wh.type.toLowerCase()}`]">
                {{ typeLabel(wh.type) }}
              </span>
            </td>
            <td>{{ wh.address || '-' }}</td>
            <td>
              <span v-if="wh.locations.length > 0">
                <span
                  v-for="loc in wh.locations.slice(0, 3)"
                  :key="loc.id"
                  :class="['location-tag', !loc.isActive ? 'inactive' : '']"
                >
                  {{ loc.code }}
                </span>
                <span v-if="wh.locations.length > 3" class="location-tag">
                  +{{ wh.locations.length - 3 }} daha
                </span>
              </span>
              <span v-else class="empty-state" style="padding:0.5rem">—</span>
            </td>
            <td>
              <span :class="wh.status === 'Active' ? 'status-active' : 'status-inactive'">
                {{ wh.status === 'Active' ? 'Aktif' : 'Pasif' }}
              </span>
            </td>
            <td>
              <button class="btn btn-sm" style="background:#f59e0b;color:white" @click="showEdit(wh)">
                Düzenle
              </button>
              <button class="btn btn-sm btn-danger" style="margin-left:0.25rem" @click="handleDelete(wh)">
                Sil
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">
        Henüz depo yok. İlk depoyu oluşturun.
      </div>
    </div>

    <!-- Create Modal -->
    <div class="modal-overlay" v-if="showCreate" @click.self="closeCreate">
      <div class="modal">
        <div class="modal-header">
          <h3>Yeni Depo Oluştur</h3>
          <button class="modal-close" @click="closeCreate">&times;</button>
        </div>
        <form @submit.prevent="handleCreate">
          <div class="form-group">
            <label>Depo Kodu *</label>
            <input v-model="form.code" placeholder="ANA-DEPO" required maxlength="20" />
          </div>
          <div class="form-group">
            <label>Depo Adı *</label>
            <input v-model="form.name" placeholder="Ana Depo" required />
          </div>
          <div class="form-group">
            <label>Tür *</label>
            <select v-model="form.type" required>
              <option value="Physical">Fiziksel</option>
              <option value="Virtual">Sanal</option>
              <option value="Machine">Makine</option>
            </select>
          </div>
          <div class="form-group" v-if="form.type === 'Machine'">
            <label>Makine Kodu *</label>
            <input v-model="form.machineCode" placeholder="TORNA-001" :required="form.type === 'Machine'" />
          </div>
          <div class="form-group">
            <label>Adres</label>
            <input v-model="form.address" placeholder="İstanbul" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="closeCreate">İptal</button>
            <button type="submit" class="btn btn-primary">Oluştur</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit Modal -->
    <div class="modal-overlay" v-if="editingWarehouse" @click.self="editingWarehouse = null">
      <div class="modal">
        <div class="modal-header">
          <h3>Depo Düzenle</h3>
          <button class="modal-close" @click="editingWarehouse = null">&times;</button>
        </div>
        <form @submit.prevent="handleUpdate">
          <div class="form-group">
            <label>Depo Kodu</label>
            <input :value="editingWarehouse.code" disabled style="background:#f5f5f5" />
          </div>
          <div class="form-group">
            <label>Depo Adı *</label>
            <input v-model="editingWarehouse.name" required />
          </div>
          <div class="form-group">
            <label>Adres</label>
            <input v-model="editingWarehouse.address" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="editingWarehouse = null">İptal</button>
            <button type="submit" class="btn btn-primary">Kaydet</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getWarehouses, createWarehouse, updateWarehouse, deleteWarehouse } from '../../api/client'
import type { Warehouse, CreateWarehouseRequest, WarehouseType } from '../../types'

const warehouses = ref<Warehouse[]>([])
const showCreate = ref(false)
const editingWarehouse = ref<Warehouse | null>(null)

const defaultForm = () => ({ code: '', name: '', type: 'Physical' as WarehouseType, address: '', machineCode: '' })
const form = ref(defaultForm())

const typeLabel = (type: WarehouseType) =>
  ({ Physical: 'Fiziksel', Virtual: 'Sanal', Machine: 'Makine' })[type]

const fetchWarehouses = async () => {
  try {
    warehouses.value = await getWarehouses()
  } catch {
    warehouses.value = []
  }
}

const closeCreate = () => {
  showCreate.value = false
  form.value = defaultForm()
}

const handleCreate = async () => {
  try {
    const data: CreateWarehouseRequest = {
      code: form.value.code,
      name: form.value.name,
      type: form.value.type,
      address: form.value.address || undefined,
      machineCode: form.value.type === 'Machine' ? form.value.machineCode : undefined
    }
    await createWarehouse(data)
    closeCreate()
    await fetchWarehouses()
  } catch {
    alert('Depo oluşturulamadı')
  }
}

const showEdit = (wh: Warehouse) => {
  editingWarehouse.value = { ...wh }
}

const handleUpdate = async () => {
  if (!editingWarehouse.value) return
  try {
    await updateWarehouse(editingWarehouse.value.id, {
      name: editingWarehouse.value.name,
      address: editingWarehouse.value.address || undefined
    })
    editingWarehouse.value = null
    await fetchWarehouses()
  } catch {
    alert('Depo güncellenemedi')
  }
}

const handleDelete = async (wh: Warehouse) => {
  if (!confirm(`"${wh.name}" deposunu silmek istediğinizden emin misiniz?`)) return
  try {
    await deleteWarehouse(wh.id)
    await fetchWarehouses()
  } catch {
    alert('Depo silinemedi')
  }
}

onMounted(fetchWarehouses)
</script>

<style scoped>
.type-badge {
  display: inline-block;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 500;
}
.type-physical { background: #dbeafe; color: #1d4ed8; }
.type-virtual  { background: #f3e8ff; color: #7c3aed; }
.type-machine  { background: #fef3c7; color: #b45309; }

.status-active   { color: #16a34a; font-weight: 500; }
.status-inactive { color: #dc2626; font-weight: 500; }
</style>
