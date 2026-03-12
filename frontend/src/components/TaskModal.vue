<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <div class="modal-header">
        <h2>Новая задача</h2>
        <button class="btn btn-ghost" @click="$emit('close')">✕</button>
      </div>

      <div class="form-group">
        <label>Название *</label>
        <input v-model="form.title" placeholder="Что нужно сделать?" />
      </div>
      <div class="form-group">
        <label>Описание</label>
        <textarea v-model="form.description" rows="3" placeholder="Подробности..."></textarea>
      </div>
      <div class="form-row">
        <div class="form-group">
          <label>Приоритет</label>
          <select v-model="form.priority">
            <option v-for="(p, i) in priorities" :key="i" :value="i+1">{{ p }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Этап</label>
          <select v-model="form.stageId">
            <option :value="null">— Без этапа —</option>
            <option v-for="s in stages" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
        </div>
      </div>
      <div class="form-group" v-if="members?.length">
        <label>Ответственный</label>
        <select v-model="form.assignedUserId">
          <option :value="null">— Не назначен —</option>
          <option v-for="m in members" :key="m.userId" :value="m.userId">{{ m.username }}</option>
        </select>
      </div>
      <div class="form-group">
        <label>Дедлайн</label>
        <input type="date" v-model="form.dueDate" />
      </div>

      <div class="modal-actions">
        <button class="btn btn-ghost" @click="$emit('close')">Отмена</button>
        <button class="btn btn-primary" :disabled="!form.title" @click="submit">
          Создать задачу
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue'
import { useTasksStore } from '@/stores/tasks'
import api from '@/api'

const props = defineProps({
  stages: Array,
  boardId: { type: Number, default: null },
  members: { type: Array, default: () => [] }
})
const emit = defineEmits(['close', 'created'])
const store = useTasksStore()

const priorities = ['Критичный', 'Высокий', 'Средний', 'Низкий', 'Минимум']

const form = reactive({
  title: '', description: '', priority: 3,
  stageId: null, dueDate: null, assignedUserId: null
})

async function submit() {
  if (!form.title.trim()) return
  if (props.boardId) {
    await api.post('/tasks', {
      ...form,
      boardId: props.boardId,
      dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null
    })
  } else {
    await store.create({
      ...form,
      dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null
    })
  }
  emit('created')
}
</script>

<style scoped>
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 24px; }
textarea { resize: vertical; min-height: 80px; }
</style>