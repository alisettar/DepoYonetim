<template>
  <div>
    <div class="header-row">
      <h1>Ürünler</h1>
      <button class="btn btn-primary" @click="showCreate = true">+ Yeni Ürün</button>
    </div>

    <div class="table-container">
      <table v-if="products.length > 0">
        <thead>
          <tr>
            <th>Kod</th>
            <th>Ad</th>
            <th>Kategori</th>
            <th>Birincil Birim</th>
            <th>Lot</th>
            <th>Min / Max Stok</th>
            <th>Durum</th>
            <th>İşlemler</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in products" :key="p.id">
            <td><strong>{{ p.code }}</strong></td>
            <td>{{ p.name }}</td>
            <td>{{ categoryName(p.categoryId) }}</td>
            <td>{{ unitName(p.primaryUnitId) }}</td>
            <td>
              <span :class="p.lotRequired ? 'badge-lot-required' : 'badge-lot-optional'">
                {{ p.lotRequired ? 'Zorunlu' : 'Opsiyonel' }}
              </span>
            </td>
            <td>
              <span v-if="p.minStock !== null || p.maxStock !== null">
                {{ p.minStock ?? '—' }} / {{ p.maxStock ?? '—' }}
              </span>
              <span v-else>—</span>
            </td>
            <td>
              <span :class="p.status === 'Active' ? 'status-active' : 'status-inactive'">
                {{ p.status === 'Active' ? 'Aktif' : 'Pasif' }}
              </span>
            </td>
            <td>
              <button class="btn btn-sm" style="background:#f59e0b;color:white" @click="showEdit(p)">Düzenle</button>
              <button class="btn btn-sm btn-danger" style="margin-left:0.25rem" @click="handleDelete(p)">Sil</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty-state">Henüz ürün yok. İlk ürünü oluşturun.</div>
    </div>

    <!-- Create Modal -->
    <div class="modal-overlay" v-if="showCreate" @click.self="closeCreate">
      <div class="modal modal-wide">
        <div class="modal-header">
          <h3>Yeni Ürün</h3>
          <button class="modal-close" @click="closeCreate">&times;</button>
        </div>
        <form @submit.prevent="handleCreate">
          <div class="form-row">
            <div class="form-group">
              <label>Ürün Kodu *</label>
              <input v-model="form.code" placeholder="HMD-CELIK-001" required maxlength="50" />
            </div>
            <div class="form-group">
              <label>Ürün Adı *</label>
              <input v-model="form.name" placeholder="Çelik Sac 2mm" required maxlength="200" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Kategori *</label>
              <select v-model="form.categoryId" required>
                <option value="">Seçiniz</option>
                <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>Birincil Birim *</label>
              <select v-model="form.primaryUnitId" required>
                <option value="">Seçiniz</option>
                <option v-for="u in units" :key="u.id" :value="u.id">{{ u.name }} ({{ u.code }})</option>
              </select>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Min Stok</label>
              <input v-model.number="form.minStock" type="number" min="0" step="0.01" placeholder="0" />
            </div>
            <div class="form-group">
              <label>Max Stok</label>
              <input v-model.number="form.maxStock" type="number" min="0" step="0.01" placeholder="0" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Raf Ömrü (gün)</label>
              <input v-model.number="form.shelfLifeDays" type="number" min="1" placeholder="—" />
            </div>
            <div class="form-group" style="display:flex;align-items:center;gap:0.5rem;padding-top:1.5rem">
              <input type="checkbox" id="lotRequired" v-model="form.lotRequired" />
              <label for="lotRequired" style="margin:0">Lot Zorunlu</label>
            </div>
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="closeCreate">İptal</button>
            <button type="submit" class="btn btn-primary">Oluştur</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit Modal -->
    <div class="modal-overlay" v-if="editingProduct" @click.self="editingProduct = null">
      <div class="modal modal-wide">
        <div class="modal-header">
          <h3>Ürün Düzenle</h3>
          <button class="modal-close" @click="editingProduct = null">&times;</button>
        </div>
        <form @submit.prevent="handleUpdate">
          <div class="form-group">
            <label>Ürün Kodu</label>
            <input :value="editingProduct.code" disabled style="background:#f5f5f5" />
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Ürün Adı *</label>
              <input v-model="editingProduct.name" required maxlength="200" />
            </div>
            <div class="form-group">
              <label>Kategori *</label>
              <select v-model="editingProduct.categoryId" required>
                <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
              </select>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Min Stok</label>
              <input v-model.number="editingProduct.minStock" type="number" min="0" step="0.01" />
            </div>
            <div class="form-group">
              <label>Max Stok</label>
              <input v-model.number="editingProduct.maxStock" type="number" min="0" step="0.01" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Raf Ömrü (gün)</label>
              <input v-model.number="editingProduct.shelfLifeDays" type="number" min="1" />
            </div>
            <div class="form-group" style="display:flex;align-items:center;gap:0.5rem;padding-top:1.5rem">
              <input type="checkbox" id="editLotRequired" v-model="editingProduct.lotRequired" />
              <label for="editLotRequired" style="margin:0">Lot Zorunlu</label>
            </div>
          </div>
          <div class="modal-actions">
            <button type="button" class="btn" @click="editingProduct = null">İptal</button>
            <button type="submit" class="btn btn-primary">Kaydet</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getProducts, createProduct, updateProduct, deleteProduct, getUnits, getCategories } from '../../api/client'
