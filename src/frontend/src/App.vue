<template>
  <div id="app">
    <nav class="sidebar">
      <div class="sidebar-header">
        <h2>WMS</h2>
      </div>
      <ul class="nav-links">
        <li>
          <router-link to="/">Dashboard</router-link>
        </li>
        <li class="nav-section">Depo</li>
        <li>
          <router-link to="/warehouses">Depolar</router-link>
        </li>
        <li class="nav-section">Katalog</li>
        <li>
          <router-link to="/products">Ürünler</router-link>
        </li>
        <li>
          <router-link to="/categories">Kategoriler</router-link>
        </li>
        <li>
          <router-link to="/units">Birimler</router-link>
        </li>
        <li class="nav-section">Envanter</li>
        <li>
          <router-link to="/inventory/movements">Hareketler</router-link>
        </li>
        <li>
          <router-link to="/inventory/goods-receipt">Mal Kabul</router-link>
        </li>
      </ul>
    </nav>
    <main class="content">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: #f5f5f5;
  color: #333;
}

#app {
  display: flex;
  min-height: 100vh;
}

.sidebar {
  width: 200px;
  background: #1a1a2e;
  color: white;
  padding: 1rem 0;
  flex-shrink: 0;
}

.sidebar-header {
  padding: 0 1rem 1rem;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.sidebar-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
}

.nav-links {
  list-style: none;
  padding: 0;
}

.nav-links li a {
  display: block;
  padding: 0.75rem 1rem;
  color: #a0a0a0;
  text-decoration: none;
  transition: color 0.2s, background 0.2s;
}

.nav-links li a:hover,
.nav-links li a.router-link-active {
  color: white;
  background: rgba(255,255,255,0.1);
}

.content {
  flex: 1;
  padding: 1.5rem 2rem;
  overflow-y: auto;
}

h1 {
  margin-bottom: 1rem;
  font-size: 1.5rem;
  font-weight: 600;
}

/* Table styles */
.table-container {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 0.75rem 1rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  background: #fafafa;
  font-weight: 600;
  font-size: 0.875rem;
  color: #666;
}

tr:hover {
  background: #fafafa;
}

/* Button styles */
.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  transition: opacity 0.2s;
}

.btn:hover {
  opacity: 0.8;
}

.btn-primary {
  background: #2563eb;
  color: white;
}

.btn-danger {
  background: #dc2626;
  color: white;
}

.btn-sm {
  padding: 0.25rem 0.75rem;
  font-size: 0.8rem;
}

/* Form styles */
.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 500;
  font-size: 0.875rem;
}

.nav-section {
  padding: 0.5rem 1rem 0.25rem;
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #555;
  font-weight: 600;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  font-size: 0.875rem;
}

.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37,99,235,0.1);
}

/* Modal styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.modal {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  width: 480px;
  max-width: 90vw;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.modal-header h3 {
  font-size: 1.125rem;
}

.modal-close {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  color: #999;
  padding: 0;
  line-height: 1;
}

.modal-close:hover {
  color: #333;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1rem;
}

/* Location tag */
.location-tag {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  background: #e0e7ff;
  color: #3730a3;
  border-radius: 9999px;
  font-size: 0.75rem;
  margin-right: 0.25rem;
  margin-bottom: 0.25rem;
}

.location-tag.inactive {
  background: #f3f4f6;
  color: #9ca3af;
}

.empty-state {
  text-align: center;
  padding: 2rem;
  color: #999;
}

.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}
</style>
