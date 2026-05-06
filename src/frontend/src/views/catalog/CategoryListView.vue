<template>
  <div>
    <div class="header-row">
      <h1>Kategoriler</h1>
      <button class="btn btn-primary" @click="showCreate = true">+ Yeni Kategori</button>
    </div>

    <div class="table-container">
      <table v-if="categories.length > 0">
        <thead>
          <tr>
            <th>Kod</th>
            <th>Ad</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in categories" :key="c.id">
            <td><strong>{{ c.code }}</strong></td>
            <td>{{ c.name }}</td>
            <td>
              <button class="btn btn-sm" style="background:#f59e0b;color:white" @click="showEdit(c)">Düzenle</button>
              <button class="btn btn-sm btn-danger" style="margin-left:0.25rem" @click="handleDelete(c)">Sil</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">Henüz kategori yok. İlk kategoriyi oluşturun.</div>
    </div>

    <div class="modal-overlay" v-if="showCreate" @click.self="closeCreate">
      <div class="modal">
        <div class="modal-header">
          <h3>Yeni Kategori</h3>
          <button class="modal-close" @click="closeCreate">&times;</button>
        </div>
        <form @submit.prevent="handleCreate">
          <div class="form-group">
            <label>Kod *</label>
            <input v-model="form.code" placeholder="HMD" required maxlength="20" />
          </div>
          <div class="form-group">
            <label>Ad *</label>
            <input v-model="form.name" placeholder="Hammadde" required maxlength="100" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="closeCreate">İptal</button>
            <button type="submit" class="btn btn-primary">Oluştur</button>
          </div>
        </form>
      </div>
    </div>

    <div class="modal-overlay" v-if="editingCategory" @click.self="editingCategory = null">
      <div class="modal">
        <div class="modal-header">
          <h3>Kategori Düzenle</h3>
          <button class="modal-close" @click="editingCategory = null">&times;</button>
        </div>
        <form @submit.prevent="handleUpdate">
          <div class="form-group">
            <label>Kod</label>
            <input :value="editingCategory.code" disabled style="background:#f5f5f5" />
          </div>
          <div class="form-group">
            <label>Ad *</label>
            <input v-model="editingCategory.name" required maxlength="100" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="editingCategory = null">İptal</button>
            <button type="submit" class="btn btn-primary">Kaydet</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getCategories, createCategory, updateCategory, deleteCategory } from '../../api/client'
import type { Category } from '../../types'

const categories = ref<Category[]>([])
const showCreate = ref(false)
const editingCategory = ref<Category | null>(null)
const form = ref({ code: '', name: '' })

const fetchCategories = async () => {
  try { categories.value = await getCategories() } catch { categories.value = [] }
}

const closeCreate = () => { showCreate.value = false; form.value = { code: '', name: '' } }

const handleCreate = async () => {
  try {
    await createCategory({ code: form.value.code, name: form.value.name })
    closeCreate()
    await fetchCategories()
  } catch { alert('Kategori oluşturulamadı') }
}

const showEdit = (c: Category) => { editingCategory.value = { ...c } }

const handleUpdate = async () => {
  if (!editingCategory.value) return
  try {
    await updateCategory(editingCategory.value.id, { name: editingCategory.value.name })
    editingCategory.value = null
    await fetchCategories()
  } catch { alert('Kategori güncellenemedi') }
}

const handleDelete = async (c: Category) => {
  if (!confirm(`"${c.name}" kategorisini silmek istediğinizden emin misiniz?`)) return
  try { await deleteCategory(c.id); await fetchCategories() } catch { alert('Kategori silinemedi') }
}

onMounted(fetchCategories)
</script>
