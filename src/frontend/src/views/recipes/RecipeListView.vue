<template>
  <div>
    <div class="header-row">
      <h1>Reçeteler</h1>
      <button class="btn btn-primary" @click="showCreate = true">+ Yeni Reçete</button>
    </div>

    <div class="table-container">
      <table v-if="recipes.length > 0">
        <thead>
          <tr>
            <th>Ad</th>
            <th>Ürün</th>
            <th>Durum</th>
            <th>Versiyon</th>
            <th>Oluşturma</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in recipes" :key="r.id">
            <td><strong>{{ r.name }}</strong></td>
            <td>{{ productLabel(r.productId) }}</td>
            <td>
              <span :class="statusClass(r.status)">
                {{ statusLabel(r.status) }}
              </span>
            </td>
            <td>{{ r.versionCount }}</td>
            <td>{{ formatDate(r.createdAt) }}</td>
            <td>
              <button class="btn btn-sm" @click="handleView(r)">Görüntüle</button>
              <button class="btn btn-sm" style="background:#f59e0b;color:white" @click="showEdit(r)">Düzenle</button>
              <button v-if="r.status !== 'Archived'" class="btn btn-sm btn-danger" style="margin-left:0.25rem" @click="handleArchive(r)">Arşivle</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">Henüz reçete yok. İlk reçeteyi oluşturun.</div>
    </div>

    <!-- Create Modal -->
    <div class="modal-overlay" v-if="showCreate" @click.self="closeCreate">
      <div class="modal">
        <div class="modal-header">
          <h3>Yeni Reçete</h3>
          <button class="modal-close" @click="closeCreate">&times;</button>
        </div>
        <form @submit.prevent="handleCreate">
          <div class="form-group">
            <label>Ürün *</label>
            <select v-model="form.productId" required>
              <option value="">Ürün seçiniz</option>
              <option v-for="p in products" :key="p.id" :value="p.id">{{ p.code }} - {{ p.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Reçete Adı *</label>
            <input v-model="form.name" placeholder="Örn: Çelik Sac Kesim Reçetesi" required maxlength="200" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="closeCreate">İptal</button>
            <button type="submit" class="btn btn-primary">Oluştur</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit Modal -->
    <div class="modal-overlay" v-if="editingRecipe" @click.self="editingRecipe = null">
      <div class="modal">
        <div class="modal-header">
          <h3>Reçete Düzenle</h3>
          <button class="modal-close" @click="editingRecipe = null">&times;</button>
        </div>
        <form @submit.prevent="handleUpdate">
          <div class="form-group">
            <label>Reçete Adı *</label>
            <input v-model="editingRecipe.name" required maxlength="200" />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="editingRecipe = null">İptal</button>
            <button type="submit" class="btn btn-primary">Kaydet</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getRecipes, createRecipe, updateRecipe, archiveRecipe, getProducts } from '../../api/client'
import type { RecipeSummary } from '../../types'

const router = useRouter()
const recipes = ref<RecipeSummary[]>([])
const products = ref<{ id: string; code: string; name: string }[]>([])

const showCreate = ref(false)
const editingRecipe = ref<{ id: string; name: string } | null>(null)

const defaultForm = () => ({ productId: '', name: '' })
const form = ref(defaultForm())

const statusLabel = (s: string) => ({ Draft: 'Taslak', Active: 'Aktif', Archived: 'Arşivlenmiş' }[s] ?? s)
const statusClass = (s: string) => s === 'Active' ? 'status-active' : s === 'Archived' ? 'status-inactive' : ''

const productLabel = (id: string) => products.value.find(p => p.id === id)?.code ?? id

const formatDate = (d: string) => new Date(d).toLocaleDateString('tr-TR')

const fetchAll = async () => {
  try {
    [recipes.value, products.value] = await Promise.all([getRecipes(), getProducts()])
  } catch { recipes.value = [] }
}

const closeCreate = () => { showCreate.value = false; form.value = defaultForm() }

const handleCreate = async () => {
  try {
    await createRecipe({ productId: form.value.productId, name: form.value.name })
    closeCreate()
    await fetchAll()
  } catch { alert('Reçete oluşturulamadı') }
}

const showEdit = (r: RecipeSummary) => { editingRecipe.value = { id: r.id, name: r.name } }

const handleUpdate = async () => {
  if (!editingRecipe.value) return
  try {
    await updateRecipe(editingRecipe.value.id, { name: editingRecipe.value.name })
    editingRecipe.value = null
    await fetchAll()
  } catch { alert('Reçete güncellenemedi') }
}

const handleView = (r: RecipeSummary) => { router.push(`/recipes/${r.id}`) }

const handleArchive = async (r: RecipeSummary) => {
  if (!confirm(`"${r.name}" reçetesini arşivlemek istediğinizden emin misiniz?`)) return
  try { await archiveRecipe(r.id); await fetchAll() } catch { alert('Reçete arşivlenemedi') }
}

onMounted(fetchAll)
</script>

<style scoped>
.status-active   { color: #16a34a; font-weight: 500; }
.status-inactive { color: #dc2626; font-weight: 500; }
</style>
