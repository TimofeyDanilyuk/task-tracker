<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <div class="modal-header">
        <h2>Управление этапами</h2>
        <button class="btn btn-ghost" @click="$emit('close')">✕</button>
      </div>

      <div class="stages-list">
        <div v-for="stage in stages" :key="stage.id" class="stage-row">
          <span class="stage-color-dot" :style="`background:${stage.color}`"></span>
          <span class="stage-name-text">{{ stage.name }}</span>
          <span v-if="stage.isFinal" class="final-badge">завершающий</span>
          <button class="icon-btn" @click="openEdit(stage)">⚙</button>
          <button class="icon-btn danger" @click="remove(stage.id)">✕</button>
        </div>
        <div v-if="stages.length === 0" class="no-stages">Этапов пока нет</div>
      </div>

      <div class="add-stage-row">
        <input v-model="newName" placeholder="Название этапа..." @keyup.enter="add" />
        <input type="color" v-model="newColor" class="color-picker" />
        <button class="btn btn-primary" @click="add">+ Добавить</button>
      </div>
    </div>
  </div>

  <!-- Модалка редактирования этапа -->
  <div class="modal-overlay" v-if="editingStage" @click.self="editingStage = null">
    <div class="modal">
      <div class="modal-header">
        <h2>Редактировать этап</h2>
        <button class="btn btn-ghost" @click="editingStage = null">✕</button>
      </div>
      <div class="form-group">
        <label>Название</label>
        <input v-model="editForm.name" />
      </div>
      <div class="form-group">
        <label>Цвет</label>
        <input type="color" v-model="editForm.color" class="color-picker-lg" />
      </div>
      <div class="form-group">
        <label class="checkbox-label">
          <input type="checkbox" v-model="editForm.isFinal" />
          Завершающий этап
        </label>
        <p class="hint">Задачи, перемещённые в этот этап, будут автоматически завершены</p>
      </div>
      <div class="modal-actions">
        <button class="btn btn-ghost" @click="editingStage = null">Отмена</button>
        <button class="btn btn-primary" @click="saveEdit">Сохранить</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useStagesStore } from '@/stores/stages'

const props = defineProps({ stages: Array })
const emit = defineEmits(['close', 'updated'])
const store = useStagesStore()

const newName  = ref('')
const newColor = ref('#5b7fff')
const editingStage = ref(null)
const editForm = reactive({ name: '', color: '#5b7fff', isFinal: false })

function openEdit(stage) {
  editingStage.value = stage
  editForm.name = stage.name
  editForm.color = stage.color
  editForm.isFinal = stage.isFinal ?? false
}

async function saveEdit() {
  await store.update(editingStage.value.id, { ...editForm })
  editingStage.value = null
  emit('updated')
}

async function add() {
  if (!newName.value.trim()) return
  await store.create({ name: newName.value, color: newColor.value, isFinal: false })
  newName.value = ''
  emit('updated')
}

async function remove(id) {
  await store.remove(id)
  emit('updated')
}
</script>

<style scoped>
.stages-list { display: flex; flex-direction: column; gap: 10px; margin-bottom: 20px; min-height: 40px; }
.stage-row { display: flex; align-items: center; gap: 10px; background: var(--surface2); padding: 10px 14px; border-radius: 10px; border: 1px solid var(--border); }
.stage-color-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.stage-name-text { flex: 1; font-size: 14px; }
.final-badge { font-size: 10px; color: var(--muted); background: var(--surface); border: 1px solid var(--border); padding: 2px 8px; border-radius: 99px; white-space: nowrap; }
.icon-btn { background: none; border: none; color: var(--muted); cursor: pointer; font-size: 14px; padding: 4px 6px; border-radius: 6px; transition: all .2s; }
.icon-btn:hover { color: var(--text); background: var(--surface); }
.icon-btn.danger:hover { color: var(--danger); background: rgba(248,113,113,.1); }
.add-stage-row { display: flex; gap: 10px; align-items: center; }
.no-stages { color: var(--muted); font-size: 13px; text-align: center; padding: 16px; }
.color-picker { width: 36px; height: 36px; border-radius: 8px; border: 1px solid var(--border); padding: 2px; cursor: pointer; background: var(--surface2); }
.color-picker-lg { width: 60px; height: 40px; border-radius: 8px; border: 1px solid var(--border); padding: 2px; cursor: pointer; background: var(--surface2); }
.checkbox-label { display: flex; align-items: center; gap: 8px; font-size: 14px; color: var(--text); cursor: pointer; margin-bottom: 4px; }
.checkbox-label input[type="checkbox"] { width: 16px; height: 16px; cursor: pointer; accent-color: var(--accent); }
.hint { font-size: 12px; color: var(--muted); margin: 0; }
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 24px; }
</style>