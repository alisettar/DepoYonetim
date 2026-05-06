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
            <th>Adres</th>
            <th>Konumlar</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="wh in warehouses" :key="wh.id">
            <td><strong>{{ wh.code }}</strong></td>
            <td>{{ wh.name }}</td>
            <td>{{ wh.address || '-' }}</td>
            <td>
              <span v-if="wh.locations.length > 0">
                <span
                  v-for="loc in wh.locations.slice(0, 4)"
                  :key="loc.id"
                  :class="['location-tag', !loc.isActive ? 'inactive' : '']"
                >
                  {{ loc.fullName }}
                </span>
                <span v-if="wh.locations.length > 4" class="location-tag">+{{ wh.locations.length - 4 }} daha</span>
              </span>
              <span v-else class="empty-state" style="padding: 0.5rem">Konum yok</span>
            </td>
            <td>
              <button class="btn btn-sm" style="background:#f59e0b;color:white" @click="showEdit(wh)">Düzenle</button>
              <button class="btn btn-sm btn-danger" @click="handleDelete(wh)" style="margin-left:0.25rem">Sil</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">
        Henüz depo yok. İlk depoyu oluşturun.
      </div>
    </div>

    <!-- Create Modal -->
    <div class="modal-overlay" v-if="showCreate" @click.self="showCreate = false">
      <div class="modal">
        <div class="modal-header">
          <h3>Yeni Depo Oluştur</h3>
          <button class="modal-close" @click="showCreate = false">&times;</button>
        </div>
        <form @submit.prevent="handleCreate">
          <div class="form-group">
            <label>Depo Kodu *</label>
            <input v-model="form.code" placeholder="ANA-DEPO" required />
          </div>
          <div class="form-group">
            <label>Depo Adı *</label>
            <input v-model="form.name" placeholder="Ana Depo" required />
          </div>
          <div class="form-group">
            <label>Adres</label>
            <input v-model="form.address" placeholder="İstanbul" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="showCreate = false">İptal</button>
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
import type { Warehouse, CreateWarehouseRequest } from '../../types'

const warehouses = ref<Warehouse[]>([])
const showCreate = ref(false)
const editingWarehouse = ref<Warehouse | null>(null)

const fetchWarehouses = async () => {
  try {
    warehouses.value = await getWarehouses()
  } catch {
    // Ignore errors - will show empty state
  }
}

const form = ref({ code: '', name: '', address: '' })

const handleCreate = async () => {
  try {
    const data: CreateWarehouseRequest = {
      code: form.value.code,
      name: form.value.name,
      address: form.value.address || undefined
    }
    await createWarehouse(data)
    showCreate.value = false
    form.value = { code: '', name: '', address: '' }
    await fetchWarehouses()
  } catch (err) {
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
