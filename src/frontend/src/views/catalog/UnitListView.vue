<template>
  <div>
    <div class="header-row">
      <h1>Birimler</h1>
      <button class="btn btn-primary" @click="showCreate = true">+ Yeni Birim</button>
    </div>

    <div class="table-container">
      <table v-if="units.length > 0">
        <thead>
          <tr>
            <th>Kod</th>
            <th>Ad</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="u in units" :key="u.id">
            <td><strong>{{ u.code }}</strong></td>
            <td>{{ u.name }}</td>
            <td>
              <button class="btn btn-sm" style="background:#f59e0b;color:white" @click="showEdit(u)">Düzenle</button>
              <button class="btn btn-sm btn-danger" style="margin-left:0.25rem" @click="handleDelete(u)">Sil</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">Henüz birim yok. İlk birimi oluşturun.</div>
    </div>

    <div class="modal-overlay" v-if="showCreate" @click.self="closeCreate">
      <div class="modal">
        <div class="modal-header">
          <h3>Yeni Birim</h3>
          <button class="modal-close" @click="closeCreate">&times;</button>
        </div>
        <form @submit.prevent="handleCreate">
          <div class="form-group">
            <label>Kod *</label>
            <input v-model="form.code" placeholder="ADET" required maxlength="20" />
          </div>
          <div class="form-group">
            <label>Ad *</label>
            <input v-model="form.name" placeholder="Adet" required maxlength="100" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="closeCreate">İptal</button>
            <button type="submit" class="btn btn-primary">Oluştur</button>
          </div>
        </form>
      </div>
    </div>

    <div class="modal-overlay" v-if="editingUnit" @click.self="editingUnit = null">
      <div class="modal">
        <div class="modal-header">
          <h3>Birim Düzenle</h3>
          <button class="modal-close" @click="editingUnit = null">&times;</button>
        </div>
        <form @submit.prevent="handleUpdate">
          <div class="form-group">
            <label>Kod</label>
            <input :value="editingUnit.code" disabled style="background:#f5f5f5" />
          </div>
          <div class="form-group">
            <label>Ad *</label>
            <input v-model="editingUnit.name" required maxlength="100" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="editingUnit = null">İptal</button>
            <button type="submit" class="btn btn-primary">Kaydet</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getUnits, createUnit, updateUnit, deleteUnit } from '../../api/client'
import type { Unit } from '../../types'

const units = ref<Unit[]>([])
const showCreate = ref(false)
const editingUnit = ref<Unit | null>(null)
const form = ref({ code: '', name: '' })

const fetchUnits = async () => {
  try { units.value = await getUnits() } catch { units.value = [] }
}

const closeCreate = () => { showCreate.value = false; form.value = { code: '', name: '' } }

const handleCreate = async () => {
  try {
    await createUnit({ code: form.value.code, name: form.value.name })
    closeCreate()
    await fetchUnits()
  } catch { alert('Birim oluşturulamadı') }
}

const showEdit = (u: Unit) => { editingUnit.value = { ...u } }

const handleUpdate = async () => {
  if (!editingUnit.value) return
  try {
    await updateUnit(editingUnit.value.id, { name: editingUnit.value.name })
    editingUnit.value = null
    await fetchUnits()
  } catch { alert('Birim güncellenemedi') }
}

const handleDelete = async (u: Unit) => {
  if (!confirm(`"${u.name}" birimini silmek istediğinizden emin misiniz?`)) return
  try { await deleteUnit(u.id); await fetchUnits() } catch { alert('Birim silinemedi') }
}

onMounted(fetchUnits)
</script>
