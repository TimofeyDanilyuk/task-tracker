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
          <input v-model="stage.name" class="stage-name-input" @blur="save(stage)" />
          <input type="color" v-model="stage.color" class="color-picker" @change="save(stage)" />
          <button class="btn btn-danger" @click="remove(stage.id)">✕</button>
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
</template>

<script setup>
import { ref } from 'vue'
import { useStagesStore } from '@/stores/stages'

const props = defineProps({ stages: Array })
const emit = defineEmits(['close', 'updated'])
const store = useStagesStore()

const newName  = ref('')
const newColor = ref('#5b7fff')

async function add() {
  if (!newName.value.trim()) return
  await store.create({ name: newName.value, color: newColor.value })
  newName.value = ''
  emit('updated')
}
async function save(stage) {
  await store.update(stage.id, { name: stage.name, color: stage.color })
}
async function remove(id) {
  await store.remove(id)
  emit('updated')
}
</script>

<style scoped>
.stages-list { display: flex; flex-direction: column; gap: 10px; margin-bottom: 20px; min-height: 40px; }
.stage-row {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--surface2);
  padding: 10px 14px;
  border-radius: 10px;
  border: 1px solid var(--border);
}
.stage-color-dot { width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }
.stage-name-input { flex: 1; }
.color-picker {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  border: 1px solid var(--border);
  padding: 2px;
  cursor: pointer;
  background: var(--surface2);
}
.add-stage-row { display: flex; gap: 10px; align-items: center; }
.no-stages { color: var(--muted); font-size: 13px; text-align: center; padding: 16px; }
</style>