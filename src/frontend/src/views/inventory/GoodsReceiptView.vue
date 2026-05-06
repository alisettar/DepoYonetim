<template>
  <div>
    <div class="header-row">
      <h1>Mal Kabul</h1>
      <button class="btn" style="background:#6b7280;color:white" @click="router.push('/inventory/movements')">← Geri</button>
    </div>

    <div style="background:white;border-radius:8px;padding:1.5rem;box-shadow:0 1px 3px rgba(0,0,0,0.1)">
      <div v-for="(item, idx) in items" :key="idx" class="item-card">
        <div class="item-header">
          <strong>Kalem {{ idx + 1 }}</strong>
          <button v-if="items.length > 1" class="btn btn-sm btn-danger" @click="removeItem(idx)">Sil</button>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Ürün *</label>
            <select v-model="item.productId" required @change="onProductChange(idx)">
              <option value="">Seçiniz</option>
              <option v-for="p in products" :key="p.id" :value="p.id">{{ p.code }} — {{ p.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Lot Numarası</label>
            <input v-model="item.lotNumber" placeholder="L-2024-001" maxlength="50" />
          </div>
          <div class="form-group">
            <label>Üretim Tarihi</label>
            <input type="date" v-model="item.productionDate" />
          </div>
          <div class="form-group">
            <label>Son Kullanma Tarihi</label>
            <input type="date" v-model="item.expiryDate" />
          </div>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>Reçete</label>
            <select v-model="item.recipeId">
              <option value="">Seçiniz</option>
              <option v-for="r in recipes" :key="r.id" :value="r.id">
                {{ r.name }} ({{ productLabel(r.productId) }})
              </option>
            </select>
          </div>
        </div>
          <div class="form-group">
            <label>Miktar *</label>
            <input type="number" v-model.number="item.quantity" min="0.001" step="0.001" required />
          </div>
          <div class="form-group">
            <label>Birim *</label>
            <select v-model="item.unitId" required>
              <option value="">Seçiniz</option>
              <option v-for="u in units" :key="u.id" :value="u.id">{{ u.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Birim Maliyet</label>
            <input type="number" v-model.number="item.unitCost" min="0" step="0.01" placeholder="0.00" />
          </div>
          <div class="form-group">
            <label>Depo *</label>
            <select v-model="item.warehouseId" required>
              <option value="">Seçiniz</option>
              <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
            </select>
          </div>
        </div>
      </div>

      <button class="btn" style="background:#e5e7eb;color:#374151;margin-bottom:1rem" @click="addItem">+ Kalem Ekle</button>

      <div class="form-group">
        <label>Not</label>
        <textarea v-model="note" rows="2" placeholder="Opsiyonel not..." style="width:100%;padding:0.5rem;border:1px solid #d1d5db;border-radius:4px;font-size:0.875rem;resize:vertical"></textarea>
      </div>

      <div style="display:flex;justify-content:flex-end;gap:0.5rem;margin-top:1rem">
        <button class="btn" style="background:#e5e7eb;color:#374151" @click="router.push('/inventory/movements')">İptal</button>
        <button class="btn btn-primary" @click="handleSubmit" :disabled="saving">
          {{ saving ? 'Kaydediliyor...' : 'Kaydet' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getProducts, getUnits, getWarehouses, getRecipes, createGoodsReceipt } from '../../api/client'
import type { Product, Unit, Warehouse, RecipeSummary } from '../../types'

const router = useRouter()
const products = ref<Product[]>([])
const units = ref<Unit[]>([])
const recipes = ref<RecipeSummary[]>([])
const warehouses = ref<Warehouse[]>([])
const saving = ref(false)
const note = ref('')

interface ItemForm {
  productId: string
  lotNumber: string
  productionDate: string
  expiryDate: string
  recipeId: string
  warehouseId: string
  quantity: number | null
  unitId: string
  unitCost: number | null
}

const emptyItem = (): ItemForm => ({
  productId: '', lotNumber: '', productionDate: '', expiryDate: '',
  recipeId: '', warehouseId: '', quantity: null, unitId: '', unitCost: null
})

const items = ref<ItemForm[]>([emptyItem()])

const productLotRequired = (productId: string) =>
  products.value.find(p => p.id === productId)?.lotRequired ?? false

const onProductChange = (idx: number) => {
  items.value[idx].lotNumber = ''
  items.value[idx].productionDate = ''
  items.value[idx].expiryDate = ''
  items.value[idx].recipeId = ''
}

const productLabel = (id: string) => products.value.find(p => p.id === id)?.code ?? id

const addItem = () => items.value.push(emptyItem())
const removeItem = (idx: number) => items.value.splice(idx, 1)

const handleSubmit = async () => {
  for (const item of items.value) {
    if (!item.productId || !item.warehouseId || !item.quantity || !item.unitId) {
      alert('Lütfen tüm zorunlu alanları doldurun.')
      return
    }
    if (productLotRequired(item.productId) && !item.lotNumber) {
      alert('Seçili ürün için lot numarası zorunludur.')
      return
    }
  }

  saving.value = true
  try {
    await createGoodsReceipt({
      items: items.value.map(i => ({
        productId: i.productId,
        lotNumber: i.lotNumber || undefined,
        productionDate: i.productionDate || undefined,
        expiryDate: i.expiryDate || undefined,
        recipeId: i.recipeId || undefined,
        warehouseId: i.warehouseId,
        quantity: i.quantity!,
        unitId: i.unitId,
        unitCost: i.unitCost ?? undefined,
      })),
      note: note.value || undefined,
    })
    alert('Mal kabul başarıyla kaydedildi.')
    router.push('/inventory/movements')
  } catch (err: any) {
    alert(err?.response?.data?.message ?? 'Mal kabul kaydedilemedi.')
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  try {
    [products.value, units.value] = await Promise.all([getProducts(), getUnits()])
    const allWarehouses = await getWarehouses()
    warehouses.value = allWarehouses.filter(w => w.type === 'Physical' || w.type === 'Virtual')
  } catch { /* ignore */ }
})
</script>

<style scoped>
.form-row {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 0.5rem;
}
.item-card {
  border: 1px solid #e5e7eb;
  border-radius: 6px;
  padding: 1rem;
  margin-bottom: 1rem;
}
.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}
</style>