import type { Product, Unit, Category } from '../../types'

const products = ref<Product[]>([])
const units = ref<Unit[]>([])
const categories = ref<Category[]>([])
const showCreate = ref(false)
const editingProduct = ref<Product | null>(null)

const defaultForm = () => ({
  code: '', name: '', categoryId: '', primaryUnitId: '',
  lotRequired: false, shelfLifeDays: undefined as number | undefined,
  minStock: undefined as number | undefined, maxStock: undefined as number | undefined
})
const form = ref(defaultForm())

const unitName = (id: string) => units.value.find(u => u.id === id)?.name ?? id
const categoryName = (id: string) => categories.value.find(c => c.id === id)?.name ?? id

const fetchAll = async () => {
  try {
    [products.value, units.value, categories.value] = await Promise.all([
      getProducts(), getUnits(), getCategories()
    ])
  } catch { products.value = [] }
}

const closeCreate = () => { showCreate.value = false; form.value = defaultForm() }

const handleCreate = async () => {
  try {
    await createProduct({
      code: form.value.code,
      name: form.value.name,
      categoryId: form.value.categoryId,
      primaryUnitId: form.value.primaryUnitId,
      lotRequired: form.value.lotRequired,
      shelfLifeDays: form.value.shelfLifeDays || undefined,
      minStock: form.value.minStock ?? undefined,
      maxStock: form.value.maxStock ?? undefined
    })
    closeCreate()
    await fetchAll()
  } catch { alert('Ürün oluşturulamadı') }
}

const showEdit = (p: Product) => { editingProduct.value = { ...p, units: [...p.units] } }

const handleUpdate = async () => {
  if (!editingProduct.value) return
  try {
    await updateProduct(editingProduct.value.id, {
      name: editingProduct.value.name,
      categoryId: editingProduct.value.categoryId,
      lotRequired: editingProduct.value.lotRequired,
      shelfLifeDays: editingProduct.value.shelfLifeDays ?? undefined,
      minStock: editingProduct.value.minStock ?? undefined,
      maxStock: editingProduct.value.maxStock ?? undefined
    })
    editingProduct.value = null
    await fetchAll()
  } catch { alert('Ürün güncellenemedi') }
}

const handleDelete = async (p: Product) => {
  if (!confirm(`"${p.name}" ürününü silmek istediğinizden emin misiniz?`)) return
  try { await deleteProduct(p.id); await fetchAll() } catch { alert('Ürün silinemedi') }
}

onMounted(fetchAll)
</script>

<style scoped>
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.modal-wide {
  width: 600px;
  max-width: 95vw;
}

.badge-lot-required  { background: #fee2e2; color: #dc2626; padding: 0.15rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 500; }
.badge-lot-optional  { background: #f0fdf4; color: #16a34a; padding: 0.15rem 0.5rem; border-radius: 4px; font-size: 0.75rem; font-weight: 500; }
.status-active   { color: #16a34a; font-weight: 500; }
.status-inactive { color: #dc2626; font-weight: 500; }
</style>
